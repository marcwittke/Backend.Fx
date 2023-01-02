namespace Backend.Fx.Features.IdGeneration
{
    public interface IIdGenerator<out TId> 
    {
        TId NextId();
    }
}