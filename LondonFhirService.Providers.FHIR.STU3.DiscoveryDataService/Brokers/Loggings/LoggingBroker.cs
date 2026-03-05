// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System;
using System.Threading.Tasks;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Extensions.Exceptions;
using LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Providers;
using Microsoft.Extensions.Logging;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.Loggings
{
    internal class LoggingBroker : ILoggingBroker
    {
        private readonly ILogger<DdsStu3Provider> logger;

        public LoggingBroker(ILogger<DdsStu3Provider> logger) =>
            this.logger = logger;

        public async ValueTask LogInformationAsync(string message) =>
            logger.LogInformation(message);

        public async ValueTask LogTraceAsync(string message) =>
            logger.LogTrace(message);

        public async ValueTask LogDebugAsync(string message) =>
            logger.LogDebug(message);

        public async ValueTask LogWarningAsync(string message) =>
            logger.LogWarning(message);

        public async ValueTask LogErrorAsync(Exception exception) =>
            logger.LogError(exception, $"{exception.Message} {exception.GetValidationSummary()}");

        public async ValueTask LogCriticalAsync(Exception exception) =>
            logger.LogCritical(exception, $"{exception.Message} {exception.GetValidationSummary()}");
    }
}