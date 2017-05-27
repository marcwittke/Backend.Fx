namespace DemoBlog.Domain
{
    using System;
    using System.Linq.Expressions;
    using System.Security.Claims;
    using System.Security.Principal;
    using Backend.Fx.Environment.Authentication;
    using Backend.Fx.Patterns.Authorization;

    public class PostAuthorization : IAggregateRootAuthorization<Post>
    {
        private readonly IIdentity identity;

        public PostAuthorization(IIdentity identity)
        {
            this.identity = identity;
        }

        public Expression<Func<Post, bool>> HasAccessExpression
        {
            get { return blogger => true; }
        }

        public bool CanCreate()
        {
            if (identity is SystemIdentity)
            {
                return true;
            }
            //TODO: die Blog Id muss hier irgendwie geprüft werden
            var claimsIdentity = identity as ClaimsIdentity;
            return claimsIdentity != null && claimsIdentity.HasClaim(claim => claim.Type == "urn:demoblog:blogadmin" && claim.Value == "blogId");
        }
    }
}