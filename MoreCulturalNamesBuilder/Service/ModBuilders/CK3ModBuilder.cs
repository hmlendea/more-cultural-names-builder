using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public sealed class CK3ModBuilder : CK2ModBuilder
    {
        protected override string LocalisationDirectoryName => "localization";
        protected override List<string> ForbiddenTokensForPreviousLine => new List<string> { "allow", "limit", "trigger" };
        protected override List<string> ForbiddenTokensForNextLine => new List<string> { "has_holder" };

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        public CK3ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            Settings settings)
            : base(localisationFetcher, nameNormaliser, languageRepository, locationRepository, settings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
        }

        protected override string GenerateMainDescriptorContent()
            => GenerateDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{Settings.Mod.Id}\"";

        protected override string GenerateDescriptorContent()
            =>  $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{Settings.Mod.Name}\"" + Environment.NewLine +
                $"version=\"{Settings.Mod.Version}\"" + Environment.NewLine +
                $"supported_version=\"{Settings.Mod.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Culture\"" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";

        protected override string ReadLandedTitlesFile()
            => File.ReadAllText(Settings.Input.LandedTitlesFilePath);

        protected override void WriteLandedTitlesFile(string filePath, string content)
            => File.WriteAllText(filePath, content);

        protected override string DoCleanLandedTitlesFile(string content)
        {
            string nameListsPattern = string.Join('|', languageGameIds.Select(x => $"name_list_{x.Id}"));
            string cleanContent = content;

            cleanContent = Regex.Replace( // Break inline cultural name into multiple lines
                cleanContent,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + nameListsPattern + ")\\s*=\\s*[a-zA-Z_-]*)\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);

            cleanContent = Regex.Replace( // Remove all original cultural names for supported languages
                cleanContent,
                "^\\s*(" + nameListsPattern + ")\\s*=\\s*[a-zA-Z_-]*\\s*\n",
                string.Empty,
                RegexOptions.Multiline);

            cleanContent = Regex.Replace( // Remove empty cultural_names blocks
                cleanContent,
                "^\\s*cultural_names\\s*=\\s*{\\s*\r*\n\\s*}\\s*\r*\n",
                "",
                RegexOptions.Multiline);

            return cleanContent;
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
                string lineToAdd =
                    $"{indentation2}name_list_{localisation.LanguageGameId} = cn_{localisation.Id}_{localisation.LanguageGameId}" +
                    $" # {nameNormaliser.ToCK3Charset(localisation.Name)}";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    lineToAdd += $" # Language={localisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    lineToAdd += $" # {nameNormaliser.ToCK3Charset(localisation.Comment)}";
                }

                lines.Add(lineToAdd);
            }

            lines.Add($"{indentation1}}}");

            return string.Join(Environment.NewLine, lines);
        }

        protected override void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string defaultLocalisationsFileContent = GenerateDefaultNamesLocalisationFileContent();
            string dynamicLocalisationsFileContent = GenerateDynamicNamesLocalisationFileContent();

            List<string> localisationLanguages = new List<string>{ "english", "french", "german", "spanish" };

            Parallel.ForEach(localisationLanguages, fileLanguage => CreateLocalisationFile(
                localisationDirectoryPath,
                "titles",
                fileLanguage,
                defaultLocalisationsFileContent));

            Parallel.ForEach(localisationLanguages, fileLanguage => CreateLocalisationFile(
                localisationDirectoryPath,
                "titles_cultural_names",
                fileLanguage,
                dynamicLocalisationsFileContent));
        }

        protected override void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, "descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string GenerateDefaultNamesLocalisationFileContent()
        {
            ConcurrentBag<string> lines = new ConcurrentBag<string>();

            Parallel.ForEach(locations.Values, location =>
            {
                foreach (GameId gameId in location.GameIds.Where(x => x.Game.Equals(Settings.Mod.Game)))
                {
                    if (string.IsNullOrWhiteSpace(gameId.DefaultNameLanguageId))
                    {
                        continue;
                    }

                    Localisation defaultLocalisation = localisations[gameId.Id]
                        .FirstOrDefault(x => x.LanguageId.Equals(gameId.DefaultNameLanguageId));

                    if (defaultLocalisation is null)
                    {
                        continue;
                    }

                    string normalisedName = nameNormaliser.ToCK3Charset(defaultLocalisation.Name);
                    lines.Add(
                        $"{defaultLocalisation.GameId}" +
                        $";{normalisedName};{normalisedName};{normalisedName};;{normalisedName};;;;;;;;;x");

                    if (!string.IsNullOrWhiteSpace(defaultLocalisation.Adjective))
                    {
                        string normalisedAdjective = nameNormaliser.ToCK3Charset(defaultLocalisation.Name);
                        lines.Add(
                            $"{defaultLocalisation.GameId}" +
                            $";{normalisedAdjective};{normalisedAdjective};{normalisedAdjective};;{normalisedAdjective};;;;;;;;;x");
                    }
                }
            });

            return string.Join(
                Environment.NewLine,
                lines.OrderBy(line => line));
        }

        string GenerateDynamicNamesLocalisationFileContent()
        {
            ConcurrentBag<string> lines = new ConcurrentBag<string>();

            List<Localisation> locs = localisations
                .SelectMany(x => x.Value)
                .GroupBy(x => $"{x.Id}_{x.LanguageGameId}")
                .Select(x => x.First())
                .ToList();

            Parallel.ForEach(locs, localisation =>
            {
                string normalisedName = nameNormaliser.ToCK3Charset(localisation.Name);
                string titleLocalisationDefinition = $" cn_{localisation.Id}_{localisation.LanguageGameId}:0 \"{normalisedName}\"";

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    titleLocalisationDefinition += $" # {localisation.Comment}";
                }

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    titleLocalisationDefinition += $" # Language={localisation.LanguageId}";
                }

                lines.Add(titleLocalisationDefinition);

                GameId gameId = locationGameIds.First(x => x.Id.Equals(localisation.GameId));

                if (!string.IsNullOrWhiteSpace(localisation.Adjective))
                {
                    string normalisedAdjective = nameNormaliser.ToCK3Charset(localisation.Adjective);
                    lines.Add($" cn_{localisation.Id}_{localisation.LanguageGameId}_adj:0 \"{normalisedAdjective}\"");
                }
            });

            return string.Join(
                Environment.NewLine,
                lines.OrderBy(line => line));
        }

        void CreateLocalisationFile(string localisationDirectoryPath, string fileLabel, string language, string content)
        {
            string fileContent = $"l_{language}:{Environment.NewLine}{content}";
            string fileName = $"{Settings.Mod.Id}_{fileLabel}_l_{language}.yml";
            string filePath = Path.Combine(localisationDirectoryPath, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        void WriteFileWithByteOrderMark(string filePath, string content)
            => File.WriteAllText(filePath, content + '\uFEFF');
    }
}
