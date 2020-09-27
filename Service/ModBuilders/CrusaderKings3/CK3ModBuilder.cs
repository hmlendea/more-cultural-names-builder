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

        readonly INameNormaliser nameNormaliser;

        public CK3ModBuilder(
            ILocalisationFetcher localisationFetcher,
            INameNormaliser nameNormaliser,
            IRepository<LanguageEntity> languageRepository,
            IRepository<LocationEntity> locationRepository,
            OutputSettings outputSettings)
            : base(localisationFetcher, nameNormaliser, languageRepository, locationRepository, outputSettings)
        {
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
                lines.Add($"{indentation2}{localisation.LanguageGameId} = \"{normalisedName}\"");
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
    }
}
