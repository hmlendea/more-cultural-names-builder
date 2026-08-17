using System;
using System.Reflection;

using MoreCulturalNamesBuilder.Service;

namespace MoreCulturalNamesBuilder.UnitTests.TestInfrastructure
{
    internal static class InternalMappingInvoker
    {
        private static string MappingNamespace => "MoreCulturalNamesBuilder.Service.Mapping";

        internal static TOutput Invoke<TOutput>(
            string mappingTypeName,
            string methodName,
            object input)
        {
            Type mappingType = typeof(LocalisationFetcher).Assembly.GetType(
                $"{MappingNamespace}.{mappingTypeName}",
                true)!;
            MethodInfo method = mappingType.GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static)!;

            return (TOutput)method.Invoke(null, [input])!;
        }
    }
}