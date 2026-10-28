using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

using NUnit.Framework;

namespace MoreCulturalNamesBuilder.ArchitectureTests
{
    [TestFixture]
    public sealed class ProjectConventionTests
    {
        [TestCase("Debug")]
        [TestCase("Release")]
        public void EvaluatedProjectsMustEnforceRequiredBuildPropertiesAndReferences(string configuration)
        {
            foreach (string projectName in RepositorySources.ProjectNames)
            {
                using JsonDocument evaluated = EvaluateProject(projectName, configuration);
                JsonElement properties = evaluated.RootElement.GetProperty("Properties");
                bool isTestProject = !string.Equals(projectName, "MoreCulturalNamesBuilder");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(properties.GetProperty("TargetFramework").GetString(), Is.EqualTo("net10.0"), projectName);
                    Assert.That(properties.GetProperty("ImplicitUsings").GetString(), Is.EqualTo("disable"), projectName);
                    Assert.That(properties.GetProperty("LangVersion").GetString(), Is.EqualTo("latest"), projectName);
                    Assert.That(properties.GetProperty("Nullable").GetString(), Is.EqualTo("enable"), projectName);
                    Assert.That(properties.GetProperty("IsPackable").GetString(), Is.EqualTo("false"), projectName);

                    if (isTestProject)
                    {
                        Assert.That(properties.GetProperty("IsTestProject").GetString(), Is.EqualTo("true"), projectName);
                        Assert.That(properties.GetProperty("ExcludeFromCodeCoverage").GetString(), Is.EqualTo("true"), projectName);
                    }

                    if (!isTestProject)
                    {
                        AssertProductionReferences(evaluated.RootElement.GetProperty("Items"), projectName);
                    }
                }
            }
        }

        [Test]
        public void DeclaredProjectPropertiesAndPackagesMustFollowConventions()
        {
            foreach (string projectName in RepositorySources.ProjectNames)
            {
                XDocument project = XDocument.Load(GetProjectPath(projectName));
                string[] packageNames = project.Descendants("PackageReference")
                    .Select(package => package.Attribute("Include")!.Value).ToArray();

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(packageNames, Is.Ordered.Using<string>(StringComparer.OrdinalIgnoreCase), projectName);
                    Assert.That(project.Descendants("TargetFramework").Select(element => element.Value),
                        Does.Contain("net10.0"), projectName);

                    foreach (string property in new[] { "ImplicitUsings", "LangVersion", "Nullable", "IsPackable" })
                    {
                        Assert.That(project.Descendants(property), Is.Not.Empty, $"{projectName}: Missing {property}.");
                    }

                    Assert.That(project.Descendants("InternalsVisibleTo"), Is.Empty, projectName);
                }
            }
        }

        [Test]
        public void SolutionMustInventorySiblingProjectsInThePrescribedOrder()
        {
            string path = Path.Combine(RepositorySources.Root, "MoreCulturalNamesBuilder.slnx");
            XDocument solution = XDocument.Load(path);
            XElement root = solution.Root!;
            string[] expected = RepositorySources.ProjectNames
                .Select(name => $"{name}/{name}.csproj").ToArray();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(solution.Declaration, Is.Null, "The SDK-style solution must not have an XML declaration.");
                Assert.That(root.Name.LocalName, Is.EqualTo("Solution"));
                Assert.That(root.Attributes(), Is.Empty);
                Assert.That(root.Elements().Select(element => element.Name.LocalName), Has.All.EqualTo("Project"));
                Assert.That(root.Elements("Project").Select(element => element.Attribute("Path")?.Value),
                    Is.EqualTo(expected), "Production first, then alphabetically ordered test projects.");
                Assert.That(Directory.GetFiles(RepositorySources.Root, "*.sln"), Is.Empty);

                foreach (XElement project in root.Elements("Project"))
                {
                    Assert.That(project.Attributes().Select(attribute => attribute.Name.LocalName),
                        Is.EqualTo(new[] { "Path" }));
                    Assert.That(File.Exists(Path.Combine(RepositorySources.Root, project.Attribute("Path")!.Value)));
                }

                string[] discovered = Directory.GetDirectories(RepositorySources.Root)
                    .SelectMany(directory => Directory.GetFiles(directory, "*.csproj"))
                    .Select(project => Path.GetRelativePath(RepositorySources.Root, project)
                        .Replace(Path.DirectorySeparatorChar, '/'))
                    .Order(StringComparer.Ordinal).ToArray();
                Assert.That(discovered, Is.EquivalentTo(expected), "New projects require explicit architecture inventory.");
            }
        }

        [TestCase("Debug")]
        [TestCase("Release")]
        public void RequiredAnalyserRulesMustBeEnabledAndEnforced(string configuration)
        {
            foreach (string projectName in RepositorySources.ProjectNames)
            {
                using JsonDocument evaluated = EvaluateProject(projectName, configuration);
                JsonElement properties = evaluated.RootElement.GetProperty("Properties");

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(properties.GetProperty("EnableNETAnalyzers").GetString(), Is.EqualTo("true"), projectName);
                    Assert.That(properties.GetProperty("EnforceCodeStyleInBuild").GetString(), Is.EqualTo("true"), projectName);
                    Assert.That(properties.GetProperty("TreatWarningsAsErrors").GetString(), Is.EqualTo("true"), projectName);
                }
            }
        }

        private static string GetProjectPath(string projectName) =>
            Path.Combine(RepositorySources.Root, projectName, $"{projectName}.csproj");

        private static JsonDocument EvaluateProject(string projectName, string configuration)
        {
            ProcessStartInfo start = new("dotnet")
            {
                WorkingDirectory = RepositorySources.Root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            start.ArgumentList.Add("msbuild");
            start.ArgumentList.Add(GetProjectPath(projectName));
            start.ArgumentList.Add($"-property:Configuration={configuration}");
            start.ArgumentList.Add("-getProperty:TargetFramework,ImplicitUsings,LangVersion,Nullable,IsPackable," +
                "IsTestProject,ExcludeFromCodeCoverage,EnableNETAnalyzers,EnforceCodeStyleInBuild,TreatWarningsAsErrors");
            start.ArgumentList.Add("-getItem:ProjectReference,Reference,PackageReference,InternalsVisibleTo,AssemblyAttribute");
            using Process process = Process.Start(start)!;
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            Assert.That(process.WaitForExit(60000), $"MSBuild evaluation timed out: {projectName}.");
            Assert.That(process.ExitCode, Is.Zero, $"MSBuild evaluation failed: {projectName}: {errors}");

            return JsonDocument.Parse(output);
        }

        private static void AssertProductionReferences(JsonElement items, string projectName)
        {
            Assert.That(items.GetProperty("InternalsVisibleTo").EnumerateArray(), Is.Empty, projectName);
            Assert.That(items.GetProperty("ProjectReference").EnumerateArray(), Is.Empty,
                "The documented single-production-project architecture has no production project dependencies.");
            string[] allowedPackages =
            [
                "Microsoft.Extensions.Configuration",
                "Microsoft.Extensions.Configuration.Binder",
                "Microsoft.Extensions.Configuration.Json",
                "Microsoft.Extensions.DependencyInjection",
                "NuciCLI.Arguments",
                "NuciDAL",
                "NuciExtensions",
                "NuciText.Conversion"
            ];
            Assert.That(items.GetProperty("PackageReference").EnumerateArray()
                .Select(package => package.GetProperty("Identity").GetString()),
                Is.EquivalentTo(allowedPackages), "Changes to production dependencies require architecture review.");

            foreach (string itemName in new[] { "ProjectReference", "Reference", "PackageReference", "AssemblyAttribute" })
            {
                foreach (JsonElement item in items.GetProperty(itemName).EnumerateArray())
                {
                    string identity = item.GetProperty("Identity").GetString()!;
                    Assert.That(identity, Does.Not.Contain("Tests").And.Not.Contain("NUnit")
                        .And.Not.Contain("Moq").And.Not.Contain("InternalsVisibleTo"),
                        $"{projectName}: Production must not reference testing infrastructure ({identity}).");
                }
            }
        }
    }
}
