using Microsoft.PowerPlatform.Dataverse.Client;

namespace CoreCore
{
    public interface IServiceClientPool
    {
        ServiceClient ServiceClient { get; }
    }
}
