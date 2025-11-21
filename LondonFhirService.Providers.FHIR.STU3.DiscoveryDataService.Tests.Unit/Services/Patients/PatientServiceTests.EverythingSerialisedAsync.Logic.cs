// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using FluentAssertions;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldEverythingSerialisedAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            string expectedRequestBody = GetExpectedRequestBody(inputId);
            string inputRequestBody = expectedRequestBody;
            CancellationToken inputCancellationToken = default;
            string randomJson = GetRandomString();
            string expectedJson = randomJson;


            var patientServiceMock = new Mock<PatientService>(this.ddsHttpBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(
                    inputId,
                    string.Empty,
                    false,
                    false))
                        .Returns(expectedRequestBody);

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(inputRequestBody, inputCancellationToken))
                    .ReturnsAsync(randomJson);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            string actualJson =
                await mockedPatientService.EverythingSerialisedAsync(
                    id: inputId,
                    cancellationToken: inputCancellationToken);

            // then
            actualJson.Should().BeEquivalentTo(randomJson);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputId,
                    string.Empty,
                    false,
                    false),
                        Times.Once);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(inputRequestBody, inputCancellationToken),
                    Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
