// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients
{
    public partial class PatientService
    {
        private delegate ValueTask<Bundle> ReturningBundleFunction();
        private delegate ValueTask<string> ReturningStringFunction();

        private async ValueTask<Bundle> TryCatch(ReturningBundleFunction returningBundleFunction)
        {
            try
            {
                return await returningBundleFunction();
            }
            catch (InvalidArgumentPatientServiceException invalidArgumentPatientServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientServiceException);
            }
        }

        private async ValueTask<string> TryCatch(ReturningStringFunction returningStringFunction)
        {
            try
            {
                return await returningStringFunction();
            }
            catch (InvalidArgumentPatientServiceException invalidArgumentPatientServiceException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidArgumentPatientServiceException);
            }
            catch (Exception exception)
            {
                var failedPatientServiceException =
                    new FailedPatientServiceException(
                        message: "Failed patient service error occurred, please contact support.",
                        innerException: exception,
                        data: exception.Data);

                throw await CreateAndLogServiceExceptionAsync(failedPatientServiceException);
            }
        }

        private async ValueTask<PatientValidationException> CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var pdsValidationException = new PatientValidationException(
                message: "Patient validation error occurred, please fix the errors and try again.",
                innerException: exception);

            return pdsValidationException;
        }

        private async ValueTask<PatientServiceException> CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var pdsServiceException = new PatientServiceException(
                message: "Patient service error occurred, please contact support.",
                innerException: exception);

            return pdsServiceException;
        }
    }
}
