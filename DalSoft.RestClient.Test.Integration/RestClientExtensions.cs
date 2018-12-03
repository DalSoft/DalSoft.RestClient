using System.Threading.Tasks;
using DalSoft.RestClient.Test.Integration.TestModels;

namespace DalSoft.RestClient.Test.Integration
{
    public static class RestClientExtensions
    {
        //  Strongly typed RestClient extension method on resource, verb left up to user
        public static IHttpMethods Users(this RestClient restClient, int id) => restClient.Resource("users").Resource(id.ToString());

        // Strongly typed RestClient extension method full verb
        public static async Task<User> GetUserById(this RestClient restClient, int id) => await restClient.Resource("users").Resource(id.ToString()).Get();

        // dynamic extension method full verb
        public static async Task<User> GetUserByIdDynamic(this RestClient restClient, int id) => await ((dynamic)restClient).users(id).Get();
        
    }
}