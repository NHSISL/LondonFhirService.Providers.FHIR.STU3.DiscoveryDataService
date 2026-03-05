// ---------------------------------------------------------
// Copyright (c) North East London ICB. All rights reserved.
// ---------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using Hl7.Fhir.Model;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    internal interface IDdsHttpBroker
    {
        ValueTask<Bundle> GetStructuredPatientAsync(string requestBody, CancellationToken cancellationToken = default);
    }
}
