// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Resources
{
    public class PatientResource : ResourceOperationBase<Patient>, IPatientResource
    {
        private readonly IPatientService patientService;

        public PatientResource(IPatientService patientService)
        {
            this.patientService = patientService;
        }

        [FhirOperation]
        public async ValueTask<Bundle> Everything(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
                await patientService.EverythingAsync(id, cancellationToken);

        [FhirOperation]
        public ValueTask<Bundle> GetStructuredRecord(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
                patientService.GetStructuredPatientAsync(
                    nhsNumber,
                    dateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
                    demographicsOnly ?? false,
                    includeInactivePatients ?? false,
                    cancellationToken);
    }
}
