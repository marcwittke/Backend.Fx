using System.Reflection;
using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Authorization
{
    public static class AuthorizationFeature
    {
        /// <summary>
        /// The feature "Authorization" obligates you the implementation of an <see cref="IAuthorizationPolicy{TAggregateRoot}"/>
        /// for every <see cref="AggregateRoot"/>. Instances of these policy classes are applied to the repositories, so
        /// that on every read or write operation on it, the policy is automatically enforced. Denied reads won't fail but
        /// just appear invisible, while a denied write throws a <see cref="ForbiddenException"/>.
        /// While implementing policies, you can start by deriving from <see cref="DenyAll{TAggregateRoot}"/> or
        /// <see cref="AllowAll{TAggregateRoot}"/>.
        /// </summary>
        /// <param name="application"></param>
        [PublicAPI]
        public static void EnableAuthorization(this IBackendFxApplication application)
            => application.CompositionRoot.RegisterModules(new AuthorizationModule(application.Assemblies));
    }
}