// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using LondonFhirService.Providers.FHIR.STU3.Abstractions;
using LondonFhirService.Providers.FHIR.STU3.Abstractions.Models.Resources;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Brokers.DdsHttp;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers
{
    public sealed class DdsStu3Provider : FhirProviderBase, IDdsStu3Provider
    {
        private readonly DdsConfigurations configurations;
        private IPatientResource patientResource { get; set; }

        public DdsStu3Provider(DdsConfigurations configurations)
        {
            this.configurations = configurations;
            IServiceProvider serviceProvider = RegisterServices(configurations);
            InitializeClients(serviceProvider);
        }

        public override string Source => this.configurations.Source;
        public override string Code => this.configurations.Code;
        public override string System => this.configurations.System;
        public override IPatientResource Patients => this.patientResource;

        public override string DisplayName => "Discovery Data Service FHIR® Care Connect API";

        public override string FhirVersion => "STU3";

        private void InitializeClients(IServiceProvider serviceProvider) =>
            this.patientResource = serviceProvider.GetRequiredService<IPatientResource>();

        private static IServiceProvider RegisterServices(DdsConfigurations configurations)
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<IDdsHttpBroker, DdsHttpBroker>()
                .AddTransient<IPatientService, PatientService>()
                .AddTransient<IPatientResource, PatientResource>()
                .AddSingleton(configurations);

            HttpClientFactoryServiceCollectionExtensions.AddHttpClient(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            return serviceProvider;
        }
    }
}
