using Microsoft.PowerPlatform.Dataverse.Client;
using System;

namespace CoreCore
{
    public class ServiceClientPool : IServiceClientPool
    {
        public ServiceClient ServiceClient => _lazyServiceClient.Value;

        #region Private Properties
        private Lazy<ServiceClient> _lazyServiceClient;
        private ServiceClient _serviceClient;
        #endregion

        public ServiceClientPool()
        {
            var ConnectionString = @"authtype=OAuth;username=;password=;url=;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d; RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;loginPrompt=Auto";
            _lazyServiceClient = new Lazy<ServiceClient>(() => GetServiceClient(ConnectionString), true);
        }

        #region Private Methods
        private ServiceClient GetServiceClient(string connectionString)
        {
            _serviceClient = new ServiceClient(connectionString);
            return _serviceClient.IsReady ? _serviceClient : default;
        }
        #endregion
    }
}
