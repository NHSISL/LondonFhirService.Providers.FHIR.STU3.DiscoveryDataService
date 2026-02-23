// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Integration
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordSerialisedAsync()
        {
            // given
            string inputNhsNumber = "9435797881";

            // when
            string patient = await this.ddsStu3Provider.Patients
                .GetStructuredRecordSerialisedAsync(nhsNumber: inputNhsNumber, demographicsOnly: false);

            string patientDemographicsOnly = await this.ddsStu3Provider.Patients
                .GetStructuredRecordSerialisedAsync(nhsNumber: inputNhsNumber, demographicsOnly: true);

            patient.Should().NotBeNullOrWhiteSpace();

            int patientDemographicsOnlyLength = patientDemographicsOnly.Length;
            int patientLength = patient.Length;

            patientLength.Should().BeGreaterThan(patientDemographicsOnlyLength);

            output.WriteLine($"Patient JSON length: {patientLength}");
            output.WriteLine($"Patient with demographics only JSON length: {patientDemographicsOnlyLength}");
        }
    }
}