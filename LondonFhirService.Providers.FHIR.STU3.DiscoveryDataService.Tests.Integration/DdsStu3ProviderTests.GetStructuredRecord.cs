// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using Xunit;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Integration
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordAsync()
        {
            // given
            string inputNhsNumber = "9435797881";

            // when
            Bundle patient = await this.ddsStu3Provider.Patients.GetStructuredRecordAsync(nhsNumber: inputNhsNumber);

            Bundle patientWithDemographicsOnly =
                await this.ddsStu3Provider.Patients
                    .GetStructuredRecordAsync(nhsNumber: inputNhsNumber, demographicsOnly: true);

            patient.Should().NotBeNull();
            patientWithDemographicsOnly.Should().NotBeNull();
            patientWithDemographicsOnly.Entry.Should().HaveCountLessThan(patient.Entry.Count);
            output.WriteLine($"Patient entry count: {patient.Entry.Count}");
            output.WriteLine($"Patient with demographics only entry count: {patientWithDemographicsOnly.Entry.Count}");
        }
    }
}