using Newtonsoft.Json;

namespace DalSoft.RestClient.Test.Unit.TestData.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string email { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        public string website { get; set; }
    }

    public class UserCamelCase
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
    }
}