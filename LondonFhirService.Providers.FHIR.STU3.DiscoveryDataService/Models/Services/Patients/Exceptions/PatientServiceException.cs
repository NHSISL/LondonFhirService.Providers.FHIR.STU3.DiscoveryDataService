// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions
{
    public class PatientServiceException : Xeption
    {
        public PatientServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
