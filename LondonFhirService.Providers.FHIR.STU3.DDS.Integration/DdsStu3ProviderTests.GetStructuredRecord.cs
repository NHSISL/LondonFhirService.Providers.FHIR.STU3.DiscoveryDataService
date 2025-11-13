// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading.Tasks;

namespace LondonFhirService.Providers.FHIR.STU3.DDS.Integration
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldGetStructuredRecordAsync()
        {
            // given
            string inputNhsNumber = "5558526785";

            // when
            await this.ddsStu3Provider.Patients.GetStructuredRecord(nhsNumber: inputNhsNumber);
        }
    }
}