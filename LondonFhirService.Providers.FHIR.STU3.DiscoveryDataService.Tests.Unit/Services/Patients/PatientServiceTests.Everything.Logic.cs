// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldEverythingAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            string expectedRequestBody = GetExpectedRequestBody(inputId);
            string inputRequestBody = expectedRequestBody;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle;

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
                broker.GetStructuredPatientAsync(inputRequestBody, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(randomBundle);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            Bundle actualBundle =
                await mockedPatientService.EverythingAsync(inputId, default);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputId,
                    string.Empty,
                    false,
                    false),
                        Times.Once);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(inputRequestBody, It.IsAny<CancellationToken>()),
                    Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
