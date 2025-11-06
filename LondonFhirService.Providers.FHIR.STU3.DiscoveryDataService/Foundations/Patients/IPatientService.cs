// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients
{
    public interface IPatientService
    {
        ValueTask<Bundle> GetStructuredPatientAsync(string id);
    }
}
