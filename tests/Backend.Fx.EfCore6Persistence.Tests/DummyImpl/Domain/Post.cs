using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;

namespace Backend.Fx.EfCore6Persistence.Tests.DummyImpl.Domain
{
    public class Post : Entity
    {
        [UsedImplicitly]
        private Post()
        {
        }

        public Post(int id, Blog blog, string name, bool isPublic = false) : base(id)
        {
            Blog = blog;
            BlogId = blog.Id;
            Name = name;
            TargetAudience = new TargetAudience {IsPublic = isPublic, Culture = "fr-FR"};
        }

        [UsedImplicitly] public int BlogId { get; private set; }

        [UsedImplicitly] public Blog Blog { get; private set; }

        [UsedImplicitly] public string Name { get; private set; }

        [UsedImplicitly] public TargetAudience TargetAudience { get; set; }

        public void SetName(string name)
        {
            Name = name;
        }
    }

    public class TargetAudience : ValueObject
    {
        public string Culture { get; set; }

        public bool IsPublic { get; set; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Culture;
            yield return IsPublic;
        }
    }
}