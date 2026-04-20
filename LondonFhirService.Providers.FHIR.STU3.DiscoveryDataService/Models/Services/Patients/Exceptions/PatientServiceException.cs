// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Exceptions;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions
{
    public class PatientServiceException : Xeption, IFhirServiceException
    {
        public PatientServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
