using System;
using System.IO;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.UnitTests.TestInfrastructure
{
    internal sealed class TemporaryDirectory : IDisposable
    {
        internal string DirectoryPath { get; }

        internal TemporaryDirectory(string testFixtureName)
        {
            DirectoryPath = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                testFixtureName,
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(DirectoryPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }
}