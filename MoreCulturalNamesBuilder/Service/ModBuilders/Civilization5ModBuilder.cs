using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.Repositories;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class Civilization5ModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        private static readonly string[] localisationLocales =
        [
            "en_US",
            "DE_DE",
            "ES_ES",
            "FR_FR",
            "IT_IT",
            "JA_JP",
            "KO_KR",
            "PL_PL",
            "RU_RU",
            "ZH_Hant_HK",
        ];

        private IList<string> cityKeys = null!;
        private IList<string> civilizationIds = null!;
        private IDictionary<string, IDictionary<string, Localisation>> localisationsByCityKey = null!;
        private byte[] luaBytes = null!;
        private byte[] localisationBytes = null!;

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = [];

            cityKeys = [.. locationGameIds
                .Where(gameId => !string.IsNullOrWhiteSpace(gameId.Id))
                .Select(gameId => gameId.Id)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(cityKey => cityKey, StringComparer.Ordinal)];

            Parallel.ForEach(cityKeys, cityKey =>
            {
                IDictionary<string, Localisation> cityLocalisations = localisationFetcher
                    .GetGameLocationLocalisations(cityKey, Settings.Mod.Game)
                    .ToDictionary(localisation => localisation.LanguageGameId, localisation => localisation);

                concurrentLocalisations.TryAdd(cityKey, cityLocalisations);
            });

            localisationsByCityKey = concurrentLocalisations;

            civilizationIds = [.. localisationsByCityKey
                .Values
                .SelectMany(localisations => localisations.Keys)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(civilizationId => civilizationId, StringComparer.Ordinal)];
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string luaDirectoryPath = Path.Combine(mainDirectoryPath, "Lua");
            string xmlDirectoryPath = Path.Combine(mainDirectoryPath, "XML");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(luaDirectoryPath);
            Directory.CreateDirectory(xmlDirectoryPath);

            CreateLuaFile(luaDirectoryPath);
            CreateLocalisationFile(xmlDirectoryPath);
            CreateDescriptorFile(mainDirectoryPath);
        }

        private void CreateLuaFile(string luaDirectoryPath)
        {
            string filePath = Path.Combine(luaDirectoryPath, $"{GetFilePrefix()}.lua");

            luaBytes = EncodeContent(GenerateLuaContent());
            File.WriteAllBytes(filePath, luaBytes);
        }

        private void CreateLocalisationFile(string xmlDirectoryPath)
        {
            string filePath = Path.Combine(xmlDirectoryPath, $"{GetFilePrefix()}.xml");

            localisationBytes = EncodeContent(GenerateLocalisationContent());
            File.WriteAllBytes(filePath, localisationBytes);
        }

        private void CreateDescriptorFile(string mainDirectoryPath)
        {
            string filePath = Path.Combine(mainDirectoryPath, $"{Settings.Mod.Id}.modinfo");

            File.WriteAllText(filePath, GenerateDescriptorContent(), new UTF8Encoding(true));
        }

        private string GenerateLocalisationContent()
        {
            string rows = GenerateLocalisationRows();
            StringBuilder builder = new();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.Append(Environment.NewLine);
            builder.Append("<GameData>");
            builder.Append(Environment.NewLine);

            foreach (string locale in localisationLocales)
            {
                builder.Append($"  <Language_{locale}>");
                builder.Append(Environment.NewLine);
                builder.Append(rows);

                if (!string.IsNullOrEmpty(rows))
                {
                    builder.Append(Environment.NewLine);
                }

                builder.Append($"  </Language_{locale}>");
                builder.Append(Environment.NewLine);
            }

            builder.Append("</GameData>");

            return builder.ToString();
        }

        private string GenerateLocalisationRows()
        {
            List<string> rows = [];
            object rowCollectionLock = new();

            Parallel.ForEach(
                cityKeys,
                List<string> () => [],
                (cityKey, _, localRows) =>
                {
                    IDictionary<string, Localisation> cityLocalisations = localisationsByCityKey[cityKey];

                    foreach (string civilizationId in civilizationIds)
                    {
                        if (!cityLocalisations.TryGetValue(civilizationId, out Localisation? localisation) ||
                            localisation is null)
                        {
                            continue;
                        }

                        localRows.Add(GenerateLocalisationRow(cityKey, civilizationId, localisation));
                    }

                    return localRows;
                },
                localRows =>
                {
                    lock (rowCollectionLock)
                    {
                        rows.AddRange(localRows);
                    }
                });

            rows.Sort(StringComparer.Ordinal);

            return string.Join(Environment.NewLine, rows);
        }

        private string GenerateLocalisationRow(
            string cityKey,
            string civilizationId,
            Localisation localisation)
        {
            string localisationKey = $"{cityKey}_{civilizationId}";
            string normalisedName = nameNormaliser.ToCivilization5Charset(localisation.Name);
            StringBuilder builder = new();

            builder.Append($"    <Row Tag=\"{Escape(localisationKey)}\">");
            builder.Append(Environment.NewLine);
            builder.Append($"      <Text>{Escape(normalisedName)}</Text>");
            builder.Append(Environment.NewLine);

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                builder.Append($"      <!-- Language={Escape(localisation.LanguageId)} -->");
                builder.Append(Environment.NewLine);
            }

            builder.Append("    </Row>");

            return builder.ToString();
        }

        private string GenerateLuaContent()
        {
            StringBuilder builder = new();

            builder.Append("-- ");
            builder.Append(Settings.Mod.Name);
            builder.Append(Environment.NewLine);

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                builder.Append("-- Generated by the More Cultural Names Builder.");
                builder.Append(Environment.NewLine);
                builder.Append("-- City names are keyed by the owning civilization type, the axis");
                builder.Append(Environment.NewLine);
                builder.Append("-- Civilization V uses to name a city.");
                builder.Append(Environment.NewLine);
            }

            builder.Append(Environment.NewLine);
            builder.Append("local STORAGE_PREFIX = \"");
            builder.Append(GetFilePrefix());
            builder.Append(":\"");
            builder.Append(Environment.NewLine);
            builder.Append("local KEY_SEPARATOR = \"_\"");
            builder.Append(Environment.NewLine);
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function getStoredBaseKey(city)");
            AppendLine(builder, "    local plot = city:Plot()");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if plot == nil then");
            AppendLine(builder, "        return nil");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    local storedValue = plot:GetScriptData()");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if storedValue == nil then");
            AppendLine(builder, "        return nil");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if string.sub(storedValue, 1, string.len(STORAGE_PREFIX)) ~= STORAGE_PREFIX then");
            AppendLine(builder, "        return nil");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    return string.sub(storedValue, string.len(STORAGE_PREFIX) + 1)");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function storeBaseKey(city, baseKey)");
            AppendLine(builder, "    local plot = city:Plot()");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if plot == nil then");
            AppendLine(builder, "        return");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    plot:SetScriptData(STORAGE_PREFIX .. baseKey)");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function getBaseKey(city)");
            AppendLine(builder, "    local storedBaseKey = getStoredBaseKey(city)");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if storedBaseKey ~= nil then");
            AppendLine(builder, "        return storedBaseKey");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    return city:GetNameKey()");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function applyCulturalName(playerId, cityId)");
            AppendLine(builder, "    local player = Players[playerId]");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if player == nil then");
            AppendLine(builder, "        return");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    local city = player:GetCityByID(cityId)");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if city == nil then");
            AppendLine(builder, "        return");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    local baseKey = getBaseKey(city)");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if baseKey == nil then");
            AppendLine(builder, "        return");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    storeBaseKey(city, baseKey)");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    local civilizationInfo = GameInfo.Civilizations[player:GetCivilizationType()]");
            AppendLine(builder, "    local nameKey = baseKey");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if civilizationInfo ~= nil then");
            AppendLine(builder, "        local candidateKey = baseKey .. KEY_SEPARATOR .. civilizationInfo.Type");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "        if Locale.HasTextKey(candidateKey) then");
            AppendLine(builder, "            nameKey = candidateKey");
            AppendLine(builder, "        end");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    local desiredName = Locale.Lookup(nameKey)");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if city:GetName() ~= desiredName then");
            AppendLine(builder, "        city:SetName(desiredName, false)");
            AppendLine(builder, "    end");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function applyCulturalNamesToPlayer(playerId)");
            AppendLine(builder, "    local player = Players[playerId]");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    if player == nil then");
            AppendLine(builder, "        return");
            AppendLine(builder, "    end");
            AppendLine(builder, string.Empty);
            AppendLine(builder, "    for city in player:Cities() do");
            AppendLine(builder, "        applyCulturalName(playerId, city:GetID())");
            AppendLine(builder, "    end");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function applyCulturalNamesToAllPlayers()");
            AppendLine(builder, "    for playerId = 0, GameDefines.MAX_CIV_PLAYERS - 1 do");
            AppendLine(builder, "        applyCulturalNamesToPlayer(playerId)");
            AppendLine(builder, "    end");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function onCityCaptured(hexPosition, playerId, cityId, newPlayerId)");
            AppendLine(builder, "    applyCulturalName(newPlayerId, cityId)");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function onCityCreated(hexPosition, playerId, cityId, artStyleType, eraType, continent, populationSize, size, fogState)");
            AppendLine(builder, "    applyCulturalName(playerId, cityId)");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "local function initialize()");
            AppendLine(builder, "    Events.SerialEventCityCaptured.Add(onCityCaptured)");
            AppendLine(builder, "    Events.SerialEventCityCreated.Add(onCityCreated)");
            AppendLine(builder, "    GameEvents.PlayerDoTurn.Add(applyCulturalNamesToPlayer)");
            AppendLine(builder, "    applyCulturalNamesToAllPlayers()");
            AppendLine(builder, "end");
            builder.Append(Environment.NewLine);

            AppendLine(builder, "Events.LoadScreenClose.Add(initialize)");

            return builder.ToString();
        }

        private string GenerateDescriptorContent()
        {
            string filePrefix = GetFilePrefix();
            StringBuilder builder = new();

            builder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            builder.Append(Environment.NewLine);
            builder.Append($"<!-- Version {Settings.Mod.Version} -->");
            builder.Append(Environment.NewLine);
            builder.Append($"<Mod id=\"{GenerateModIdentifier()}\" version=\"1\">");
            builder.Append(Environment.NewLine);
            builder.Append("  <Properties>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <Name>{Escape(Settings.Mod.Name)}</Name>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <Teaser>{Escape(Settings.Mod.Name)}</Teaser>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <Description>{Escape(Settings.Mod.Name)}</Description>");
            builder.Append(Environment.NewLine);
            builder.Append("    <AffectsSavedGames>0</AffectsSavedGames>");
            builder.Append(Environment.NewLine);
            builder.Append("    <SupportsSinglePlayer>1</SupportsSinglePlayer>");
            builder.Append(Environment.NewLine);
            builder.Append("    <SupportsMultiplayer>1</SupportsMultiplayer>");
            builder.Append(Environment.NewLine);
            builder.Append("    <SupportsMac>1</SupportsMac>");
            builder.Append(Environment.NewLine);
            builder.Append("  </Properties>");
            builder.Append(Environment.NewLine);
            builder.Append("  <Dependencies/>");
            builder.Append(Environment.NewLine);
            builder.Append("  <References/>");
            builder.Append(Environment.NewLine);
            builder.Append("  <Blocks/>");
            builder.Append(Environment.NewLine);
            builder.Append("  <Files>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <File md5=\"{GenerateContentHash(luaBytes)}\" import=\"1\">Lua/{filePrefix}.lua</File>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <File md5=\"{GenerateContentHash(localisationBytes)}\" import=\"0\">XML/{filePrefix}.xml</File>");
            builder.Append(Environment.NewLine);
            builder.Append("  </Files>");
            builder.Append(Environment.NewLine);
            builder.Append("  <Actions>");
            builder.Append(Environment.NewLine);
            builder.Append("    <OnModActivated>");
            builder.Append(Environment.NewLine);
            builder.Append($"      <UpdateDatabase>XML/{filePrefix}.xml</UpdateDatabase>");
            builder.Append(Environment.NewLine);
            builder.Append("    </OnModActivated>");
            builder.Append(Environment.NewLine);
            builder.Append("  </Actions>");
            builder.Append(Environment.NewLine);
            builder.Append("  <EntryPoints>");
            builder.Append(Environment.NewLine);
            builder.Append($"    <EntryPoint type=\"InGameUIAddin\" file=\"Lua/{filePrefix}.lua\">");
            builder.Append(Environment.NewLine);
            builder.Append($"      <Name>{Escape(Settings.Mod.Name)}</Name>");
            builder.Append(Environment.NewLine);
            builder.Append($"      <Description>{Escape(Settings.Mod.Name)}</Description>");
            builder.Append(Environment.NewLine);
            builder.Append("    </EntryPoint>");
            builder.Append(Environment.NewLine);
            builder.Append("  </EntryPoints>");
            builder.Append(Environment.NewLine);
            builder.Append("</Mod>");

            return builder.ToString();
        }

        private string GenerateModIdentifier()
        {
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(Settings.Mod.Id));

            return new Guid(hash.AsSpan(0, 16)).ToString();
        }

        private static byte[] EncodeContent(string content) =>
            [.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(content)];

        private static string GenerateContentHash(byte[] content)
            => Convert.ToHexString(MD5.HashData(content));

        private string GetFilePrefix() => Settings.Mod.Id.Replace('-', '_');

        private static void AppendLine(StringBuilder builder, string line)
        {
            builder.Append(line);
            builder.Append(Environment.NewLine);
        }

        private static string Escape(string value)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return SecurityElement.Escape(value);
        }
    }
}
