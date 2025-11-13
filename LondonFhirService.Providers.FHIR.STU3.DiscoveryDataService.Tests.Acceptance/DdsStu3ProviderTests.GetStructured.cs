// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Net;
using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Acceptance
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            Bundle bundleResponse = CreateRandomBundle();
            Bundle expectedResponse = bundleResponse.DeepClone();
            string randomAccessToken = GetRandomString();

            var authenticateRequestBody = new
            {
                grantType = "client_credentials",
                clientId = ddsConfigurations.ClientId,
                clientSecret = ddsConfigurations.ClientSecret
            };

            var expectedAuthenticateResponse = new
            {
                access_token = randomAccessToken,
                token_type = "Bearer",
                expires_in = 60
            };

            var fhirJsonSerializer = new FhirJsonSerializer();

            string expectedBundleResponse =
                fhirJsonSerializer.SerializeToString(bundleResponse);

            this.wireMockServer
                .Given(
                    Request
                        .Create()
                        .WithPath("/authenticate")
                        .WithBodyAsJson(authenticateRequestBody)
                        .UsingPost())

                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(HttpStatusCode.OK)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(expectedAuthenticateResponse));

            this.wireMockServer
                .Given(
                    Request
                        .Create()
                        .WithPath("/patient/$get-structured-record")
                        .UsingPost())

                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(HttpStatusCode.OK)
                        .WithHeader("Content-Type", "application/fhir+json")
                        .WithBody(expectedBundleResponse));

            // when
            Bundle actualResponse =
                await ddsStu3Provider.Patients.GetStructuredRecord(
                    nhsNumber: inputNhsNumber);

            // then
            actualResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
