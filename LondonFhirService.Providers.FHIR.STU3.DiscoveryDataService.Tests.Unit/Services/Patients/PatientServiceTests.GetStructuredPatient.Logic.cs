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
        public async Task ShouldGetStructuredPatientAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomDateOfBirth = GetRandomString();
            string inputDateOfBirth = randomDateOfBirth;
            string expectedRequestBody = GetExpectedRequestBody(inputNhsNumber, inputDateOfBirth);
            string inputRequestBody = expectedRequestBody;
            CancellationToken inputCancellationToken = default;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle;

            var patientServiceMock = new Mock<PatientService>(this.ddsHttpBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    inputDateOfBirth,
                    false,
                    false))
                        .Returns(expectedRequestBody);

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(inputRequestBody, inputCancellationToken))
                    .ReturnsAsync(randomBundle);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            Bundle actualBundle = await mockedPatientService.GetStructuredPatientAsync(
                nhsNumber: inputNhsNumber,
                dateOfBirth: inputDateOfBirth,
                demographicsOnly: false,
                includeInactivePatients: false,
                cancellationToken: inputCancellationToken);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    inputDateOfBirth,
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
