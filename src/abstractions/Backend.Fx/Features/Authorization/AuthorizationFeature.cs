using Backend.Fx.Domain;
using Backend.Fx.Exceptions;
using Backend.Fx.Features.Persistence;
using JetBrains.Annotations;

namespace Backend.Fx.Features.Authorization
{
    /// <summary>
    /// The feature "Authorization" obligates you the implementation of an <see cref="IAuthorizationPolicy{TAggregateRoot, TId}"/>
    /// for every <see cref="IAggregateRoot{TId}"/>. Instances of these policy classes are applied to the repositories, so
    /// that on every read or write operation on it, the policy is automatically enforced. Denied reads won't fail but
    /// just appear invisible, while a denied write throws a <see cref="ForbiddenException"/>.
    /// While implementing policies, you can start by deriving from <see cref="DenyAll{TAggregateRoot, TId}"/> or
    /// <see cref="AllowAll{TAggregateRoot, TId}"/>.
    /// </summary>
    [PublicAPI]
    public class AuthorizationFeature : Feature
    {
        public override void Enable(IBackendFxApplication application)
        {
            application.RequireDependantFeature<PersistenceFeature>();
            application.CompositionRoot.RegisterModules(new AuthorizationModule(application.Assemblies));
        }
    }
}