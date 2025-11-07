// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients
{
    public partial class PatientService
    {
        private static void ValidateArgsOnGetStructuredPatient(string nhsNumber)
        {
            Validate(
                createException: () => new InvalidArgumentPatientServiceException(
                    message: "Invalid patient service argument, please correct the errors and try again."),

                (Rule: IsInvalid(nhsNumber), Parameter: "nhsNumber"));
        }

        private static void ValidateArgsOnEverything(string id)
        {
            Validate(
                createException: () => new InvalidArgumentPatientServiceException(
                    message: "Invalid patient service argument, please correct the errors and try again."),

                (Rule: IsInvalid(id), Parameter: "id"));
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required"
        };

        private static void Validate<T>(
            Func<T> createException,
            params (dynamic Rule, string Parameter)[] validations)
            where T : Xeption
        {
            T invalidDataException = createException();

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDataException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDataException.ThrowIfContainsErrors();
        }
    }
}
