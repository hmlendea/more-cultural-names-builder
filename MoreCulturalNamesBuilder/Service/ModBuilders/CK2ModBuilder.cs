using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

using NuciDAL.IO;
using NuciDAL.Repositories;

using NuciExtensions;

namespace MoreCulturalNamesBuilder.Service.ModBuilders
{
    public partial class CK2ModBuilder : ModBuilder
    {
        private readonly ILocalisationFetcher localisationFetcher;
        protected readonly INameNormaliser NameNormaliser;
        private readonly ConcurrentDictionary<string, string> windows1252NameCache;

        protected IDictionary<string, IEnumerable<Localisation>> localisations = null!;
        protected Dictionary<string, IEnumerable<Localisation>> localisationsOrderedByLanguageGameId;
        private IDictionary<string, GameId> locationGameIdsById = null!;

        [GeneratedRegex("\\t", RegexOptions.Multiline)]
        private static partial Regex TabRegex { get; }

        [GeneratedRegex("\\s*=\\s*", RegexOptions.Multiline)]
        private static partial Regex AroundEqualsRegex { get; }

        [GeneratedRegex("\\s*#[^\\n]*", RegexOptions.Multiline)]
        private static partial Regex CommentRegex { get; }

        [GeneratedRegex("^\\s*\\n", RegexOptions.Multiline)]
        private static partial Regex EmptyLineRegex { get; }

        [GeneratedRegex("\\s+$", RegexOptions.Multiline)]
        private static partial Regex TrailingWhitespaceRegex { get; }

        [GeneratedRegex("(^\\s*)([^\\s]*\\s*=\\s*\\{)\\s*\\}", RegexOptions.Multiline)]
        private static partial Regex EmptyBlockRegex { get; }

        [GeneratedRegex("^\\s*([ekdcb]_[^ =]*)[^=]\\s*=\\s*\\{[^\\{\\}]*$")]
        private static partial Regex TitleIdRegex { get; }

        protected virtual string LocalisationDirectoryName => "localisation";

        protected virtual IEnumerable<string> ForbiddenTokensForPreviousLine
            => ["allow", "dejure_liege_title", "gain_effect", "limit", "trigger"];

        protected virtual IEnumerable<string> ForbiddenTokensForNextLine
            => ["any_direct_de_jure_vassal_title", "has_holder", "is_titular", "owner", "show_scope_change"];

        public CK2ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IFileRepository<LanguageDataObject> languageRepository,
            IFileRepository<LocationDataObject> locationRepository,
            Settings settings)
            : base(languageRepository, locationRepository, settings)
        {
            this.localisationFetcher = localisationFetcher;
            NameNormaliser = nameNormaliser;
            windows1252NameCache = [];

            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);

            localisationsOrderedByLanguageGameId = [];
        }

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IEnumerable<Localisation>> concurrentLocalisations = new();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                IEnumerable<Localisation> locationLocalisations = localisationFetcher.GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game);
                concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
            });

            localisations = concurrentLocalisations.ToDictionary(entry => entry.Key, entry => entry.Value);
            localisationsOrderedByLanguageGameId = localisations
                .ToDictionary(
                    localisationsByGameId => localisationsByGameId.Key,
                    localisationsByGameId =>
                        (IEnumerable<Localisation>)[.. localisationsByGameId.Value.OrderBy(localisation => localisation.LanguageGameId)]);

            locationGameIdsById = locationGameIds
                .GroupBy(gameId => gameId.Id)
                .ToDictionary(group => group.Key, group => group.First());
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

            CreateDescriptorFiles();
            CreateLandedTitlesFile(landedTitlesDirectoryPath);
            CreateLocalisationFiles(localisationDirectoryPath);
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
            => GenerateDescriptorContent() +
                Environment.NewLine +
                $"path = \"mod/{Settings.Mod.Id}\"";

        protected virtual string? GetTitleLocalisationsContent(string line, string gameId)
        {
            IEnumerable<Localisation> titleLocalisations = localisationsOrderedByLanguageGameId.TryGetValue(gameId);

            if (EnumerableExt.IsNullOrEmpty(titleLocalisations))
            {
                return null;
            }

            string indentation = GetLeadingWhitespace(line) + "    ";
            List<string> lines = [];

            foreach (Localisation localisation in titleLocalisations)
            {
                string normalisedName = ToWindows1252(localisation.Name);
                string lineToAdd = $"{indentation}{localisation.LanguageGameId} = \"{normalisedName}\"";

                if (Settings.Output.AreVerboseCommentsEnabled)
                {
                    lineToAdd += $" # Language={localisation.LanguageId}";
                }

                if (!string.IsNullOrWhiteSpace(localisation.Comment))
                {
                    lineToAdd += $" # {ToWindows1252(localisation.Comment)}";
                }

                lines.Add(lineToAdd);
            }

            return string.Join(Environment.NewLine, lines);
        }

        protected virtual string ReadLandedTitlesFile()
            => File.ReadAllText(
                Settings.Input.LandedTitlesFilePath!,
                Encoding.GetEncoding("windows-1252"));

        protected virtual void WriteLandedTitlesFile(string filePath, string content)
            => Windows1252File.WriteAllText(filePath, content);

        protected virtual string DoCleanLandedTitlesFile(string content)
            => Regex.Replace( // Break inline cultural name into multiple lines
                content,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + string.Join('|', languageGameIds.Select(gameId => gameId.Id)) + ")\\s*=\\s*\"*[^\"]*\")\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);

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

            string filePath = Path.Combine(localisationsDirectoryPath, $"000_{Settings.Mod.Id}_landed_titles.csv");
            string content = GenerateLocalisationFileContent();

            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            Windows1252File.WriteAllText(filePath, content);
        }

        private void CreateLandedTitlesFile(string landedTitlesDirectoryPath)
        {
            string landedTitlesFileContent = BuildLandedTitlesFile();
            string landedTitlesFilePath = Path.Combine(landedTitlesDirectoryPath, Settings.Output.LandedTitlesFileName!);

            WriteLandedTitlesFile(landedTitlesFilePath, landedTitlesFileContent);
        }

        private string BuildLandedTitlesFile()
        {
            string landedTitlesFile = ReadLandedTitlesFile();
            landedTitlesFile = CleanLandedTitlesFile(landedTitlesFile);

            List<string> content = [];
            string[] landedTitlesFileLines = landedTitlesFile.Split('\n');

            Regex forbiddenTokensForPreviousLineRegEx = new(
                "^.*" + string.Join('|', ForbiddenTokensForPreviousLine) + ".*$",
                RegexOptions.Compiled);
            Regex forbiddenTokensForNextLineRegEx = new(
                "^.*" + string.Join('|', ForbiddenTokensForNextLine) + ".*$",
                RegexOptions.Compiled);
            string previousLine = string.Empty;

            for (int lineIndex = 0; lineIndex < landedTitlesFileLines.Length; lineIndex += 1)
            {
                string line = landedTitlesFileLines[lineIndex];
                string nextLine = string.Empty;

                if (lineIndex + 1 < landedTitlesFileLines.Length)
                {
                    nextLine = landedTitlesFileLines[lineIndex + 1];
                }

                content.Add(line);

                if (forbiddenTokensForPreviousLineRegEx.IsMatch(previousLine) ||
                    forbiddenTokensForNextLineRegEx.IsMatch(nextLine))
                {
                    continue;
                }

                Match titleIdMatch = TitleIdRegex.Match(line);

                if (!titleIdMatch.Success)
                {
                    continue;
                }

                string titleId = titleIdMatch.Groups[1].Value;

                if (string.IsNullOrWhiteSpace(titleId))
                {
                    continue;
                }

                string? titleLocalisationsContent = GetTitleLocalisationsContent(line, titleId);

                if (!string.IsNullOrWhiteSpace(titleLocalisationsContent))
                {
                    content.Add(titleLocalisationsContent);
                }

                previousLine = content.Last();
            }

            return string.Join(Environment.NewLine, content);
        }

        protected virtual string CleanLandedTitlesFile(string content)
        {
            string culturesPattern = string.Join('|', languageGameIds.Select(gameId => gameId.Id));

            string newContent = content.Replace("\r", ""); // Remove carriage returns
            newContent = TabRegex.Replace(newContent, "    "); // Replace tabs
            newContent = AroundEqualsRegex.Replace(newContent, " = "); // Standardise spacings aroung equals
            newContent = CommentRegex.Replace(newContent, string.Empty); // Remove comments
            newContent = EmptyLineRegex.Replace(newContent, string.Empty); // Remove empty/whitespace lines
            newContent = TrailingWhitespaceRegex.Replace(newContent, string.Empty); // Remove trailing whitespaces

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

            newContent = EmptyBlockRegex.Replace(newContent, "$1$2\n$1}"); // Break ={} into multiple lines

            return DoCleanLandedTitlesFile(newContent);
        }

        private string GenerateLocalisationFileContent()
        {
            List<string> lines = [];
            object lineCollectionLock = new();

            Parallel.ForEach(
                localisations,
                List<string> () => [],
                (localisationEntry, _, localLines) =>
                {
                    string locationGameId = localisationEntry.Key;
                    GameId gameId = locationGameIdsById[locationGameId];

                    foreach (Localisation localisation in localisationEntry.Value)
                    {
                        if (localisation.LanguageId.Equals(gameId.DefaultNameLanguageId))
                        {
                            localLines.Add(GenerateLocationLocalisationLine(
                                locationGameId,
                                localisation.Name));
                        }

                        if (string.IsNullOrWhiteSpace(localisation.Adjective))
                        {
                            continue;
                        }

                        localLines.Add(GenerateLocationLocalisationLine(
                            $"{locationGameId}_adj_{localisation.LanguageGameId}",
                            localisation.Adjective));

                        if (localisation.LanguageId.Equals(gameId.DefaultNameLanguageId))
                        {
                            localLines.Add(GenerateLocationLocalisationLine(
                                $"{locationGameId}_adj",
                                localisation.Adjective));
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

            lines.Sort();

            return string.Join(Environment.NewLine, lines);
        }

        private string GenerateLocationLocalisationLine(string localisationKey, string localisationValue)
        {
            string normalisedName = ToWindows1252(localisationValue);
            string localisationDefinition =
                $"{localisationKey}" +
                $";{normalisedName};{normalisedName};{normalisedName};;{normalisedName};;;;;;;;;x";

            return localisationDefinition;
        }

        private string ToWindows1252(string text)
        {
            if (text is null)
            {
                return null!;
            }

            if (windows1252NameCache.TryGetValue(text, out string? cachedNormalisedText))
            {
                return cachedNormalisedText;
            }

            string normalisedText = NameNormaliser.ToWindows1252(text);
            windows1252NameCache.TryAdd(text, normalisedText);

            return normalisedText;
        }

        protected static string GetLeadingWhitespace(string line)
        {
            int firstNonWhitespaceCharacterIndex = 0;

            while (firstNonWhitespaceCharacterIndex < line.Length &&
                char.IsWhiteSpace(line[firstNonWhitespaceCharacterIndex]))
            {
                firstNonWhitespaceCharacterIndex += 1;
            }

            return line[..firstNonWhitespaceCharacterIndex];
        }
    }
}
