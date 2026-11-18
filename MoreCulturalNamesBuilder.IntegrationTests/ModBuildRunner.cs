using System;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class ModBuildRunner
    {
        internal static void Run(string outputDirectoryPath, ModBuildRequest request)
            => Run(TestArguments.Build(outputDirectoryPath, request));

        internal static void Run(string[] arguments)
        {
            try
            {
                Program.Main(arguments);
            }
            finally
            {
                DisposeServiceProvider();
            }
        }

        private static void DisposeServiceProvider()
        {
            if (Program.ServiceProvider is IDisposable serviceProvider)
            {
                serviceProvider.Dispose();
            }
        }
    }
}
