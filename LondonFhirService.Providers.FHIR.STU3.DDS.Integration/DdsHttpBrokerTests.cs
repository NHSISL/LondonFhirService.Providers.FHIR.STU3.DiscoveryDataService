// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Text.Json;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using Microsoft.Extensions.DependencyInjection;

namespace LondonFhirService.Providers.FHIR.STU3.DDS.Integration
{
    public class DdsHttpBrokerTests
    {
        [Fact]
        public async System.Threading.Tasks.Task ShouldGetStructuredPatientAsync()
        {
            // given
            var parameters = new object[] {
                new
                {
                    name = "patientNHSNumber",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/nhs-number",
                        value = "5558526785"
                    }
                },
                new {
                    name = "demographicsOnly",
                    part = new object[]
                        {
                        new
                        {
                            name = "includeDemographicsOnly",
                            valueBoolean = false
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
                            valueBoolean = false
                        }
                    }
                }
            };

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

            var ddsConfigurations = new DdsConfigurations
            {
                BaseUrl = "https://devfhirapi.discoverydataservice.net:8443/fhirTestAPI",
                AuthorisationUrl = "https://devauthentication.discoverydataservice.net/authenticate/$gettoken",
                ClientId = "fhir-api",
                ClientSecret = "6d8e1310-5e9c-4b62-b314-79fbd9c735d5",
                Source = "DDS(London Discovery Data Service)",
                Code = "DDS",
                System = "https://devfhirapi.discoverydataservice.net/"
            };

            var serviceCollection = new ServiceCollection()
                .AddSingleton(ddsConfigurations);

            HttpClientFactoryServiceCollectionExtensions.AddHttpClient(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            var broker = new DdsHttpBroker(ddsConfigurations, httpClientFactory);

            // when
            Bundle actualBundle = await broker.GetStructuredPatientAsync(jsonContent);
        }
    }
}
