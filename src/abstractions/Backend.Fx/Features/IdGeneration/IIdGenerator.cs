namespace Backend.Fx.Features.IdGeneration
{
    public interface IIdGenerator<out TId> where TId : struct
    {
        TId NextId();
    }
}