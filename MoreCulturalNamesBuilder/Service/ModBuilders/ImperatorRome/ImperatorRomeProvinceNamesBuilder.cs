using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.Service.Models;
using System.Collections.Generic;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeProvinceNamesBuilder(
        INameNormaliser nameNormaliser,
        Settings settings) : IImperatorRomeProvinceNamesBuilder
    {
        public void CreateProvinceNameFiles(
            string outputDirectoryPath,
            IDictionary<string, IDictionary<string, Localisation>> localisations,
            IEnumerable<GameId> languageGameIds)
        {
            string provinceNamesDirectoryPath = Path.Combine(outputDirectoryPath, settings.Mod.Id, "common", "province_names");

            Directory.CreateDirectory(provinceNamesDirectoryPath);

            Parallel.ForEach(languageGameIds, languageGameId =>
            {
                string path = Path.Combine(provinceNamesDirectoryPath, $"{languageGameId.Id.ToLower()}.txt");
                string content = $"{languageGameId.Id} = {{" + Environment.NewLine;

                foreach (string provinceId in localisations.Keys
                    .OrderBy(x => int.Parse(x))
                    .Where(x => localisations[x].ContainsKey(languageGameId.Id)))
                {
                    Localisation localisation = localisations[provinceId][languageGameId.Id];

                    content += $"    {localisation.GameId} = PROV{localisation.GameId}_{languageGameId.Id} # {nameNormaliser.ToImperatorRomeCharset(localisation.Name)}";

                    if (settings.Output.AreVerboseCommentsEnabled)
                    {
                        content += $" # Language={localisation.LanguageId}";
                    }

                    if (!string.IsNullOrWhiteSpace(localisation.Comment))
                    {
                        content += $" # {localisation.Comment}";
                    }

                    content += Environment.NewLine;
                }

                content += "}";

                File.WriteAllText(path, content);
            });
        }
    }
}
