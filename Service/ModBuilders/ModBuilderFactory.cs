using System;

using Microsoft.Extensions.DependencyInjection;

using MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings2;
using MoreCulturalNamesModBuilder.Service.ModBuilders.CrusaderKings3;
using MoreCulturalNamesModBuilder.Service.ModBuilders.ImperatorRome;

namespace MoreCulturalNamesModBuilder.Service.ModBuilders
{
    public sealed class ModBuilderFactory : IModBuilderFactory
    {
        readonly IServiceProvider serviceProvider;

        public ModBuilderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IModBuilder GetModBuilder(string game)
        {
            switch (game.ToUpperInvariant().Trim())
            {
                case "CK2":
                    return serviceProvider.GetService<ICK2ModBuilder>();
                case "CK2HIP":
                    return serviceProvider.GetService<ICK2HIPModBuilder>();
                case "CK3":
                    return serviceProvider.GetService<ICK3ModBuilder>();
                case "HOI4":
                    return serviceProvider.GetService<ICK3ModBuilder>();
                case "IMPERATORROME":
                    return serviceProvider.GetService<IImperatorRomeModBuilder>();
                default:
                    throw new NotImplementedException($"The game \"{game}\" is not supported");
            }
        }
    }
}
