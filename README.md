# Backend.Fx 
## An opinionated backend architecture based on .NET Standard
I am using this set of class libraries in three projects now. Obeying the Rule of Three I am generalizing most of it now. You should be able to reference these libraries from .net4.6, .net core 1.0, Xamarin and UWP. The more abstract the library the lower the required .NET Standard gets.

Abstraction | .Net Standard | NuGet
--- | --- | ---
Backend.Fx | 1.3 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.svg)](https://www.nuget.org/packages/Backend.Fx)

## Integration into your target environment made easy

Environment | .Net Standard | NuGet
--- | --- | ---
Backend.Fx.AspNetCore | 2.0 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.AspNetCore.svg)](https://www.nuget.org/packages/Backend.Fx.AspNetCore)
Backend.Fx.AspNetCore.Mvc | 2.0 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.AspNetCore.Mvc.svg)](https://www.nuget.org/packages/Backend.Fx.AspNetCore.Mvc)
Backend.Fx.NetCore | 1.3 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.NetCore.svg)](https://www.nuget.org/packages/Backend.Fx.NetCore)
 
## What does "opinionated" mean?
You get vendor locked to a set of abstractions, like my DDD building blocks and some architecture [patterns](https://github.com/marcwittke/Backend.Fx/tree/master/src/abstractions/Backend.Fx/Patterns) defined as interfaces

## Less opinonated, but already provided as pluggable implementations of patterns

Vendor | Library | .NET Standard | NuGet
--- | --- | --- | ---
[Entity Framework Core 2.1](https://github.com/aspnet/EntityFramework) as persistence mechanism | Backend.Fx.EfCorePersistence | 2.0 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.EfCorePersistence.svg)](https://www.nuget.org/packages/Backend.Fx.EfCorePersistence)
InMemory Persistence implementation  | Backend.Fx.InMemoryPersistence | 1.3 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.InMemoryPersistence.svg)](https://www.nuget.org/packages/Backend.Fx.InMemoryPersistence)
[NLog](https://github.com/NLog/NLog) logging | Backend.Fx.NLogLogging | 1.6 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.NLogLogging.svg)](https://www.nuget.org/packages/Backend.Fx.NLogLogging)
[RabbitMq](https://www.rabbitmq.com/) Event Bus | Backend.Fx.RabbitMq | 1.5 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.RabbitMq.svg)](https://www.nuget.org/packages/Backend.Fx.RabbitMq)
[Simple Injector](https://github.com/simpleinjector/SimpleInjector) as DI container | Backend.Fx.SimpleInjectorDependencyInjection | 1.3 | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.SimpleInjectorDependencyInjection.svg)](https://www.nuget.org/packages/Backend.Fx.SimpleInjectorDependencyInjection)
