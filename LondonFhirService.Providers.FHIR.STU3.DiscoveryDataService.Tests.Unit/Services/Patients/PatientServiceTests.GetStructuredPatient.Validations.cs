// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ShouldThrowValidationExceptionOnGetStructuredPatientAsync(string invalidText)
        {
            // given
            var invalidArgumentPatientServiceException =
                new InvalidArgumentPatientServiceException(
                    "Invalid patient service argument, please correct the errors and try again.");

            invalidArgumentPatientServiceException.AddData(
                key: "Id",
                values: "Text is required");

            var expectedPatientValidationException =
                new PatientValidationException(
                    message: "Patient validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentPatientServiceException);

            // when
            ValueTask<Bundle> getStructuredPatientAsyncAction =
                patientService.GetStructuredPatientAsync(invalidText);

            PatientValidationException actualException =
                await Assert.ThrowsAsync<PatientValidationException>(getStructuredPatientAsyncAction.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientValidationException);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
