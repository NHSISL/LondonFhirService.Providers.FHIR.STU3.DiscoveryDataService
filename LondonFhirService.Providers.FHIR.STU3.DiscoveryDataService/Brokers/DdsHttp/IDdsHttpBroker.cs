// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public interface IDdsHttpBroker
    {
        ValueTask<string> GetStructuredPatientAsync(string requestBody, CancellationToken cancellationToken = default);
    }
}
