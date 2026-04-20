// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Exceptions;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions
{
    public class PatientValidationException : Xeption, IFhirValidationException
    {
        public PatientValidationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
