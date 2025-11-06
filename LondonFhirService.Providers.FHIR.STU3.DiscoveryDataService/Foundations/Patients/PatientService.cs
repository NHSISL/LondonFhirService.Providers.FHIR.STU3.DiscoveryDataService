// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Hl7.Fhir.Model;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients
{
    public partial class PatientService : IPatientService
    {
        private readonly IDdsHttpBroker ddsHttpBroker;

        public PatientService(IDdsHttpBroker ddsHttpBroker) =>
            this.ddsHttpBroker = ddsHttpBroker;

        public ValueTask<Bundle> GetStructuredPatientAsync(string id) =>
            TryCatch(async () =>
            {
                ValidateArgsOnGetStructuredPatient(id);

                return await this.ddsHttpBroker.GetStructuredPatientAsync(id);
            });
    }
}
