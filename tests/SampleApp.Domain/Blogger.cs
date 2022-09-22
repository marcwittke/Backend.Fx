using JetBrains.Annotations;

namespace SampleApp.Domain
{
    public class Blogger : AggregateRoot
    {
        [UsedImplicitly]
        private Blogger()
        {
        }

        public Blogger(int id, string lastName, string firstName) : base(id)
        {
            LastName = lastName;
            FirstName = firstName;
        }

        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Bio { get; set; }
    }
}