using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace CoreCore
{
    public class ServiceClientsPool : ObjectPool<ServiceClient>, IServiceClientsPool, IDisposable
    {
        private readonly int MaxServiceClient = 10;

        private readonly ServiceClient _serviceClient;
        private readonly ILogger<ServiceClientsPool> _logger;   

        private static ConcurrentQueue<ServiceClient> _serviceClientQueue;
        private static SemaphoreSlim _semaphoreSlim;

        public ServiceClientsPool(IServiceClientPool serviceClientPool, ILogger<ServiceClientsPool> logger)
        {
            _serviceClient = serviceClientPool.ServiceClient;
            _serviceClientQueue = new ConcurrentQueue<ServiceClient>();
            _semaphoreSlim = new SemaphoreSlim(0, MaxServiceClient);
            _semaphoreSlim.Release(MaxServiceClient);

            _serviceClient = serviceClientPool.ServiceClient;
            _logger = logger;

            Enumerable.Range(0, MaxServiceClient).ToList().ForEach(x => _serviceClientQueue.Enqueue(_serviceClient.Clone()));
        }

        public override ServiceClient Get()
        {
            _logger.LogTrace($"{Thread.CurrentThread.ManagedThreadId}, Start to waiting for Semaphore, Count in Queue: {_serviceClientQueue.Count}, Semaphore Count: {_semaphoreSlim.CurrentCount}");
            _semaphoreSlim.Wait();
            _logger.LogTrace($"{Thread.CurrentThread.ManagedThreadId}, Entter Semaphore, Count in Queue: {_serviceClientQueue.Count}, Semaphore Count: {_semaphoreSlim.CurrentCount}");
            try
            {
                if (_serviceClientQueue.TryDequeue(out var serviceClient))
                {
                     return serviceClient;
                }

                return default(ServiceClient);
            }
            finally
            { 
                _logger.LogTrace($"{Thread.CurrentThread.ManagedThreadId}, Left Semaphore, Count in Queue: {_serviceClientQueue.Count}, Semaphore Count: {_semaphoreSlim.CurrentCount}");
            }
        }

        public override void Return(ServiceClient obj)
        {
            _serviceClientQueue.Enqueue(obj);
            _semaphoreSlim.Release();
        }      

        public void Dispose()
        {
            _serviceClientQueue.ToList().ForEach(service => service.Dispose()) ;
            _semaphoreSlim?.Dispose();
        }

        public int GetAvailableCount()
        {
            return _serviceClientQueue.Count;
        }
    }
}
