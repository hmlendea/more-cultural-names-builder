using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using NuciDAL.Repositories;
using NuciExtensions;

using MoreCulturalNamesModBuilder.Configuration;
using MoreCulturalNamesModBuilder.DataAccess.DataObjects;
using MoreCulturalNamesModBuilder.Service.Models;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public sealed class CK3ModBuilder : CK2ModBuilder
    {
        protected override string OutputLandedTitlesFileName => "999_MoreCulturalNames.txt";

        protected override List<string> ForbiddenTokensForPreviousLine => new List<string> { "allow", "limit", "trigger" };
        protected override List<string> ForbiddenTokensForNextLine => new List<string> { "has_holder" };

        readonly ILocalisationFetcher localisationFetcher;
        readonly INameNormaliser nameNormaliser;
        readonly InputSettings inputSettings;

        public CK3ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            IRepository<TitleEntity> titleRepository,
            ModSettings modSettings,
            InputSettings inputSettings,
            OutputSettings outputSettings)
            : base(localisationFetcher, nameNormaliser, languageRepository, locationRepository, titleRepository, modSettings, inputSettings, outputSettings)
        {
            this.localisationFetcher = localisationFetcher;
            this.nameNormaliser = nameNormaliser;
            this.inputSettings = inputSettings;
        }

        protected override string GenerateMainDescriptorContent()
        {
            return GenerateDescriptorContent() + Environment.NewLine +
                $"path=\"mod/{modSettings.Id}\"";
        }

        protected override string GenerateDescriptorContent()
        {
            return
                $"# Version {modSettings.Version} ({DateTime.Now})" + Environment.NewLine +
                $"name=\"{modSettings.Name}\"" + Environment.NewLine +
                $"version=\"{modSettings.Version}\"" + Environment.NewLine +
                $"supported_version=\"{modSettings.GameVersion}\"" + Environment.NewLine +
                $"tags={{" + Environment.NewLine +
                $"    \"Culture\"" + Environment.NewLine +
                $"    \"Historical\"" + Environment.NewLine +
                $"    \"Map\"" + Environment.NewLine +
                $"    \"Translation\"" + Environment.NewLine +
                $"}}";
        }

        protected override string ReadLandedTitlesFile()
        {
            return File.ReadAllText(inputSettings.LandedTitlesFilePath);
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
                string lineToAdd = $"{indentation2}{localisation.LanguageGameId} = \"{normalisedName}\"";

                if (outputSettings.AreVerboseCommentsEnabled)
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

        // TODO: This shouldn't exist
        protected override string GenerateTitlesLocalisationFile(GameId languageGameId)
        {
            return null;
        }

        // TODO: This shouldn't exist
        protected override void CreateTitlesLocalisationFiles()
        {
            
        }

        protected override void CreateDescriptorFiles()   
        {	
            string mainDescriptorContent = GenerateMainDescriptorContent();
            string innerDescriptorContent = GenerateDescriptorContent();
            
            string mainDescriptorFilePath = Path.Combine(OutputDirectoryPath, $"{modSettings.Id}.mod");
            string innerDescriptorFilePath = Path.Combine(OutputDirectoryPath, modSettings.Id, "descriptor.mod");	

            File.WriteAllText(mainDescriptorFilePath, mainDescriptorContent);	
            File.WriteAllText(innerDescriptorFilePath, innerDescriptorContent);	
        }

        void WriteFileWithByteOrderMark(string filePath, string content)
        {
            File.WriteAllText(filePath, content + '\uFEFF');
        }
    }
}
