// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers
{
    public sealed class DdsStu3Provider : FhirProviderBase, IFhirProvider
    {
        private readonly string source;
        private readonly string code;
        private readonly string system;
        private IPatientResource patientResource { get; set; }

        public DdsStu3Provider(
            DdsConfigurations ddsConfigurations, IPatientResource patientResource)
        {
            this.source = ddsConfigurations.Source;
            this.code = ddsConfigurations.Code;
            this.system = ddsConfigurations.System;
            this.patientResource = patientResource;
        }

        public override string Source => this.source;

        public override string Code => this.Code;

        public override string System => this.System;

        public override IPatientResource Patients => this.patientResource;
    }
}
