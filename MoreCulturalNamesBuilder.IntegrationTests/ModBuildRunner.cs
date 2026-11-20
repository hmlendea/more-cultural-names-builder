using System;

namespace MoreCulturalNamesBuilder.IntegrationTests
{
    internal static class ModBuildRunner
    {
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

        internal static void Run(string outputDirectoryPath, ModBuildRequest request)
            => Run(TestArguments.Build(outputDirectoryPath, request));

        private static void DisposeServiceProvider()
        {
            if (Program.ServiceProvider is IDisposable serviceProvider)
            {
                serviceProvider.Dispose();
            }
        }
    }
}
