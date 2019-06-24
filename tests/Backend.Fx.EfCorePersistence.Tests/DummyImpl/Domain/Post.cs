﻿using System.Collections.Generic;
using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;

namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Domain
{
    public class Post : Entity
    {
        [UsedImplicitly]
        private Post() { }

        public Post(int id, Blog blog, string name, bool isPublic=false) : base(id)
        {
            Blog = blog;
            BlogId = blog.Id;
            Name = name;
            TargetAudience = new TargetAudience {IsPublic = isPublic, Culture = "fr-FR"};
        }

        public int BlogId { get; [UsedImplicitly] private set; }
        public Blog Blog { get; [UsedImplicitly] private set; }
        public string Name { get; private set; }

        public TargetAudience TargetAudience { get; set; }

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