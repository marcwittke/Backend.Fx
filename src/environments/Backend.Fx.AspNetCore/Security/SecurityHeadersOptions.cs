namespace Backend.Fx.AspNetCore.Security
{
    public class SecurityHeadersOptions
    {
        public int HstsExpiration { get; set; }
        public ContentSecurityPolicyOptions ContentSecurityPolicy { get; set; }
    }
}