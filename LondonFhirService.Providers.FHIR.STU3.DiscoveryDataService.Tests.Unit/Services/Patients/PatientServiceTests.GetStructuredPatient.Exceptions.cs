// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
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
            string randomId = GetRandomString();
            string inputId = randomId;

            var failedServicePatientException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, please contact support.",
                    innerException: failedServicePatientException);

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(inputId))
                    .ThrowsAsync(serviceException);

            // when
            ValueTask<Bundle> getStructuredPatientAsyncAction =
                patientService.GetStructuredPatientAsync(inputId);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredPatientAsyncAction.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientServiceException);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(inputId),
                    Times.Once);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
