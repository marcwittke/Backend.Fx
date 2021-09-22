using System;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class SimulatedException : Exception
    {
        public SimulatedException() : base(
            "This exception was intentionally thrown by the unit test. If you see this message unexpectedly, probably the exception handling is broken")
        { }
    }
}
