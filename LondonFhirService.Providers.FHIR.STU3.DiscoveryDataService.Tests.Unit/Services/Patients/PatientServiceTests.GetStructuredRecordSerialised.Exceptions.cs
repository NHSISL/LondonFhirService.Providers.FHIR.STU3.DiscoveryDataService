// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordSerialisedAsync()
        {
            // given
            var serviceException = new Exception(GetRandomString());
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var failedServicePatientException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, please contact support.",
                    innerException: failedServicePatientException);

            var patientServiceMock = new Mock<PatientService>(this.ddsHttpBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(serviceException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredRecordSerialisedTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
