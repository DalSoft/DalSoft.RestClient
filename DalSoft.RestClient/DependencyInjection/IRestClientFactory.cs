namespace DalSoft.RestClient.DependencyInjection
{
    public interface IRestClientFactory
    {
        RestClient CreateClient();
        RestClient CreateClient(string name);
    }
}