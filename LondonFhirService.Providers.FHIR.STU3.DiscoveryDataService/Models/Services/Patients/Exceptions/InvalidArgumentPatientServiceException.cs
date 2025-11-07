// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions
{
    public class InvalidArgumentPatientServiceException : Xeption
    {
        public InvalidArgumentPatientServiceException(string message)
            : base(message)
        { }
    }
}
