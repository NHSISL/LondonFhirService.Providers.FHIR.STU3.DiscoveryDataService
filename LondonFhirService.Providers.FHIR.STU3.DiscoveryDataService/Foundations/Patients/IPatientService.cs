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
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false,
            CancellationToken cancellationToken = default);

        ValueTask<string> GetStructuredRecordSerialisedAsync(
            string nhsNumber,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false,
            CancellationToken cancellationToken = default);

        ValueTask<Bundle> EverythingAsync(string id, CancellationToken cancellationToken = default);
        ValueTask<string> EverythingSerialisedAsync(string id, CancellationToken cancellationToken = default);
    }
}
