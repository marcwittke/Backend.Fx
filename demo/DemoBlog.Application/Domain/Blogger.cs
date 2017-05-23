namespace DemoBlog.Domain
{
    using Backend.Fx.BuildingBlocks;
    using JetBrains.Annotations;

    public class Blogger : AggregateRoot
    {
        [UsedImplicitly]
        private Blogger()
        {}

        public Blogger(string lastName, string firstName)
        {
            LastName = lastName;
            FirstName = firstName;
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Bio { get; set; }
    }
}