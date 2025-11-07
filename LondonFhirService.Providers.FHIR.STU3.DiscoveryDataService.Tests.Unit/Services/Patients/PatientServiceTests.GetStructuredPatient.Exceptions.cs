// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredPatientAsync()
        {
            // given
            var serviceException = new Exception(GetRandomString());
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomDateOfBirth = GetRandomString();
            string inputDateOfBirth = randomDateOfBirth;

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
                service.CreateRequestBody(inputNhsNumber, inputDateOfBirth, false, false))
                    .Throws(serviceException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientAsyncAction =
                mockedPatientService.GetStructuredPatientAsync(inputNhsNumber, inputDateOfBirth, false, false);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredPatientAsyncAction.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    inputDateOfBirth,
                    false,
                    false),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
