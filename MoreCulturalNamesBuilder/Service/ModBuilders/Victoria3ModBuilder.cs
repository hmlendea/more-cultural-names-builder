using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.Repositories;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class Victoria3ModBuilder(
        ILocalisationFetcher localisationFetcher,
        INameNormaliser nameNormaliser,
        IFileRepository<LanguageDataObject> languageRepository,
        IFileRepository<LocationDataObject> locationRepository,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        private static readonly string[] hubTypes = ["city", "port", "mine", "farm", "wood"];

        private static readonly string[] localisationLanguages =
        [
            "braz_por",
            "english",
            "french",
            "german",
            "japanese",
            "korean",
            "polish",
            "russian",
            "simp_chinese",
            "spanish",
            "turkish",
        ];

        private IList<string> stateIds = null!;
        private IList<string> cultureIds = null!;
        private IDictionary<string, IList<string>> hubIdsByStateId = null!;
        private IDictionary<string, IDictionary<string, Localisation>> localisationsByGameId = null!;

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = [];
            ConcurrentBag<KeyValuePair<string, string>> hubStatePairs = [];

            localisationsByGameId = concurrentLocalisations;

            stateIds = [.. locationGameIds
                .Where(gameId => string.Equals(gameId.Type, "State", StringComparison.Ordinal))
                .Select(gameId => gameId.Id)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(stateId => stateId, StringComparer.Ordinal)];

            Parallel.ForEach(stateIds, stateId =>
                concurrentLocalisations.TryAdd(stateId, GetLocalisations(stateId, "State")));

            Parallel.ForEach(
                locationGameIds.Where(gameId => string.Equals(gameId.Type, "Hub", StringComparison.Ordinal)),
                hubGameId =>
                {
                    string stateId = GetStateId(hubGameId.Id);

                    if (string.IsNullOrWhiteSpace(stateId))
                    {
                        return;
                    }

                    concurrentLocalisations.TryAdd(hubGameId.Id, GetLocalisations(hubGameId.Id, "Hub"));
                    hubStatePairs.Add(new(stateId, hubGameId.Id));
                });

            hubIdsByStateId = hubStatePairs
                .GroupBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => (IList<string>)[.. group
                        .Select(pair => pair.Value)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(hubId => hubId, StringComparer.Ordinal)],
                    StringComparer.Ordinal);

            stateIds = [.. stateIds.Where(HasLocalisations)];

            cultureIds = [.. localisationsByGameId
                .Values
                .SelectMany(localisations => localisations.Keys)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(cultureId => cultureId, StringComparer.Ordinal)];
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string effectsDirectoryPath = Path.Combine(mainDirectoryPath, "common", "scripted_effects");
            string onActionsDirectoryPath = Path.Combine(mainDirectoryPath, "common", "on_actions");
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, "localization");
            string metadataDirectoryPath = Path.Combine(mainDirectoryPath, ".metadata");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(effectsDirectoryPath);
            Directory.CreateDirectory(onActionsDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);
            Directory.CreateDirectory(metadataDirectoryPath);

            CreateStateNamesFile(effectsDirectoryPath);
            CreateOnActionsFile(onActionsDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateMetadataFiles(metadataDirectoryPath);
        }

        private void CreateStateNamesFile(string effectsDirectoryPath)
        {
            string filePath = Path.Combine(effectsDirectoryPath, $"{GetFilePrefix()}_state_names.txt");

            File.WriteAllText(filePath, GenerateStateNamesContent(), new UTF8Encoding(true));
        }

        private void CreateOnActionsFile(string onActionsDirectoryPath)
        {
            string filePath = Path.Combine(onActionsDirectoryPath, $"{GetFilePrefix()}_state_names.txt");
            string content = GenerateOnActionsContent();

            File.WriteAllText(filePath, content, new UTF8Encoding(true));
        }

        private void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string content = GenerateStateNamesLocalisationContent();

            Parallel.ForEach(localisationLanguages, fileLanguage =>
                CreateLocalisationFile(localisationDirectoryPath, fileLanguage, content));
        }

        private void CreateMetadataFiles(string metadataDirectoryPath)
        {
            string filePath = Path.Combine(metadataDirectoryPath, "metadata.json");

            File.WriteAllText(filePath, GenerateMetadataContent(), new UTF8Encoding(false));
        }

        private void CreateLocalisationFile(string localisationDirectoryPath, string language, string content)
        {
            string languageDirectoryPath = Path.Combine(localisationDirectoryPath, language);
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{GetFilePrefix()}_state_names_l_{language}.yml";
            string filePath = Path.Combine(languageDirectoryPath, fileName);

            Directory.CreateDirectory(languageDirectoryPath);
            File.WriteAllText(filePath, fileContent, new UTF8Encoding(true));
        }

        private string GenerateStateNamesContent()
        {
            List<string> blocks =
            [
                GenerateHeader(),
                GenerateEvaluationEffect(),
                GenerateDispatchEffect()
            ];

            blocks.AddRange(stateIds.Select(GenerateStateAssignmentEffect));

            return string.Join(Environment.NewLine + Environment.NewLine, blocks);
        }

        private string GenerateOnActionsContent()
        {
            List<string> blocks =
            [
                GenerateHeader(),
                GenerateHook("on_state_owner_change", $"{GetFilePrefix()}_on_state_owner_change"),
                GenerateHook("on_state_created", $"{GetFilePrefix()}_on_state_created"),
                GenerateHook("on_become_subject", $"{GetFilePrefix()}_on_become_subject"),
                GenerateHook("on_become_independent", $"{GetFilePrefix()}_on_become_independent"),
                GenerateStateOnAction($"{GetFilePrefix()}_on_state_owner_change", false),
                GenerateStateOnAction($"{GetFilePrefix()}_on_state_created", false),
                GenerateStateOnAction($"{GetFilePrefix()}_on_become_subject", true),
                GenerateStateOnAction($"{GetFilePrefix()}_on_become_independent", true)
            ];

            return string.Join(Environment.NewLine + Environment.NewLine, blocks);
        }

        private string GenerateHeader()
            => $"# {Settings.Mod.Name}{Environment.NewLine}# Generated by the More Cultural Names Builder";

        private string GenerateEvaluationEffect()
        {
            StringBuilder builder = new();

            AppendIndentedLine(builder, 0, $"{GetFilePrefix()}_evaluate_and_assign_state_hub_dynamic_names = {{");
            AppendIndentedLine(builder, 1, "if = {");
            AppendIndentedLine(builder, 2, "limit = { has_game_rule = no_dynamic_naming }");
            AppendIndentedLine(builder, 2, "reset_state_name = yes");
            AppendIndentedLine(builder, 2, "reset_hub_names = yes");
            AppendIndentedLine(builder, 1, "}");
            AppendIndentedLine(builder, 1, "else = {");
            AppendIndentedLine(builder, 2, $"{GetFilePrefix()}_assign_state_hub_dynamic_names = yes");
            AppendIndentedLine(builder, 1, "}");
            AppendIndentedLine(builder, 0, "}");

            return builder.ToString().TrimEnd();
        }

        private string GenerateDispatchEffect()
        {
            StringBuilder builder = new();

            AppendIndentedLine(builder, 0, $"{GetFilePrefix()}_assign_state_hub_dynamic_names = {{");

            foreach (string stateId in stateIds)
            {
                AppendIndentedLine(builder, 1, "if = {");
                AppendIndentedLine(builder, 2, $"limit = {{ state_region = {{ s:{stateId} ?= this }} }}");
                AppendIndentedLine(builder, 2, $"{GetFilePrefix()}_{stateId}_state_name_assign = yes");
                AppendIndentedLine(builder, 1, "}");
            }

            AppendIndentedLine(builder, 0, "}");

            return builder.ToString().TrimEnd();
        }

        private string GenerateStateAssignmentEffect(string stateId)
        {
            StringBuilder builder = new();

            AppendIndentedLine(builder, 0, $"{GetFilePrefix()}_{stateId}_state_name_assign = {{");

            foreach (string cultureId in cultureIds)
            {
                builder.Append(GenerateStateAssignmentBlock(stateId, cultureId));
            }

            AppendIndentedLine(builder, 0, "}");

            return builder.ToString().TrimEnd();
        }

        private string GenerateStateAssignmentBlock(string stateId, string cultureId)
        {
            IDictionary<string, Localisation> stateLocalisations = localisationsByGameId[stateId];

            if (!stateLocalisations.TryGetValue(cultureId, out Localisation? stateLocalisation) ||
                stateLocalisation is null)
            {
                return string.Empty;
            }

            StringBuilder builder = new();

            AppendIndentedLine(builder, 1, "if = {");
            AppendIndentedLine(
                builder,
                2,
                $"limit = {{ owner = {{ country_has_primary_culture = cu:{cultureId} }} }}");
            AppendIndentedLine(builder, 2, $"set_state_name = {stateId}_{cultureId}{GenerateComment(stateLocalisation)}");

            foreach (string hubId in GetHubIds(stateId))
            {
                if (!localisationsByGameId.TryGetValue(hubId, out IDictionary<string, Localisation>? hubLocalisations) ||
                    hubLocalisations is null ||
                    !hubLocalisations.TryGetValue(cultureId, out Localisation? hubLocalisation) ||
                    hubLocalisation is null)
                {
                    continue;
                }

                AppendIndentedLine(
                    builder,
                    2,
                    $"set_hub_name = {{ type = {GetHubType(hubId)} name = HUB_NAME_{hubId}_{cultureId} }}{GetNameComment(hubLocalisation)}");
            }

            AppendIndentedLine(builder, 1, "}");

            return builder.ToString();
        }

        private static string GenerateHook(string onActionName, string handlerName)
        {
            StringBuilder builder = new();

            AppendIndentedLine(builder, 0, $"{onActionName} = {{");
            AppendIndentedLine(builder, 1, $"on_actions = {{ {handlerName} }}");
            AppendIndentedLine(builder, 0, "}");

            return builder.ToString().TrimEnd();
        }

        private string GenerateStateOnAction(string onActionName, bool isCountryScope)
        {
            string invocation = $"{GetFilePrefix()}_evaluate_and_assign_state_hub_dynamic_names = yes";
            StringBuilder builder = new();

            AppendIndentedLine(builder, 0, $"{onActionName} = {{");
            AppendIndentedLine(builder, 1, "effect = {");

            if (isCountryScope)
            {
                AppendIndentedLine(builder, 2, "every_scope_state = {");
                AppendIndentedLine(builder, 3, invocation);
                AppendIndentedLine(builder, 2, "}");
            }
            else
            {
                AppendIndentedLine(builder, 2, invocation);
            }

            AppendIndentedLine(builder, 1, "}");
            AppendIndentedLine(builder, 0, "}");

            return builder.ToString().TrimEnd();
        }

        private string GenerateStateNamesLocalisationContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            Parallel.ForEach(
                stateIds,
                List<string> () => [],
                (stateId, _, localLines) =>
                {
                    foreach (string cultureId in cultureIds)
                    {
                        IDictionary<string, Localisation> stateLocalisations = localisationsByGameId[stateId];

                        if (stateLocalisations.TryGetValue(cultureId, out Localisation? stateLocalisation) &&
                            stateLocalisation is not null)
                        {
                            localLines.Add(GenerateLocalisationLine($"{stateId}_{cultureId}", stateLocalisation));
                        }

                        foreach (string hubId in GetHubIds(stateId))
                        {
                            if (!localisationsByGameId.TryGetValue(hubId, out IDictionary<string, Localisation>? hubLocalisations) ||
                                hubLocalisations is null ||
                                !hubLocalisations.TryGetValue(cultureId, out Localisation? hubLocalisation) ||
                                hubLocalisation is null)
                            {
                                continue;
                            }

                            localLines.Add(GenerateLocalisationLine($"HUB_NAME_{hubId}_{cultureId}", hubLocalisation));
                        }
                    }

                    return localLines;
                },
                localLines =>
                {
                    lock (lineCollectionLock)
                    {
                        lines.AddRange(localLines);
                    }
                });

            lines.Sort(StringComparer.Ordinal);

            return string.Join(Environment.NewLine, lines);
        }

        private string GenerateLocalisationLine(string key, Localisation localisation)
        {
            string line =
                $" {key}: " +
                $"\"{nameNormaliser.ToVictoria3Charset(localisation.Name)}\"";

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                line += $" # Language={localisation.LanguageId}";
            }

            if (!string.IsNullOrWhiteSpace(localisation.Comment))
            {
                line += $" # {localisation.Comment}";
            }

            return line;
        }

        private string GenerateMetadataContent()
        {
            StringBuilder builder = new();

            builder.Append('{');
            builder.Append(Environment.NewLine);
            builder.Append($"  \"name\": {Serialize(Settings.Mod.Name)},");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"id\": {Serialize(Settings.Mod.Id)},");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"version\": {Serialize(Settings.Mod.Version)},");
            builder.Append(Environment.NewLine);
            builder.Append("  \"game_id\": \"victoria3\",");
            builder.Append(Environment.NewLine);
            builder.Append("  \"picture\": \"thumbnail.png\",");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"supported_game_version\": {Serialize(Settings.Mod.GameVersion)},");
            builder.Append(Environment.NewLine);
            builder.Append($"  \"short_description\": {Serialize(Settings.Mod.Name)},");
            builder.Append(Environment.NewLine);
            builder.Append("  \"tags\": [");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Culture\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Historical\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Map\",");
            builder.Append(Environment.NewLine);
            builder.Append("    \"Translation\"");
            builder.Append(Environment.NewLine);
            builder.Append("  ],");
            builder.Append(Environment.NewLine);
            builder.Append("  \"relationships\": [],");
            builder.Append(Environment.NewLine);
            builder.Append("  \"game_custom_data\": {");
            builder.Append(Environment.NewLine);
            builder.Append("    \"multiplayer_synchronized\": true");
            builder.Append(Environment.NewLine);
            builder.Append("  }");
            builder.Append(Environment.NewLine);
            builder.Append('}');

            return builder.ToString();
        }

        private string GenerateComment(Localisation localisation)
        {
            string comment = string.Empty;

            if (Settings.Output.AreVerboseCommentsEnabled)
            {
                comment += $" # Language={localisation.LanguageId}";
            }

            if (!string.IsNullOrWhiteSpace(localisation.Comment))
            {
                comment += $" # {localisation.Comment}";
            }

            return comment;
        }

        private string GetNameComment(Localisation localisation)
        {
            if (!Settings.Output.AreVerboseCommentsEnabled)
            {
                return string.Empty;
            }

            return $" # {nameNormaliser.ToVictoria3Charset(localisation.Name)}";
        }

        private string GetFilePrefix() => Settings.Mod.Id.Replace('-', '_');

        private bool HasLocalisations(string stateId) =>
            localisationsByGameId.TryGetValue(stateId, out IDictionary<string, Localisation>? localisations) &&
            localisations is not null &&
            localisations.Count > 0;

        private IList<string> GetHubIds(string stateId)
        {
            if (hubIdsByStateId.TryGetValue(stateId, out IList<string>? hubIds) && hubIds is not null)
            {
                return hubIds;
            }

            return [];
        }

        private IDictionary<string, Localisation> GetLocalisations(string gameLocationId, string gameLocationIdType)
            => localisationFetcher
                .GetGameLocationLocalisations(gameLocationId, gameLocationIdType, Settings.Mod.Game)
                .ToDictionary(localisation => localisation.LanguageGameId, localisation => localisation);

        private static string GetStateId(string hubId)
        {
            string hubType = GetHubType(hubId);

            if (string.IsNullOrWhiteSpace(hubType))
            {
                return string.Empty;
            }

            return hubId[..(hubId.Length - hubType.Length - 1)];
        }

        private static string GetHubType(string hubId)
        {
            foreach (string hubType in hubTypes)
            {
                if (hubId.EndsWith($"_{hubType}", StringComparison.Ordinal))
                {
                    return hubType;
                }
            }

            return string.Empty;
        }

        private static string Serialize(string value) => JsonSerializer.Serialize(value);

        private static void AppendIndentedLine(StringBuilder builder, int indentationLevel, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            builder.Append(new string(' ', indentationLevel * 4));
            builder.Append(text);
            builder.Append(Environment.NewLine);
        }
    }
}
