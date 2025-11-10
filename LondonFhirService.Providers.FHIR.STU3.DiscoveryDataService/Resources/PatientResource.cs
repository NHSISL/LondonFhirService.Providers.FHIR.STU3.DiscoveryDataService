// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Capabilities;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Resources
{
    public class PatientResource : IPatientResource
    {
        private readonly IPatientService patientService;

        public PatientResource(IPatientService patientService)
        {
            this.patientService = patientService;
        }

        public ResourceCapabilities Capabilities =>
            new ResourceCapabilities
            {
                ResourceName = "Patients",
                SupportedOperations = new List<string> { "Everything", "GetStructuredRecord" }
            };

        public ValueTask<Patient> Create(Patient resource, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<OperationOutcome> Delete(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Bundle> Everything(
            string id,
            DateTimeOffset? start = null,
            DateTimeOffset? end = null,
            string typeFilter = null,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default) =>
                await patientService.EverythingAsync(id, cancellationToken);

        public ValueTask<Bundle> GetStructuredRecord(
            string nhsNumber,
            DateTime? dateOfBirth = null,
            bool? demographicsOnly = null,
            bool? includeInactivePatients = null,
            CancellationToken cancellationToken = default) =>
                patientService.GetStructuredPatientAsync(
                    nhsNumber,
                    cancellationToken,
                    dateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty,
                    demographicsOnly ?? false,
                    includeInactivePatients ?? false);

        public ValueTask<Bundle> HistoryInstance(
            string id,
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Bundle> HistorySystem(
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Bundle> HistoryType(
            DateTimeOffset? since = null,
            int? count = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Bundle> Match(Parameters parameters, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Patient> Patch(
            string id,
            Parameters patchParameters,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Patient> Read(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Bundle> Search(SearchParams @params, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Patient> Update(Patient resource, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Patient> VRead(string id, string versionId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
