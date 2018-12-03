namespace DalSoft.RestClient.Test.Unit.TestData.Resources
{
    public class UsersResources
    {
        public string Departments => $"users/{nameof(Departments)}";
        
        public string GetUser(int id)
        {
            return $"users/{id}";
        }
    }
}