namespace Backend.Fx.Domain
{
    /// <summary>
    /// An object that is not defined by its attributes, but rather by a thread of continuity and its identity.
    /// https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    public abstract class Entity : Identified
    {
        protected Entity()
        {
        }

        protected Entity(int id)
        {
            Id = id;
        }
    }
}