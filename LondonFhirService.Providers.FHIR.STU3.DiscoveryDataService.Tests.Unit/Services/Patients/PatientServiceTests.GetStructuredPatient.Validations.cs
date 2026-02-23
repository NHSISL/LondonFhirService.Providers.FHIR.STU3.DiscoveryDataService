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
                key: "nhsNumber",
                values: "Text is required");

            var expectedPatientValidationException =
                new PatientValidationException(
                    message: "Patient validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentPatientServiceException);

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                patientService.GetStructuredPatientAsync(
                    nhsNumber: invalidText,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientValidationException actualException =
                await Assert.ThrowsAsync<PatientValidationException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientValidationException);
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("2002-24-01")]
        [InlineData("24-01-2002")]
        public async Task ShouldThrowValidationExceptionOnGetStructuredPatientWhenDateIsInvalidAsync(string dateString)
        {
            // given
            string nhsNumber = "1234567890";

            var invalidArgumentPatientServiceException =
                new InvalidArgumentPatientServiceException(
                    "Invalid patient service argument, please correct the errors and try again.");

            invalidArgumentPatientServiceException.AddData(
                key: "dateOfBirth",
                values: "Text must be a valid date string in format 'yyyy-MM-dd' e.g. '2002-10-01'");

            var expectedPatientValidationException =
                new PatientValidationException(
                    message: "Patient validation error occurred, please fix the errors and try again.",
                    innerException: invalidArgumentPatientServiceException);

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                patientService.GetStructuredPatientAsync(
                    nhsNumber: nhsNumber,
                    dateOfBirth: dateString,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientValidationException actualException =
                await Assert.ThrowsAsync<PatientValidationException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientValidationException);
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
