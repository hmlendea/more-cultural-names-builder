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
            string normalisedGame = game.ToUpperInvariant().Trim();

            if (normalisedGame.StartsWith("CK2"))
            {
                return serviceProvider.GetService<ICK2ModBuilder>();
            }
            
            if (normalisedGame.StartsWith("CK2HIP"))
            {
                return serviceProvider.GetService<ICK2HIPModBuilder>();
            }
            
            if (normalisedGame.StartsWith("CK3"))
            {
                return serviceProvider.GetService<ICK3ModBuilder>();
            }
            
            if (normalisedGame.StartsWith("HOI4"))
            {
                return serviceProvider.GetService<ICK3ModBuilder>();
            }
            
            if (normalisedGame.StartsWith("IR") ||
                normalisedGame.StartsWith("IMPERATORROME"))
            {
                return serviceProvider.GetService<IImperatorRomeModBuilder>();
            }
            
            throw new NotImplementedException($"The game \"{game}\" is not supported");
        }
    }
}
