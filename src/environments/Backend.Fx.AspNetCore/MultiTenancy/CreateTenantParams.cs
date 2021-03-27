using Newtonsoft.Json;

namespace Backend.Fx.AspNetCore.MultiTenancy
{
    public class CreateTenantParams
    {
        [JsonProperty(PropertyName = "isDemo")]
        public bool IsDemo { get; set; }

        [JsonProperty(PropertyName = "name")] public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        
        [JsonProperty(PropertyName = "administratorEmail")]
        public string AdministratorEmail { get; set; }

        [JsonProperty(PropertyName = "administratorPassword")]
        public string AdministratorPassword { get; set; }

        [JsonProperty(PropertyName = "configuration")]
        public string Configuration { get; set; }
    }
}