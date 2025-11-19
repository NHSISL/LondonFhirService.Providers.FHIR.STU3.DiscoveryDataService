// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public class DdsHttpBroker : IDdsHttpBroker, IDisposable
    {
        private readonly DdsConfigurations ddsConfigurations;
        private readonly HttpClient tokenHttp;
        private readonly HttpClient apiHttp;
        private readonly SemaphoreSlim tokenGate = new(1, 1);
        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;
        private bool disposed;

        public DdsHttpBroker(DdsConfigurations ddsConfigurations)
        {
            this.ddsConfigurations =
                ddsConfigurations ?? throw new ArgumentNullException(nameof(ddsConfigurations));

            this.tokenHttp = new HttpClient();
            this.apiHttp = new HttpClient();

            if (!string.IsNullOrWhiteSpace(ddsConfigurations.BaseUrl))
            {
                this.apiHttp.BaseAddress = new Uri(ddsConfigurations.BaseUrl, UriKind.Absolute);
            }

            this.apiHttp.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/fhir+json");
        }

        public async ValueTask<string> GetStructuredPatientAsync(
            string requestBody,
            CancellationToken cancellationToken = default)
        {
            await EnsureAccessTokenAsync(cancellationToken).ConfigureAwait(false);

            using var content = new StringContent(
                requestBody,
                Encoding.UTF8,
                "application/json");

            using var response = await this.apiHttp
                .PostAsync("fhirTestAPI/patient/$getstructuredrecord", content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string json = await response
                .Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return json;
        }

        private async ValueTask EnsureAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(this.accessToken)
                && DateTimeOffset.UtcNow < this.tokenExpiry)
            {
                return;
            }

            await this.tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (string.IsNullOrEmpty(this.accessToken)
                    || DateTimeOffset.UtcNow >= this.tokenExpiry)
                {
                    await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                    this.apiHttp.DefaultRequestHeaders.Clear();

                    this.apiHttp.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/fhir+json"));

                    this.apiHttp.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    this.apiHttp.DefaultRequestHeaders.Remove("Authorization");
                    this.apiHttp.DefaultRequestHeaders.Add("Authorization", accessToken);
                }
            }
            finally
            {
                this.tokenGate.Release();
            }
        }

        private async ValueTask GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                grantType = "client_credentials",
                clientId = ddsConfigurations.ClientId,
                clientSecret = ddsConfigurations.ClientSecret
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await this.tokenHttp
                .PostAsync(ddsConfigurations.AuthorisationUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var json = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            this.accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("Access token is null");

            if (!root.GetProperty("expires_in").TryGetInt32(out int expiresIn))
            {
                throw new InvalidOperationException("Invalid expires_in value");
            }

            this.tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 30);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.tokenGate.Dispose();
            this.tokenHttp.Dispose();
            this.apiHttp.Dispose();
        }
    }
}
