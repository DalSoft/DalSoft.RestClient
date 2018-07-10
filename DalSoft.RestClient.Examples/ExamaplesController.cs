using System.Collections.Generic;
using System.Threading.Tasks;
using DalSoft.RestClient.DependencyInjection;
using DalSoft.RestClient.Examples.Models;
using Microsoft.AspNetCore.Mvc;

namespace DalSoft.RestClient.Examples
{
    public class ExamaplesController : Controller
    {
        private readonly IRestClientFactory _restClientFactory;
        
        public ExamaplesController(IRestClientFactory restClientFactory)
        {
            _restClientFactory = restClientFactory;
        }

        [Route("examples/createclient"), HttpGet]
        public async Task<List<Repository>> CreateClient()
        {
            dynamic restClient = _restClientFactory.CreateClient();
            
            var repositories = await restClient.users.dalsoft.repos.Get();
            
            return repositories;
        }
        [Route("examples/createclientnamed"), HttpGet]
        public async Task<List<Repository>> CreateClientNamed()
        {
            dynamic restClient = _restClientFactory.CreateClient("NamedGitHubClient");
            
            var repositories = await restClient.dotnet.repos.Get();
            
            return repositories;
        }
    }
}
