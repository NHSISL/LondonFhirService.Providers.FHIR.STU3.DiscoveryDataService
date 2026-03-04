// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using FluentAssertions;
using Hl7.Fhir.Model;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Integration
{
    public partial class DdsStu3ProviderTests
    {
        [Fact]
        public async Task ShouldEverythingAsync()
        {
            // given
            string inputId = "9435797881";

            // when
            Bundle patient = await this.ddsStu3Provider.Patients.EverythingAsync(id: inputId);
            patient.Should().NotBeNull();
        }
    }
}