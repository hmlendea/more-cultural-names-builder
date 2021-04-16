using System.IO;
using System.Reflection;

namespace MoreCulturalNamesModBuilder.Configuration
{
    public sealed class ApplicationPaths
    {
        public static readonly string ApplicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}
