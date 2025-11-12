// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using Hl7.Fhir.Model;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DDS.Integration.DeleteMe
{
    public partial class DdsHttpBrokerTempTests
    {
        [Fact]
        public async Task ShouldPatientLookupByDetailsAsync()
        {
            // given
            string inputNhsNumber = "5558526785";
            string body = CreateRequestBody(nhsNumber: inputNhsNumber);

            // when
            Bundle result = await this.ddsHttpBroker.GetStructuredPatientAsync(requestBody: body);

            // then
            Assert.NotNull(result);
        }
    }
}