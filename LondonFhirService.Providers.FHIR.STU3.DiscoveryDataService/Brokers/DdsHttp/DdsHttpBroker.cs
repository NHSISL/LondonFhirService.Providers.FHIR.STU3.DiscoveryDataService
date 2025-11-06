// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using RESTFulSense.Clients;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public class DdsHttpBroker : IDdsHttpBroker
    {
        private readonly DdsHttpConfigurations ddsHttpConfigurations;
        private Task? setupTask = null;
        private readonly IHttpClientFactory httpClientFactory;
        private IRESTFulApiFactoryClient? apiClient = null;
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;

        public DdsHttpBroker(
            DdsHttpConfigurations ddsHttpConfigurations,
            IHttpClientFactory httpClientFactory)
        {
            this.ddsHttpConfigurations = ddsHttpConfigurations;
            this.httpClientFactory = httpClientFactory;
        }

        public async ValueTask<Bundle> GetStructuredPatientAsync(string id)
        {
            var requestBody = new
            {
                meta = new
                {
                    profile = new string[] {
                        "https://fhir.hl7.org.uk/STU3/OperationDefinition/CareConnect-GetStructuredRecord-Operation-1"
                    }
                },
                resourceType = "Parameters",
                parameter = new object[]
                {
                    new
                    {
                        name = "patientNHSNumber",
                        valueIdentifier = new
                        {
                            system = "https://fhir.nhs.uk/Id/nhs-number",
                            value = id
                        }
                    }
                }
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            Bundle bundle =
                await PostAsync<Bundle>($"{ddsHttpConfigurations.BaseUrl}/patient/$get-structured-record", content);

            return bundle;
        }

        private async ValueTask<T> PostAsync<T>(string relativeUrl, StringContent bodyContent)
        {
            if (apiClient is null || DateTimeOffset.UtcNow >= tokenExpiry)
            {
                if (setupTask is null || setupTask.IsCompleted)
                {
                    setupTask = SetupApiClientAsync();
                }

                await setupTask;
            }

            if (apiClient is null)
            {
                throw new InvalidOperationException("Failed to setup API client");
            }

            return await this.apiClient.PostContentAsync<StringContent, T>(relativeUrl, bodyContent);
        }

        private async ValueTask GetAccessTokenAsync()
        {
            using var httpClient = httpClientFactory.CreateClient("TokenClient");

            var requestBody = new
            {
                grantType = "client_credentials",
                clientId = ddsHttpConfigurations.ClientId,
                clientSecret = ddsHttpConfigurations.ClientSecret
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(ddsHttpConfigurations.AuthorisationUrl, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            this.accessToken = doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            var expiresInString = doc.RootElement.GetProperty("expires_in").GetString();

            if (!int.TryParse(expiresInString, out var expiresIn))
            {
                throw new InvalidOperationException("Invalid expires_in value");
            }

            this.tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
        }

        private async Task SetupApiClientAsync()
        {
            await GetAccessTokenAsync();
            HttpClient httpClient = httpClientFactory.CreateClient("ApiClient");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/fhir+json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    scheme: "Bearer",
                    parameter: this.accessToken);

            this.apiClient = new RESTFulApiFactoryClient(httpClient);
        }
    }
}
