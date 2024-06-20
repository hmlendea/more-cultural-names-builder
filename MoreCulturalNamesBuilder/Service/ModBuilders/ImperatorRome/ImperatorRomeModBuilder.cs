using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NuciDAL.Repositories;

using MoreCulturalNamesBuilder.Configuration;
using MoreCulturalNamesBuilder.DataAccess.DataObjects;
using MoreCulturalNamesBuilder.Service.Models;

namespace MoreCulturalNamesBuilder.Service.ModBuilders.ImperatorRome
{
    public sealed class ImperatorRomeModBuilder(
        ILocalisationFetcher localisationFetcher,
        IRepository<LanguageEntity> languageRepository,
        IRepository<LocationEntity> locationRepository,
        IImperatorRomeDescriptorBuilder descriptorBuilder,
        IImperatorRomeLocalisationBuilder localisationBuilder,
        IImperatorRomeProvinceNamesBuilder provinceNamesBuilder,
        Settings settings) : ModBuilder(languageRepository, locationRepository, settings)
    {
        IDictionary<string, IDictionary<string, Localisation>> localisations;

        protected override void LoadData()
        {
            ConcurrentDictionary<string, IDictionary<string, Localisation>> concurrentLocalisations = new();

            Parallel.ForEach(locationGameIds, locationGameId =>
            {
                IDictionary<string, Localisation> locationLocalisations = localisationFetcher
                    .GetGameLocationLocalisations(locationGameId.Id, Settings.Mod.Game)
                    .ToDictionary(x => x.LanguageGameId, x => x);

                concurrentLocalisations.TryAdd(locationGameId.Id, locationLocalisations);
            });

            localisations = concurrentLocalisations.ToDictionary(x => x.Key, x => x.Value);
        }

        protected override void GenerateFiles()
        {
            LoadData();

            provinceNamesBuilder.CreateProvinceNameFiles(OutputDirectoryPath, localisations, languageGameIds);
            localisationBuilder.CreateLocalisationFiles(OutputDirectoryPath, localisations, locationGameIds);
            descriptorBuilder.CreateDescriptorFiles(OutputDirectoryPath);
        }
    }
}
