// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using RESTFulSense.Clients;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public class DdsHttpBroker : IDdsHttpBroker
    {
        private readonly DdsConfigurations ddsConfigurations;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly SemaphoreSlim setupGate = new(1, 1);
        private IRESTFulApiFactoryClient? apiClient = null;
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;

        public DdsHttpBroker(
            DdsConfigurations ddsHttpConfigurations,
            IHttpClientFactory httpClientFactory)
        {
            this.ddsConfigurations = ddsHttpConfigurations;
            this.httpClientFactory = httpClientFactory;
        }

        public async ValueTask<Bundle> GetStructuredPatientAsync(
            string requestBody,
            CancellationToken cancellationToken)
        {
            using var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            await EnsureClientAsync(cancellationToken);


            //TODO : Add cancellation token support to RESTFulApiFactoryClient
            return await apiClient!.PostContentAsync<StringContent, Bundle>(
                $"{ddsConfigurations.BaseUrl}/patient/$get-structured-record",
                content);
        }

        private async Task EnsureClientAsync(CancellationToken cancellationToken)
        {
            if (apiClient is not null && DateTimeOffset.UtcNow < tokenExpiry)
            {
                return;
            }

            await setupGate.WaitAsync().ConfigureAwait(false);

            try
            {
                if (apiClient is null || DateTimeOffset.UtcNow >= tokenExpiry)
                {
                    await SetupApiClientAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                setupGate.Release();
            }
        }

        private async ValueTask GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var httpClient = httpClientFactory.CreateClient("TokenClient");

            var requestBody = new
            {
                grantType = "client_credentials",
                clientId = ddsConfigurations.ClientId,
                clientSecret = ddsConfigurations.ClientSecret
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(ddsConfigurations.AuthorisationUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            this.accessToken = doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            var expiresInString = doc.RootElement.GetProperty("expires_in").GetString();

            if (expiresInString is null || !int.TryParse(expiresInString, out var expiresIn))
            {
                throw new InvalidOperationException("Invalid expires_in value");
            }

            this.tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
        }

        private async Task SetupApiClientAsync(CancellationToken cancellationToken)
        {
            await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
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
