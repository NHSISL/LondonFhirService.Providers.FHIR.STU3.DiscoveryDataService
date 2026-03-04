// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers;
using Microsoft.Extensions.Configuration;
using Tynamix.ObjectFiller;
using Xunit.Abstractions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Integration
{
    public partial class DdsStu3ProviderTests
    {
        private readonly IDdsStu3Provider ddsStu3Provider;
        private readonly IConfiguration configuration;
        private readonly ITestOutputHelper output;

        public DdsStu3ProviderTests(ITestOutputHelper output)
        {
            this.output = output;

            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = configurationBuilder.Build();

            DdsConfigurations notifyConfigurations = configuration
                .GetSection("DdsConfigurations").Get<DdsConfigurations>();

            this.ddsStu3Provider = new DdsStu3Provider(notifyConfigurations);
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();
    }
}