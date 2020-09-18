using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Mapping;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2
{
    public sealed class CK2ModBuilder : ModBuilder, ICK2ModBuilder
    {
        public override string Game => "CK2HIP";

        const string LandedTitlesFileName = "0_HIP_MoreCulturalNames.txt";

        public CK2ModBuilder(
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(languageRepository, locationRepository, outputSettings)
        {
            EncodingProvider encodingProvider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(encodingProvider);
        }

        protected override void BuildMod()
        {
            string mainDirectoryPath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId);
            string commonDirectoryPath = Path.Combine(mainDirectoryPath, "common");
            string landedTitlesDirectoryPath = Path.Combine(commonDirectoryPath, "landed_titles");

            Directory.CreateDirectory(mainDirectoryPath);
            Directory.CreateDirectory(landedTitlesDirectoryPath);
            Directory.CreateDirectory(commonDirectoryPath);

            CreateDataFiles(landedTitlesDirectoryPath);
            CreateDescriptorFiles();
        }

        void CreateDataFiles(string landedTitlesDirectoryPath)
        {
            List<Localisation> localisations = GetLocalisations();

            Dictionary<string, List<Localisation>> localisationsByLocation = localisations
                .GroupBy(x => x.LocationId)
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

            IEnumerable<Location> locations = locationRepository.GetAll().ToServiceModels();
            
            string content = BuildLandedTitlesFile();
            WriteLandedTitlesFile(content, landedTitlesDirectoryPath);
        }

        void CreateDescriptorFiles()
        {
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateInnerDescriptorContent();

            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{outputSettings.CK2HipModId}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, outputSettings.CK2HipModId, "descriptor.mod");

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);
        }

        string BuildLandedTitlesFile()
        {
            string landedTitlesFile = ReadLandedTitlesFileLines(Path.Combine(ApplicationPaths.DataDirectory, "ck2hip_landed_titles.txt"));
            landedTitlesFile = CleanLandedTitlesFile(landedTitlesFile);

            List<string> landedTitlesFileLines = landedTitlesFile.Split('\n').ToList();
            landedTitlesFileLines.Add(string.Empty);

            IEnumerable<GameId> gameLocationIds = locations.Values
                    .SelectMany(x => x.GameIds)
                    .Where(x => x.Game == Game)
                    .OrderBy(x => x.Id);

            List<string> content = new List<string> { string.Empty };

            
            for (int i = 0; i < landedTitlesFileLines.Count - 1; i++)
            {
                string line = landedTitlesFileLines[i];
                string previousLine = content.Last();
                string nextLine = landedTitlesFileLines[i + 1];

                content.Add(line);

                if (previousLine.Contains("gain_effect") ||
                    previousLine.Contains("allow") ||
                    previousLine.Contains("limit") ||
                    previousLine.Contains("trigger") ||

                    // Be careful with these
                    nextLine.Contains("is_titular") || // Could cause problems, potentially
                    nextLine.Contains("owner_under_ROOT") ||
                    nextLine.Contains("show_scope_change"))
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

            return string.Join(Environment.NewLine, content);
        }

        string GetTitleLocalisationsContent(string line, string gameId)
        {
            IEnumerable<Localisation> localisations = GetGameLocationLocalisations(gameId);

            string indentation = Regex.Match(line, "^(\\s*)" + gameId + "\\s*=\\s*\\{.*$").Groups[1].Value + "\t";
            List<string> lines = new List<string>();

            foreach (Localisation localisation in localisations.OrderBy(x => x.LanguageId))
            {
                string transliteratedName = GetWindows1252Name(localisation.Name);
                lines.Add($"{indentation}{localisation.LanguageId} = \"{transliteratedName}\"");
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

            string newContent = Regex.Replace( // Break inline cultural name into multiple lines
                content,
                "^(\\s*)([ekdcb]_[^\\s]*)\\s*=\\s*\\{\\s*((" + culturesPattern + ")\\s*=\\s*\"*[^\"]*\")\\s*\\}",
                "$1$2 = {\n$1\t$3\n$1}",
                RegexOptions.Multiline);
            
            newContent = Regex.Replace( // Remove cultural names
                newContent,
                "^\\s*(" + culturesPattern + ")\\s*=\\s*\"*[^\"]*\".*\n",
                "",
                RegexOptions.Multiline);

            newContent = Regex.Replace( // Break ={} into multiple lines
                newContent,
                "(^\\s*)([^\\s]*\\s*=\\s*\\{)\\s*\\}",
                "$1$2\n$1}",
                RegexOptions.Multiline);
            
            newContent = Regex.Replace(newContent, "^\\s*\n", "", RegexOptions.Multiline); // Remove empty/whitespace lines
            newContent = Regex.Replace(newContent, "^\\s*#.*\n", "", RegexOptions.Multiline); // Remove comment lines

            newContent = newContent.Replace("\r", "");
            
            return newContent;
        }

        string ReadLandedTitlesFileLines(string filePath)
        {
            Encoding encoding = Encoding.GetEncoding("windows-1252");
            
            return File.ReadAllText(filePath, encoding);
        }

        void WriteLandedTitlesFile(string content, string landedTitlesDirectoryPath)
        {
            string filePath = Path.Combine(landedTitlesDirectoryPath, LandedTitlesFileName);

            Encoding encoding = Encoding.GetEncoding("windows-1252");
            byte[] contentBytes = encoding.GetBytes(content.ToCharArray());
            
            File.WriteAllBytes(filePath, contentBytes);
        }

        string GenerateMainDescriptorContent()
        {
            return GenerateInnerDescriptorContent() + Environment.NewLine +
                $"path = \"mod/{outputSettings.CK2HipModId}\"";
        }

        string GenerateInnerDescriptorContent()
        {
            return
                $"name = \"{outputSettings.CK2HipModName}\"" + Environment.NewLine +
                $"dependencies = {{ \"HIP - Historical Immersion Project\" }}" + Environment.NewLine +
                $"tags = {{ map immersion HIP }}";
        }
    }
}
