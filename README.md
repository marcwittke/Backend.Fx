# Backend.Fx 
## An opinionated backend architecture based on .NET Standard 1.3
I am using this set of class libraries in three projects now. Obeying the Rule of Three I am generalizing most of it now. Targetting .NET Standard 1.3 allows you to reference these libraries from .net4.6, .net core 1.0, Xamarin and UWP

Library | NuGet
--- | --- 
Backend.Fx | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.svg)]()
Backend.Fx.Bootstrapping | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.Bootstrapping.svg)]()
Backend.Fx.EfCorePersistence | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.EfCorePersistence.svg)]()
Backend.Fx.NLogLogging | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.NLogLogging.svg)]()
Backend.Fx.Testing | [![NuGet](https://img.shields.io/nuget/v/Backend.Fx.Testing.svg)]()
 

## What does "opinionated" mean?
You are getting pretty much vendor locked using this architecture. Although, in my opinion those vendors are well chosen:

- My DDD building blocks and some architecture [patterns](https://github.com/marcwittke/Backend.Fx/tree/master/src/Backend.Fx/Patterns) defined as interfaces
  - This means for example to live with int as id value and some ```Entity``` and ```AggregateRoot``` base classes
- [Simple Injector](https://github.com/simpleinjector/SimpleInjector) as DI container
- [Entity Framework Core](https://github.com/aspnet/EntityFramework) as persitence mechanism
- [ASP.Net Identity](https://github.com/aspnet/Identity) as authentication mechamism

## Less opinonated, but already provided either as pluggable library or demo implementation
- [NLog](https://github.com/NLog/NLog) logging
- Application Insights diagnostics
- ASP.Net Core controller frontend
- [FluentScheduler](https://github.com/fluentscheduler/FluentScheduler) as background job scheduler
