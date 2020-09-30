using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;
using MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings3
{
    public sealed class CK3ModBuilder : CK2ModBuilder, ICK3ModBuilder
    {
        public override string Game => "CK3";

        protected override string InputLandedTitlesFileName => "ck3_landed_titles.txt";
        protected override string OutputLandedTitlesFileName => "999_MoreCulturalNames.txt";

        protected override List<string> ForbiddenTokensForPreviousLine => new List<string> { "allow", "limit", "trigger" };
        protected override List<string> ForbiddenTokensForNextLine => new List<string> { "has_holder" };

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        public CK3ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            OutputSettings outputSettings)
            : base(localisationFetcher, nameNormaliser, languageRepository, locationRepository, titleRepository, outputSettings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
        }

        protected override string GenerateMainDescriptorContent()
        {
            return GenerateDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{outputSettings.CK3ModId}\"";
        }

        protected override string GenerateDescriptorContent()
        {
            return
                $"# Version {outputSettings.ModVersion} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{outputSettings.CK3ModName}\"" + Environment.NewLine +
                $"version=\"{outputSettings.ModVersion}\"" + Environment.NewLine +
                $"supported_version=\"{outputSettings.CK3GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Culture\"" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";
        }

        protected override string GetTitleLocalisationsContent(string line, string gameId)
        {
            IEnumerable<Localisation> titleLocalisations = localisations.TryGetValue(gameId);

            if (EnumerableExt.IsNullOrEmpty(titleLocalisations))
            {
                return null;
            }

            string indentation1 = Regex.Match(line, "^(\\s*)" + gameId + "\\s*=\\s*\\{.*$").Groups[1].Value + "    ";
            string indentation2 = indentation1 + "    ";
            List<string> lines = new List<string>();

            lines.Add($"{indentation1}cultural_names = {{");

            foreach (Localisation localisation in titleLocalisations.OrderBy(x => x.LanguageId))
            {
                string normalisedName = nameNormaliser.ToCK3Charset(localisation.Name);
                lines.Add($"{indentation2}{localisation.LanguageGameId} = \"{normalisedName}\" # Language={localisation.LanguageId}");
            }

            lines.Add($"{indentation1}}}");

            return string.Join(Environment.NewLine, lines);
        }

        protected override string ReadLandedTitlesFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        protected override void WriteLandedTitlesFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        protected override string DoCleanLandedTitlesFile(string content)
        {
            return Regex.Replace( // Remove empty cultural_names blocks
                content,
                "^\\s*cultural_names\\s*=\\s*{\\s*\r*\n\\s*}\\s*\r*\n",
                "",
                RegexOptions.Multiline);
        }

        protected override string GenerateTitlesLocalisationFile(GameId languageGameId)
        {
            IList<string> lines = new List<string>();

            foreach (Title title in titles.Values)
            {
                IEnumerable<GameId> titleGameIds = title.GameIds
                    .Where(x => x.Game == Game)
                    .OrderBy(x => x.Id);

                Localisation localisation = localisationFetcher.GetTitleLocalisation(title.Id, languageGameId.Id, Game);

                if (localisation is null)
                {
                    continue;
                }

                foreach (GameId titleGameId in titleGameIds)
                {
                    string normalisedName = nameNormaliser.ToCK3Charset(localisation.Name);
                    string localisationKey = $"{titleGameId.Id}_{languageGameId.Id}";

                    if (!string.IsNullOrWhiteSpace(titleGameId.Type))
                    {
                        localisationKey += $"_{titleGameId.Type}";
                    }

                    string line = $" {localisationKey}:0 \"{normalisedName}\" # Language={localisation.LanguageId}";
                    
                    lines.Add(line);
                }
            }

            lines = lines.OrderBy(x => x).ToList();
            lines.Add(string.Empty);

            return string.Join(Environment.NewLine, lines);
        }

        protected override void CreateTitlesLocalisationFiles()
        {
            string localisationsDirectoryPath = Path.Combine(OutputDirectoryPath, ModId, "localization");

            Directory.CreateDirectory(localisationsDirectoryPath);

            List<string> localisationDirectories = new List<string> { "english", "french", "german", "spanish" };
            localisationDirectories = localisationDirectories.Select(x => Path.Combine(localisationsDirectoryPath, x)).ToList();
            localisationDirectories.ForEach(x => Directory.CreateDirectory(x));

            foreach (GameId languageGameId in languageGameIds)
            {
                foreach (string localisationDirectory in localisationDirectories)
                {
                    string filePath = Path.Combine(localisationDirectory, $"873_MCN_titles_{languageGameId.Id}.yml");
                    string content = GenerateTitlesLocalisationFile(languageGameId);

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }
                    
                    content = $"l_{localisationDirectory}:" + Environment.NewLine + content;

                    File.WriteAllText(filePath, content);
                }
            }
        }
    }
}
