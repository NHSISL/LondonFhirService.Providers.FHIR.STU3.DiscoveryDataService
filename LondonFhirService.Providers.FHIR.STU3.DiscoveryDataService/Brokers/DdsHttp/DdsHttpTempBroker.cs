// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public sealed class DdsHttpTempBroker : IDdsHttpBroker, IDisposable
    {
        private readonly DdsConfigurations ddsConfigurations;
        private readonly HttpClient tokenHttp;
        private readonly HttpClient apiHttp;
        private readonly SemaphoreSlim tokenGate = new(1, 1);
        private readonly FhirJsonDeserializer fhirJsonDeserializer = new();

        private string accessToken = string.Empty;
        private DateTimeOffset tokenExpiry = DateTimeOffset.MinValue;
        private bool disposed;

        public DdsHttpTempBroker(DdsConfigurations ddsConfigurations)
        {
            this.ddsConfigurations =
                ddsConfigurations ?? throw new ArgumentNullException(nameof(ddsConfigurations));

            this.tokenHttp = new HttpClient();
            this.apiHttp = new HttpClient();

            if (!string.IsNullOrWhiteSpace(ddsConfigurations.BaseUrl))
            {
                this.apiHttp.BaseAddress = new Uri(ddsConfigurations.BaseUrl, UriKind.Absolute);
            }

            this.apiHttp.DefaultRequestHeaders.TryAddWithoutValidation(
                "Accept",
                "application/fhir+json");
        }

        public async ValueTask<Bundle> GetStructuredPatientAsync(
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

            var body = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            Console.WriteLine($"Status: {(int)response.StatusCode} {response.StatusCode}");
            Console.WriteLine(body);

            response.EnsureSuccessStatusCode();

            string json = await response
                .Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            return this.fhirJsonDeserializer.Deserialize<Bundle>(json);
        }

        private async ValueTask EnsureAccessTokenAsync(CancellationToken cancellationToken)
        {
            // Fast path: still valid
            if (!string.IsNullOrEmpty(this.accessToken)
                && DateTimeOffset.UtcNow < this.tokenExpiry)
            {
                return;
            }

            await this.tokenGate.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Double-check inside the gate
                if (string.IsNullOrEmpty(this.accessToken)
                    || DateTimeOffset.UtcNow >= this.tokenExpiry)
                {
                    await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

                    //this.apiHttp.DefaultRequestHeaders.Authorization =
                    //    new AuthenticationHeaderValue("Bearer", this.accessToken);

                    // Clear any legacy headers
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
            var body = new
            {
                grantType = "client_credentials",
                clientId = this.ddsConfigurations.ClientId,
                clientSecret = this.ddsConfigurations.ClientSecret
            };

            string jsonContent = JsonSerializer.Serialize(body);

            using var content = new StringContent(
                jsonContent,
                Encoding.UTF8,
                "application/json");

            using var response = await this.tokenHttp
                .PostAsync(this.ddsConfigurations.AuthorisationUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            string json = await response
                .Content
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

            // Refresh slightly early to avoid edge-of-expiry failures
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
