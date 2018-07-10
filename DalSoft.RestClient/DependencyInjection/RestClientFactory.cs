using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DalSoft.RestClient.DependencyInjection
{
    public class RestClientFactory : IRestClientFactory
    {
        internal const string DefaultClientName = "tVRtg8GreQMrsF6g";
        private readonly IServiceProvider _serviceProvider;

        public RestClientFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public RestClient CreateClient()
        {
            return CreateClient(DefaultClientName);
        }

        public RestClient CreateClient(string name)
        {
            var matchingContainers = _serviceProvider.GetServices<RestClientContainer>().Where(_ => _.Name == name).ToList();

            if (matchingContainers.Count > 1)
                throw new InvalidOperationException($"More than one registered RestClient named: {name}");

            if (!matchingContainers.Any())
                throw new InvalidOperationException($"No registered RestClient named: {name}");

            return matchingContainers.Single().RestClient();
        }
    }
}
