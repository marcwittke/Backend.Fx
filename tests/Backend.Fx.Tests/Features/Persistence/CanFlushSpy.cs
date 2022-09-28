using Backend.Fx.Features.Persistence;

namespace Backend.Fx.Tests.Features.Persistence;

public interface ICanFlushSpy : ICanFlush
{
}

public class CanFlushSpy : ICanFlush
{
    private readonly ICanFlushSpy _canFlushSpy;
    private readonly ICanFlush _canFlush;

    public CanFlushSpy(ICanFlushSpy canFlushSpy, ICanFlush canFlush)
    {
        _canFlushSpy = canFlushSpy;
        _canFlush = canFlush;
    }

    public void Flush()
    {
        _canFlushSpy.Flush();
        _canFlush.Flush();
    }
}