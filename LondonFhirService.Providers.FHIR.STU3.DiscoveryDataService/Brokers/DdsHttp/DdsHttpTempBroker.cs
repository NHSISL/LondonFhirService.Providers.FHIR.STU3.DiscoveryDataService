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

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public sealed class DdsHttpTempBroker : IDdsHttpBroker, IDisposable
    {
        private readonly DdsConfigurations ddsConfigurations;
        private readonly HttpClient tokenHttp;
        private readonly HttpClient apiHttp;
        private readonly SemaphoreSlim setupGate = new(1, 1);

        private IRESTFulApiFactoryClient? apiClient;
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;
        private bool disposed;

        public DdsHttpTempBroker(DdsConfigurations ddsConfigurations)
        {
            this.ddsConfigurations = ddsConfigurations ?? throw new ArgumentNullException(nameof(ddsConfigurations));

            this.tokenHttp = new HttpClient();
            this.apiHttp = new HttpClient();

            // Set default configuration
            if (!string.IsNullOrWhiteSpace(ddsConfigurations.BaseUrl))
            {
                this.apiHttp.BaseAddress = new Uri(ddsConfigurations.BaseUrl, UriKind.Absolute);
            }

            this.apiHttp.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/fhir+json");
        }

        public async ValueTask<Bundle> GetStructuredPatientAsync(
            string requestBody,
            CancellationToken cancellationToken = default)
        {
            using var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

            await EnsureClientAsync(cancellationToken).ConfigureAwait(false);

            // TODO: Add cancellation token support to RESTFulApiFactoryClient
            return await apiClient!.PostContentAsync<StringContent, Bundle>(
                $"patient/$getstructuredrecord",
                content);
        }

        private async ValueTask EnsureClientAsync(CancellationToken cancellationToken)
        {
            if (apiClient is not null && DateTimeOffset.UtcNow < tokenExpiry)
            {
                return;
            }

            await setupGate.WaitAsync(cancellationToken).ConfigureAwait(false);
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
            var body = new
            {
                grantType = "client_credentials",
                clientId = ddsConfigurations.ClientId,
                clientSecret = ddsConfigurations.ClientSecret
            };

            string jsonContent = JsonSerializer.Serialize(body);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            using var response = await tokenHttp.PostAsync(ddsConfigurations.AuthorisationUrl, content, cancellationToken)
                                               .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            using var doc = JsonDocument.Parse(json);

            accessToken = doc.RootElement.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            if (!doc.RootElement.GetProperty("expires_in").TryGetInt32(out var expiresIn))
            {
                throw new InvalidOperationException("Invalid expires_in value");
            }

            tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
        }

        private async ValueTask SetupApiClientAsync(CancellationToken cancellationToken)
        {
            await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            apiHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            apiClient ??= new RESTFulApiFactoryClient(apiHttp);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            setupGate.Dispose();
            tokenHttp.Dispose();
            apiHttp.Dispose();
            (apiClient as IDisposable)?.Dispose();
        }
    }
}
