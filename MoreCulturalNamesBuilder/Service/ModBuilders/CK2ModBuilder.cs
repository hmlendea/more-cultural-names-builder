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
    public class CK2ModBuilder : ModBuilder
    {
        protected virtual string LocalisationDirectoryName => "localisation";

        protected virtual List<string> ForbiddenTokensForPreviousLine => new List<string>
            { "allow", "dejure_liege_title", "gain_effect", "limit", "trigger" };

        protected virtual List<string> ForbiddenTokensForNextLine => new List<string>
            { "any_direct_de_jure_vassal_title", "has_holder", "is_titular", "owner", "show_scope_change" };
            
        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        protected IDictionary<string, IEnumerable<Localisation>> localisations;

        public CK2ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            Settings settings)
            : base(languageRepository, locationRepository, titleRepository, settings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
            
            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
        }

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IEnumerable<Localisation>> concurrentLocalisations =
                new ConcurrentDictionary<string, IEnumerable<Localisation>>();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                IEnumerable<Localisation> locationLocalisations = localisationFetcher.GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game);
                concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
            });

            localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
        }

        protected override void GenerateFiles()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");
            string localisationDirectoryPath = Path.Combine(mainDirectoryPath, LocalisationDirectoryName);

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);
            Directory.CreateDirectory(localisationDirectoryPath);

            LoadData();

            CreateDescriptorFiles();
            CreateLandedTitlesFile(landedTitlesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
            CreateTitlesLocalisationFiles();
        }

        protected virtual string GenerateDescriptorContent()
        {
            string content =
                $"# Version {Settings.Mod.Version} ({DateTime.Now})" + Environment.NewLine +
                $"# for {Settings.Mod.Game} {Settings.Mod.GameVersion}" + Environment.NewLine +
                $"name = \"{Settings.Mod.Name}\"" + Environment.NewLine;

            if (!string.IsNullOrWhiteSpace(Settings.Mod.Dependency))
            {
                content += $"dependencies = {{ \"{Settings.Mod.Dependency}\" }}" + Environment.NewLine;
            }
            
            content +=
                $"picture = \"thumbnail.png\"" + Environment.NewLine +
                $"tags = {{ map immersion }}";

            return content;
        }

        protected virtual string GenerateMainDescriptorContent()
        {
            return GenerateDescriptorContent() + Environment.NewLine +
                $"path = \"mod/{Settings.Mod.Id}\"";
        }

        protected virtual string GetTitleLocalisationsContent(string line, string gameId)
        {
            IEnumerable<Localisation> titleLocalisations = localisations.TryGetValue(gameId);

            if (EnumerableExt.IsNullOrEmpty(titleLocalisations))
            {
                return null;
            }

            string indentation = Regex.Match(line, "^(\\s*)" + gameId + "\\s*=\\s*\\{.*$").Groups[1].Value + "    ";
            List<string> lines = new List<string>();

            foreach (Localisation localisation in titleLocalisations.OrderBy(x => x.LanguageGameId))
            {
                string normalisedName = nameNormaliser.ToWindows1252(localisation.Name);
                string lineToAdd = $"{indentation}{localisation.LanguageGameId} = \"{normalisedName}\"";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    lineToAdd += $" # Language={localisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    lineToAdd += $" # {nameNormaliser.ToWindows1252(localisation.Comment)}";
                }

                lines.Add(lineToAdd);
            }

            return string.Join(Environment.NewLine, lines);
        }

        protected virtual string GenerateTitlesLocalisationFile(GameId languageGameId)
        {
            IList<string> lines = new List<string>();

            foreach (Title title in titles.Values)
            {
                IEnumerable<GameId> titleGameIds = title.GameIds
                    .Where(x => x.Game == Settings.Mod.Game)
                    .OrderBy(x => x.Id);

                Localisation localisation = localisationFetcher.GetTitleLocalisation(title.Id, languageGameId.Id, Settings.Mod.Game);

                if (localisation is null)
                {
                    continue;
                }

                foreach (GameId titleGameId in titleGameIds)
                {
                    string normalisedName = nameNormaliser.ToWindows1252(localisation.Name);
                    string line = $"{titleGameId.Id}_{languageGameId.Id};{normalisedName};{normalisedName};{normalisedName};;{normalisedName};;;;;;;;;x";
                    
                    if (Settings.Output.AreVerboseCommentsEnabled)
                    {
                        line += $" # Language={localisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        line += $" # ${nameNormaliser.ToWindows1252(localisation.Comment)}";
                    }
                    
                    lines.Add(line);
                }
            }

            lines = lines.OrderBy(x => x).ToList();
            lines.Add(string.Empty);

            return string.Join(Environment.NewLine, lines);
        }

        protected virtual string ReadLandedTitlesFile()
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(Settings.Input.LandedTitlesFilePath, encoding);
        }

        protected virtual void WriteLandedTitlesFile(string filePath, string content)
        {
            WriteWindows1252File(filePath, content);
        }

        protected virtual string DoCleanLandedTitlesFile(string content)
        {
            string culturesPattern = string.Join('|', languageGameIds.Select(x => x.Id));

            return Regex.Replace( // Break inline cultural name into multiple lines
                content,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + culturesPattern + ")\\s*=\\s*\"*[^\"]*\")\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);
        }

        protected virtual void CreateTitlesLocalisationFiles()
        {
            string localisationsDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, LocalisationDirectoryName);
            Directory.CreateDirectory(localisationsDirectoryPath);

            foreach (GameId languageGameId in languageGameIds)
            {
                string filePath = Path.Combine(localisationsDirectoryPath, $"000_{Settings.Mod.Id}_titles_{languageGameId.Id}.csv");
                string content = GenerateTitlesLocalisationFile(languageGameId);

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                WriteWindows1252File(filePath, content);
            }
        }

        protected virtual void CreateDescriptorFiles()
        {
            string filePath = Path.Combine(OutputDirectoryPath, $"{Settings.Mod.Id}.mod");
            string content = GenerateMainDescriptorContent();

            File.WriteAllText(filePath, content);
        }

        protected virtual void CreateLocalisationFiles(string localisationDirectoryPath)
        {
            string localisationsDirectoryPath = Path.Combine(OutputDirectoryPath, Settings.Mod.Id, LocalisationDirectoryName);
            Directory.CreateDirectory(localisationsDirectoryPath);

            foreach (GameId languageGameId in languageGameIds)
            {
                string filePath = Path.Combine(localisationsDirectoryPath, $"000_{Settings.Mod.Id}_landed_titles.csv");
                string content = GenerateLocalisationFileContent();

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                WriteWindows1252File(filePath, content);
            }
        }
        
        void CreateLandedTitlesFile(string landedTitlesDirectoryPath)
        {
            string landedTitlesFileContent = BuildLandedTitlesFile();
            string landedTitlesFilePath = Path.Combine(landedTitlesDirectoryPath, Settings.Output.LandedTitlesFileName);

            WriteLandedTitlesFile(landedTitlesFilePath, landedTitlesFileContent);
        }

        string BuildLandedTitlesFile()
        {
            string landedTitlesFile = ReadLandedTitlesFile();
            landedTitlesFile = CleanLandedTitlesFile(landedTitlesFile);
            
            List<string> content = new List<string> { string.Empty };
            List<string> landedTitlesFileLines = landedTitlesFile.Split('\n').ToList();
            landedTitlesFileLines.Add(string.Empty);

            string forbiddenTokensForPreviousLinePattern = "^.*" + string.Join('|', ForbiddenTokensForPreviousLine) + ".*$";
            string forbiddenTokensForNextLinePattern = "^.*" + string.Join('|', ForbiddenTokensForNextLine) + ".*$";

            for (int i = 0; i < landedTitlesFileLines.Count - 1; i++)
            {
                string line = landedTitlesFileLines[i];
                string previousLine = content.Last();
                string nextLine = landedTitlesFileLines[i + 1];

                content.Add(line);

                if (Regex.IsMatch(previousLine, forbiddenTokensForPreviousLinePattern) ||
                    Regex.IsMatch(nextLine, forbiddenTokensForNextLinePattern))
                {
                    continue;
                }

                string titleId = Regex.Match(line, "^\\s*([ekdcb]_[^ =]*)[^=]\\s*=\\s*\\{[^\\{\\}]*$").Groups[1].Value;

                if (string.IsNullOrWhiteSpace(titleId))
                {
                    continue;
                }

                string titleLocalisationsContent = GetTitleLocalisationsContent(line, titleId);

                if (!string.IsNullOrWhiteSpace(titleLocalisationsContent))
                {
                    content.Add(titleLocalisationsContent);
                }
            }

            return string.Join(Environment.NewLine, content.Skip(1));
        }

        string CleanLandedTitlesFile(string content)
        {
            string culturesPattern = string.Join('|', languageGameIds.Select(x => x.Id));

            string newContent = content.Replace("\r", ""); // Remove carriage returns
            newContent = Regex.Replace(newContent, "\\t", "    ", RegexOptions.Multiline); // Replace tabs
            newContent = Regex.Replace(newContent, "\\s*=\\s*", " = ", RegexOptions.Multiline); // Standardise spacings aroung equals
            newContent = Regex.Replace(newContent, "\\s*#[^\n]*", "", RegexOptions.Multiline); // Remove comments
            newContent = Regex.Replace(newContent, "^\\s*\n", "", RegexOptions.Multiline); // Remove empty/whitespace lines
            newContent = Regex.Replace(newContent, "\\s+$", "", RegexOptions.Multiline); // Remove trailing whitespaces

            newContent = Regex.Replace( // Break inline cultural name into multiple lines
                newContent,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + culturesPattern + ")\\s*=\\s*\"*[^\"]*\")\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);
            
            newContent = Regex.Replace( // Remove cultural names
                newContent,
                "^\\s*(" + culturesPattern + ")\\s*=\\s*\"*[^\"\n]*\"*[^\n]*\n",
                "",
                RegexOptions.Multiline);

            newContent = Regex.Replace( // Break ={} into multiple lines
                newContent,
                "(^\\s*)([^\\s]*\\s*=\\s*\\{)\\s*\\}",
                "$1$2\n$1}",
                RegexOptions.Multiline);
            
            newContent = DoCleanLandedTitlesFile(newContent);
            
            return newContent;
        }

        string GenerateLocalisationFileContent()
        {
            ConcurrentBag<string> lines = new ConcurrentBag<string>();

            Parallel.ForEach(localisations.Keys, locationGameId =>
            {
                foreach (Localisation localisation in localisations[locationGameId])
                {
                    if (string.IsNullOrWhiteSpace(localisation.Adjective))
                    {
                        continue;
                    }

                    string normalisedAdjective = nameNormaliser.ToWindows1252(localisation.Adjective);
                    string line =
                        $"{locationGameId}_adj_{localisation.LanguageGameId}" +
                        $";{normalisedAdjective};{normalisedAdjective};{normalisedAdjective};;{normalisedAdjective};;;;;;;;;x";

                    lines.Add(line);
                };
            });

            return string.Join(Environment.NewLine, lines.OrderBy(x => x));
        }

        void WriteWindows1252File(string filePath, string content)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }
    }
}
