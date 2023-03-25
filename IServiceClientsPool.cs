using Microsoft.PowerPlatform.Dataverse.Client;
using System;

namespace CoreCore
{
    public interface IServiceClientsPool : IDisposable
    {
        ServiceClient Get();
        int GetAvailableCount();
        void Return(ServiceClient serviceClient);
    }
}
