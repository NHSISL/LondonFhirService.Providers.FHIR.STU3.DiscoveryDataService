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
        public async Task ShouldGetStructuredRecordSerialisedAsync()
        {
            // given
            string inputNhsNumber = "9435797881";

            // when
            await this.ddsStu3Provider.Patients.GetStructuredRecordSerialisedAsync(nhsNumber: inputNhsNumber);
        }
    }
}