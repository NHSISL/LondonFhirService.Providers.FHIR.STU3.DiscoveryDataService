// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Text.Json;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using Microsoft.Extensions.Configuration;
using Tynamix.ObjectFiller;

namespace LondonFhirService.Providers.FHIR.STU3.DDS.Integration.DeleteMe
{
    public partial class DdsHttpBrokerTempTests
    {
        private readonly IDdsHttpBroker ddsHttpBroker;
        private readonly IConfiguration configuration;

        public DdsHttpBrokerTempTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration = configurationBuilder.Build();

            DdsConfigurations notifyConfigurations = configuration
                .GetSection("DdsConfigurations").Get<DdsConfigurations>();

            this.ddsHttpBroker = new DdsHttpTempBroker(notifyConfigurations);
        }

        private static string GetRandomString() =>
            new MnemonicString(wordCount: GetRandomNumber()).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 2, max: 10).GetValue();

        private static string CreateRequestBody(
            string nhsNumber,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false)
        {
            var parameters = new object[] {
                new
                {
                    name = "patientNHSNumber",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/nhs-number",
                        value = nhsNumber
                    }
                },
                new {
                    name = "demographicsOnly",
                    part = new object[]
                        {
                        new
                        {
                            name = "includeDemographicsOnly",
                            valueBoolean = demographicsOnly
                        }
                    }
                },
                new {
                    name = "includeInactivePatients",
                    part = new object[]
                        {
                        new
                        {
                            name = "includeInactivePatients",
                            valueBoolean = includeInactivePatients
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(dateOfBirth))
            {
                var dobParameter = new
                {
                    name = "patientDOB",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/dob",
                        value = dateOfBirth
                    }
                };

                parameters = parameters.Append(dobParameter).ToArray();
            }

            var requestBody = new
            {
                meta = new
                {
                    profile = new string[] {
                        "https://fhir.hl7.org.uk/STU3/OperationDefinition/CareConnect-GetStructuredRecord-Operation-1"
                    }
                },
                resourceType = "Parameters",
                parameter = parameters
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            return jsonContent;
        }
    }
}