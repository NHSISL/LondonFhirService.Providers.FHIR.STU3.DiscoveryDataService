// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Foundations.Patients;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Models.Services.Patients.Exceptions;
using Moq;
using RESTFulSense.Exceptions;
using Task = System.Threading.Tasks.Task;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Tests.Unit.Services.Patients
{
    public partial class PatientServiceTests
    {
        [Fact]
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordSerialisedAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordSerialisedWhenCancellationRequestedAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowServiceExceptionOnGetStructuredRecordSerialisedWhenNetworkFailureAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientServiceException actualException =
                await Assert.ThrowsAsync<PatientServiceException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordSerialisedWhenHttpRequestExceptionOccursAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordSerialisedWhenInvalidOperationExceptionOccursAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordSerialisedWhenHttpRequestExceptionHasStatusCodeAsync()
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
            ValueTask<string> getStructuredRecordSerialisedTask =
                mockedPatientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredRecordSerialisedTask.AsTask);

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
        public async Task ShouldThrowDependencyValidationExceptionOnGetStructuredRecordSerialisedWhenPatientIsNotFoundAsync()
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;

            string notFoundTitle =
                $"NotFound:Patient resource not found.  CorrelationId: {GetRandomString()}";

            var httpResponseInternalServerErrorException =
                new HttpResponseInternalServerErrorException(
                    responseMessage: new HttpResponseMessage(HttpStatusCode.InternalServerError),
                    message: notFoundTitle);

            httpResponseInternalServerErrorException.AddData(
                key: "nhsNumber",
                values: inputNhsNumber);

            var notFoundPatientException =
                new NotFoundPatientException(
                    message: notFoundTitle,
                    innerException: httpResponseInternalServerErrorException,
                    data: httpResponseInternalServerErrorException.Data);

            var expectedPatientDependencyValidationException =
                new PatientDependencyValidationException(
                    message: "Patient dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundPatientException);

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(httpResponseInternalServerErrorException);

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                this.patientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyValidationException actualException =
                await Assert.ThrowsAsync<PatientDependencyValidationException>(getStructuredRecordSerialisedTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyValidationException);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyValidationException))),
                        Times.Once);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        public async Task ShouldThrowDependencyExceptionOnGetStructuredRecordSerialisedWhenDependencyRespondsWithAnErrorAsync(
            HttpStatusCode statusCode)
        {
            // given
            string randomNhsNumber = GetRandomString();
            string inputNhsNumber = randomNhsNumber;
            string randomResponseMessage = GetRandomString();

            var httpResponseException =
                new HttpResponseException(
                    httpResponseMessage: new HttpResponseMessage(statusCode),
                    message: randomResponseMessage);

            var failedPatientDependencyException =
                new FailedPatientDependencyException(
                    message: "Failed patient HTTP dependency error occurred - " +
                        $"HTTP {(int)statusCode} ({statusCode}), contact support.",
                    innerException: httpResponseException,
                    data: httpResponseException.Data);

            var expectedPatientDependencyException =
                new PatientDependencyException(
                    message: "Patient service dependency error occurred, contact support.",
                    innerException: failedPatientDependencyException);

            this.ddsHttpBrokerMock.Setup(broker =>
                broker.GetStructuredPatientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(httpResponseException);

            // when
            ValueTask<string> getStructuredRecordSerialisedTask =
                this.patientService.GetStructuredRecordSerialisedAsync(
                    nhsNumber: inputNhsNumber,
                    dateOfBirth: string.Empty,
                    demographicsOnly: false,
                    includeInactivePatients: false,
                    cancellationToken: default);

            PatientDependencyException actualException =
                await Assert.ThrowsAsync<PatientDependencyException>(getStructuredRecordSerialisedTask.AsTask);

            // then
            actualException.Should().BeEquivalentTo(expectedPatientDependencyException);

            this.ddsHttpBrokerMock.Verify(broker =>
                broker.GetStructuredPatientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedPatientDependencyException))),
                        Times.Once);

            this.ddsHttpBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
