// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Infrastructure.Services;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Infrastructure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scriptGenerationService = new ScriptGenerationService();

            scriptGenerationService.GenerateBuildScript(
                branchName: "main",
                projectName: "LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService",
                dotNetVersion: "10.0.100");

            scriptGenerationService.GeneratePrLintScript(branchName: "main");
        }
    }
}
