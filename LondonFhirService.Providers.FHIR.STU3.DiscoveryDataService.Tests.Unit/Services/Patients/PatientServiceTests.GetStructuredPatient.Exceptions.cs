// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredPatientAsync()
        {
            // given
            var serviceException = new Exception(GetRandomString());
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var failedServicePatientException =
                new FailedPatientServiceException(
                    message: "Failed patient service error occurred, please contact support.",
                    innerException: serviceException,
                    data: serviceException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, please contact support.",
                    innerException: failedServicePatientException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(serviceException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredPatientWhenCancellationRequestedAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            var taskCanceledException =
                new TaskCanceledException(
                    message: "Task was cancelled.",
                    innerException: null,
                    token: cancellationTokenSource.Token);

            var cancelledPatientServiceException =
                new CancelledPatientServiceException(
                    message: "Patient service was cancelled, please try again.",
                    innerException: taskCanceledException,
                    data: taskCanceledException.Data);

            var expectedPatientDependencyException =
                new PatientDependencyException(
                    message: "Patient dependency cancellation error occurred, please try again.",
                    innerException: cancelledPatientServiceException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(taskCanceledException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredPatientWhenNetworkFailureAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var taskCanceledException = new TaskCanceledException();

            var failedPatientServiceException =
                new FailedPatientServiceException(
                    message: "Network connectivity failure occurred, please check connection and try again.",
                    innerException: taskCanceledException,
                    data: taskCanceledException.Data);

            var expectedPatientServiceException =
                new PatientServiceException(
                    message: "Patient service error occurred, please contact support.",
                    innerException: failedPatientServiceException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(taskCanceledException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientServiceException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientServiceException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredPatientWhenHttpRequestExceptionOccursAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var httpRequestException = new HttpRequestException();

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient HTTP dependency error occurred - " +
                        "no HTTP status code returned (possible network failure), contact support.",
                    innerException: httpRequestException,
                    data: httpRequestException.Data);

            var expectedPatientDependencyException =
                new PatientDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(httpRequestException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredPatientWhenInvalidOperationExceptionOccursAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            var invalidOperationException = new InvalidOperationException();

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient dependency error occurred, contact support.",
                    innerException: invalidOperationException,
                    data: invalidOperationException.Data);

            var expectedPatientDependencyException =
                new PatientDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(invalidOperationException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredPatientWhenHttpRequestExceptionHasStatusCodeAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            var httpRequestException =
                new HttpRequestException(
                    message: "Service Unavailable",
                    inner: null,
                    statusCode: HttpStatusCode.ServiceUnavailable);

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient HTTP dependency error occurred - " +
                        $"HTTP {(int)HttpStatusCode.ServiceUnavailable} (ServiceUnavailable), contact support.",
                    innerException: httpRequestException,
                    data: httpRequestException.Data);

            var expectedPatientDependencyException =
                new PatientDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            var patientServiceMock = new Mock<PatientService>(
                this.ddsHttpBrokerMock.Object,
                this.loggingBrokerMock.Object)
            {
                CallBase = true
            };

            patientServiceMock.Setup(service =>
                service.CreateRequestBody(inputNhsNumber, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                    .Throws(httpRequestException);

            PatientService mockedPatientService = patientServiceMock.Object;

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                mockedPatientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredPatientTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyException);

            patientServiceMock.Verify(service =>
                service.CreateRequestBody(
                    inputNhsNumber,
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyException))),
                        Times.Once);

            patientServiceMock.VerifyNoOtherCalls();
            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredPatientWhenFhirDeserializationFailsAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string invalidFhirJson = "{ \"resourceType\": \"OperationOutcome\" }";

            this.ddsHttpBrokerMock
                .Setup(broker => broker.GetStructuredPatientAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(invalidFhirJson);

            // when
            ValueTask<Bundle> getStructuredPatientTask =
                this.patientService.GetStructuredPatientAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredPatientTask.AsTask);

            // then
            actualException.InnerException.Should().BeOfType<FailedPatientDependencyException>();
            actualException.InnerException.InnerException.Should().BeOfType<DeserializationFailedException>();

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                        Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(actualException))),
                    Times.Once);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
