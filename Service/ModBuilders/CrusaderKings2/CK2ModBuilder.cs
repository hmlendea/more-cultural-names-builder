using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public class CK2ModBuilder : ModBuilder, ICK2ModBuilder
    {
        public override string Game => "CK2";

        protected virtual string InputLandedTitlesFileName => "ck2_landed_titles.txt";

        protected virtual string OutputLandedTitlesFileName => "landed_titles.txt";

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;

        IDictionary<string, IEnumerable<Localisation>> localisations;

        public CK2ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
            
            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, ModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);

            LoadData();
            CreateDataFiles(landedTitlesDirectoryPath);
            CreateDescriptorFiles();
        }

        protected virtual string GenerateDescriptorContent()
        {
            return
                $"# Version {outputSettings.ModVersion} ({DateTime.Now})" + Environment.NewLine +
                $"name = \"{outputSettings.CK2ModName}\"" + Environment.NewLine +
                $"picture = \"thumbnail.png\"" + Environment.NewLine +
                $"tags = {{ map immersion }}";
        }

        protected virtual string GenerateMainDescriptorContent()
        {
            return GenerateDescriptorContent() + Environment.NewLine +
                $"path = \"mod/{outputSettings.CK2ModId}\"";
        }

        void LoadData()
        {
            IEnumerable<GameId> locationGameIds = locations.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game);
            
            localisations = new Dictionary<string, IEnumerable<Localisation>>();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                localisations.TryAdd(locationGameId.Id, localisationFetcher.GetGameLocationLocalisations(locationGameId.Id, Game));
            });
        }

        void CreateDataFiles(string landedTitlesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            Dictionary<string, List<Localisation>> localisationsByLocation = localisations
                .GroupBy(x => x.LocationGameId)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            
            string content = BuildLandedTitlesFile();
            WriteLandedTitlesFile(content, landedTitlesDirectoryPath);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{ModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, ModId, "descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string BuildLandedTitlesFile()
        {
            string landedTitlesFile = ReadLandedTitlesFile(Path.Combine(ApplicationPaths.DataDirectory, InputLandedTitlesFileName));
            landedTitlesFile = CleanLandedTitlesFile(landedTitlesFile);

            List<string> content = new List<string> { string.Empty };
            List<string> landedTitlesFileLines = landedTitlesFile.Split('\n').ToList();
            landedTitlesFileLines.Add(string.Empty);

            List<string> forbiddenTokensForPreviousLine = new List<string> { "allow", "dejure_liege_title", "gain_effect", "limit", "trigger" };
            List<string> forbiddenTokensForNextLine = new List<string> { "any_direct_de_jure_vassal_title", "has_holder", "is_titular", "owner", "show_scope_change" };
            
            string forbiddenTokensForPreviousLinePattern = "^.*" + string.Join('|', forbiddenTokensForPreviousLine) + ".*$";
            string forbiddenTokensForNextLinePattern = "^.*" + string.Join('|', forbiddenTokensForNextLine) + ".*$";

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

        string GetTitleLocalisationsContent(string line, string gameId)
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
                lines.Add($"{indentation}{localisation.LanguageGameId} = \"{normalisedName}\"");
            }

            return string.Join(Environment.NewLine, lines);
        }

        string CleanLandedTitlesFile(string content)
        {
            IEnumerable<GameId> gameLanguageIds = languages.Values
                .SelectMany(x => x.GameIds)
                .Where(x => x.Game == Game)
                .OrderBy(x => x.Id);

            string culturesPattern = string.Join('|', gameLanguageIds.Select(x => x.Id));

            string newContent = content;
            newContent = Regex.Replace(newContent, "\\t", "    ", RegexOptions.Multiline); // Replace tabs
            newContent = Regex.Replace(newContent, "\\s*=\\s*", " = ", RegexOptions.Multiline); // Standardise spacings aroung equals
            newContent = Regex.Replace(newContent, "\\s*#[^\r\n]*", "", RegexOptions.Multiline); // Remove comments
            newContent = Regex.Replace(newContent, "^\\s*\n", "", RegexOptions.Multiline); // Remove empty/whitespace lines
            newContent = Regex.Replace(newContent, "\\s+$", "", RegexOptions.Multiline); // Remove trailing whitespaces

            newContent = Regex.Replace( // Break inline cultural name into multiple lines
                newContent,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + culturesPattern + ")\\s*=\\s*\"*[^\"]*\")\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);
            
            newContent = Regex.Replace( // Remove cultural names
                newContent,
                "^\\s*(" + culturesPattern + ")\\s*=\\s*\"*[^\"\r\n]*\"*[^\r\n]*\r*\n",
                "",
                RegexOptions.Multiline);

            newContent = Regex.Replace( // Break ={} into multiple lines
                newContent,
                "(^\\s*)([^\\s]*\\s*=\\s*\\{)\\s*\\}",
                "$1$2\n$1}",
                RegexOptions.Multiline);

            newContent = newContent.Replace("\r", "");
            
            return newContent;
        }

        string ReadLandedTitlesFile(string filePath)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(filePath, encoding);
        }

        void WriteLandedTitlesFile(string content, string landedTitlesDirectoryPath)
        {
            string filePath = Path.Combine(landedTitlesDirectoryPath, OutputLandedTitlesFileName);

            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }
    }
}
