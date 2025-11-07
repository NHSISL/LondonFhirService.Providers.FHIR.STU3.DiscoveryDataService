// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using FluentAssertions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldCreateRequestBody()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomDateOfBirth = GetRandomString();
            string inputDateOfBirth = randomDateOfBirth;
            string expectedRequestBody = GetExpectedRequestBody(inputNhsNumber, inputDateOfBirth);

            // when
            string actualRequestBody =
                this.patientService.CreateRequestBody(inputNhsNumber, inputDateOfBirth, false, false);

            // then
            actualRequestBody.Should().BeEquivalentTo(expectedRequestBody);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
