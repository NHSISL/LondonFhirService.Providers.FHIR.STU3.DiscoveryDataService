// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Acceptance
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldThrowDependencyValidationExceptionOnGetStructuredRecordSerialisedWhenPatientIsNotFoundAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomAccessToken = GetRandomString();

            string notFoundTitle =
                $"NotFound:Patient resource not found.  CorrelationId: {GetRandomString()}";

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

            // The body the dependency answers with for a patient it does not hold - a 500, not a
            // 404, told apart from a genuine fault only by its title.
            var notFoundProblemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = notFoundTitle,
                status = (int)HttpStatusCode.InternalServerError,
                errors = new Dictionary<string, string[]>
                {
                    ["nhsNumber"] = new[] { inputNhsNumber }
                }
            };

            this.wireMockServer
                .Given(
                    Request
                        .Create()
                        .WithPath("/authenticate/$gettoken")
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
                        .WithPath($"/{this.ddsConfigurations.GetStructuredRecordRelativeUrl}")
                        .UsingPost())

                .RespondWith(
                    Response
                        .Create()
                        .WithStatusCode(HttpStatusCode.InternalServerError)
                        .WithHeader("Content-Type", "application/problem+json")
                        .WithBodyAsJson(notFoundProblemDetails));

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                this.ddsStu3Provider.Patients.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber);

            PatientDependencyValidationException actualException =
                await Assert.ThrowsAsync<PatientDependencyValidationException>(
                    getStructuredRecordSerialisedTask.AsTask);

            // then
            actualException.InnerException.Should().BeOfType<NotFoundPatientException>();
            actualException.InnerException.Message.Should().Be(notFoundTitle);

            actualException.InnerException.Data["nhsNumber"].Should()
                .BeEquivalentTo(new[] { inputNhsNumber });
        }
    }
}
