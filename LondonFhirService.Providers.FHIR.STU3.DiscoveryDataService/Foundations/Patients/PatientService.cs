// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Linq;
using System.Text.Json;
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

        public ValueTask<Bundle> GetStructuredPatientAsync(
            string nhsNumber,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false) =>
        TryCatch(async () =>
        {
            ValidateArgsOnGetStructuredPatient(nhsNumber);

            string requestBody = CreateRequestBody(
                nhsNumber,
                dateOfBirth,
                demographicsOnly,
                includeInactivePatients);

            return await this.ddsHttpBroker.GetStructuredPatientAsync(requestBody);
        });

        virtual internal string CreateRequestBody(
            string nhsNumber,
            string dateOfBirth = "",
            bool demographicsOnly = false,
            bool includeInactivePatients = false)
        {
            var parameters = new object[] {
                new
                {
                    name = "patientNHSNumber",
                    valueIdentifier = new
                    {
                        system = "https://fhir.nhs.uk/Id/nhs-number",
                        value = nhsNumber
                    }
                },
                new {
                    name = "demographicsOnly",
                    part = new object[]
                        {
                        new
                        {
                            name = "includeDemographicsOnly",
                            valueBoolean = demographicsOnly
                        }
                    }
                },
                new {
                    name = "includeInactivePatients",
                    part = new object[]
                        {
                        new
                        {
                            name = "includeInactivePatients",
                            valueBoolean = includeInactivePatients
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(dateOfBirth))
            {
                var dobParameter = new
                {
                    name = "patientDOB",
                    valueIdentifier = new
                    {
                        system = "https://fhir.hl7.org.uk/Id/dob",
                        value = dateOfBirth
                    }
                };

                parameters = parameters.Append(dobParameter).ToArray();
            }

            var requestBody = new
            {
                meta = new
                {
                    profile = new string[] {
                        "https://fhir.hl7.org.uk/STU3/OperationDefinition/CareConnect-GetStructuredRecord-Operation-1"
                    }
                },
                resourceType = "Parameters",
                parameter = parameters
            };

            string jsonContent = JsonSerializer.Serialize(requestBody);

            return jsonContent;
        }
    }
}
