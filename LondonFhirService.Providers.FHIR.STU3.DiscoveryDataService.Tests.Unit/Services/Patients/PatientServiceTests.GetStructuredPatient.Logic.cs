// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using FluentAssertions;
using Hl7.Fhir.Model;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldGetStructuredPatientAsync()
        {
            // given
            string randomId = GetRandomString();
            string inputId = randomId;
            Bundle randomBundle = CreateRandomBundle();
            Bundle expectedBundle = randomBundle;

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(inputId))
                    .ReturnsAsync(randomBundle);

            // when
            Bundle actualBundle =
                await this.patientService.GetStructuredPatientAsync(inputId);

            // then
            actualBundle.Should().BeEquivalentTo(expectedBundle);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(inputId),
                    Times.Once);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
        }
    }
}
