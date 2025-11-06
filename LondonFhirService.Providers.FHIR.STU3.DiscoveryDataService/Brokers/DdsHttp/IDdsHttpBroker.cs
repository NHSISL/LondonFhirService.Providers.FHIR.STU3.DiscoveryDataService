using Hl7.Fhir.Model;
using System.Threading.Tasks;

namespace LondonFhirService.Providers.FHIR.STU3.DiscoveryDataService.Brokers.DdsHttp
{
    public interface IDdsHttpBroker
    {
        ValueTask<Bundle> GetStructuredPatientAsync(string id);
    }
}
