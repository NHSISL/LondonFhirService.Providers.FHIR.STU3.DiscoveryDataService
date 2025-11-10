// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients
{
    public interface IPatientService
    {
        ValueTask<Bundle> GetStructuredPatientAsync(
            string nhsNumber,
            CancellationToken cancellationToken,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false);

        ValueTask<Bundle> EverythingAsync(string id, CancellationToken cancellationToken);
    }
}
