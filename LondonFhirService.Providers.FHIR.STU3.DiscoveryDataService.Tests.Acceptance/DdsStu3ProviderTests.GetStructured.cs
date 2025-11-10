// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using FluentAssertions;
using Force.DeepCloner;
using Hl7.Fhir.Model;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Acceptance
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var authenticateRequestBody = new
            {
                grantType = "client_credentials",
                clientId = ddsConfigurations.ClientId,
                clientSecret = ddsConfigurations.ClientSecret
            };

            var expectedAuthenticateResponse = new
            {
                accessToken = GetRandomString(),
                tokenType = "Bearer",
                expiresIn = 60
            };

            string requestBody = GetRequestBody(randomNhsNumber);
            Bundle bundleResponse = CreateRandomBundle();
            Bundle expectedResponse = bundleResponse.DeepClone();
            var requestPath = "$get-structured-record";

            this.wireMockServer
                .Given(
                    Request.Create()
                        .WithPath("")
                        .WithBody(authenticateRequestBody)
                        .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithSuccess()
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(expectedAuthenticateResponse));

            this.wireMockServer
                .Given(
                    Request.Create()
                        .WithPath(requestPath)
                        .WithBody(requestBody)
                        .UsingPost()
                        .WithHeader("X-REQUEST-ID", expectedAuthenticateResponse.accessToken))
                .RespondWith(
                    Response.Create()
                        .WithSuccess()
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(bundleResponse));

            // when
            Bundle actualResponse = await ddsStu3Provider.Patients.GetStructuredRecord("some-patient-id");

            // then
            actualResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
