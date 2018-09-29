namespace Backend.Fx.AspNetCore.Security
{
    public class ContentSecurityPolicyOptions
    {
        public string ContentSecurityPolicy { get; set; }
        public bool ReportOnly { get; set; }
        public string ReportUrl { get; set; }
    }
}