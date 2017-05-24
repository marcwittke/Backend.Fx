# Backend.Fx - An opinionated backend architecture based on .NET Standard 1.3
I am using this class library in three projects now. Obeying the Rule of Three I am generalizing most of it now. Targetting .NET Standard 1.3 allows you to reference these libraries from .net4.6, .net core 1.0, Xamarin and UWP

# What does "opinionated" mean?
You are getting prettymuch vendor locked using this architecture. Although, in my opinion those vendors are well chosen:

- My DDD building blocks and some architecture patterns defined as interfaces
  - This means for example to live with int as id value and some ```Entity``` and ```AggregateRoot``` base classes
- Simple Injector as DI container
- Entity Framework Core as persitence mechanism
- ASP.Net Identity as authentication mechamism

# Less opinonated, but already provided either as pluggable library or demo implementation
- NLog logging
- ASP.Net Core controller frontend
- FluentScheduler as background job scheduler
