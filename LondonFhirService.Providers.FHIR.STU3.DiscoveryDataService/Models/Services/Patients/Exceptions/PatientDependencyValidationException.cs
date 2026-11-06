// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Exceptions;
using Xeptions;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions
{
    /// <summary>
    /// Marked as a FHIR dependency exception, as PatientDependencyException is, rather than as a
    /// validation exception. Consumers choose their own handling - and the status they answer
    /// with - from the marker, and a patient the dependency does not hold reached them as a
    /// dependency exception before this category existed. Keeping the marker keeps that.
    /// </summary>
    public class PatientDependencyValidationException : Xeption, IFhirDependencyException
    {
        public PatientDependencyValidationException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
