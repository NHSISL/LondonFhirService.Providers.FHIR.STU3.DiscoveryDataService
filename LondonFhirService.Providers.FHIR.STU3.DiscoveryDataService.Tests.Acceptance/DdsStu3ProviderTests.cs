// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers;
using Microsoft.Extensions.Configuration;
using Tynamix.ObjectFiller;
using WireMock.Server;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Acceptance
{
    public partial class DdsStu3ProviderTests
    {
        private readonly IDdsStu3Provider ddsStu3Provider;
        private readonly DdsConfigurations ddsConfigurations;
        private readonly WireMockServer wireMockServer;
        private readonly IConfiguration configuration;

        public DdsStu3ProviderTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = configurationBuilder.Build();
            this.wireMockServer = WireMockServer.Start();

            this.ddsConfigurations = configuration
                .GetSection("DdsConfigurations").Get<DdsConfigurations>();

            ddsConfigurations.BaseUrl = wireMockServer.Url;
            ddsConfigurations.AuthorisationUrl = $"{wireMockServer.Url}/authenticate";

            this.ddsStu3Provider = new DdsStu3Provider(ddsConfigurations);
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime()).GetValue();

        private static string GetRandomString() =>
            new MnemonicString().GetValue();

        private static Patient CreateRandomPatient()
        {
            var patient = new Patient();

            HumanName humanName = new HumanName
            {
                ElementId = GetRandomString(),
                Family = GetRandomString(),
                Given = new List<string> { GetRandomString() },
                Prefix = new List<string> { "Mr" },
                Use = HumanName.NameUse.Usual
            };

            patient.Name = new List<HumanName> { humanName };
            patient.Gender = AdministrativeGender.Male;
            patient.BirthDate = GetRandomDateTimeOffset().ToString("yyyy-MM-dd");

            return patient;
        }

        private Bundle CreateRandomBundle()
        {
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Searchset,
                Total = 1
            };

            Patient patient = CreateRandomPatient();

            bundle.Entry = new List<Bundle.EntryComponent>
            {
                new Bundle.EntryComponent
                {
                    FullUrl =
                        $"https://api.service.nhs.uk/personal-demographics/FHIR/STU3/Patient/{patient.Id}",
                    Search = new Bundle.SearchComponent { Score = 1 },
                    Resource = patient
                }
            };

            bundle.Meta = new Meta
            {
                LastUpdated = DateTimeOffset.UtcNow
            };

            return bundle;
        }
    }
}
