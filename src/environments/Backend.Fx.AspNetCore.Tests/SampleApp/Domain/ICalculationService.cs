using System;
using System.Security.Principal;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.AspNetCore.Tests.SampleApp.Domain
{
    public interface ICalculationService
    {
        CalculationResult Add(double arg1, double arg2);
        CalculationResult Subtract(double arg1, double arg2);
        CalculationResult Multiply(double arg1, double arg2);
        CalculationResult Divide(double arg1, double arg2);


        public class CalculationResult
        {
            public CalculationResult(DateTime timestamp, string executor, double result, int tenantId)
            {
                Timestamp = timestamp;
                Executor = executor;
                Result = result;
                TenantId = tenantId;
            }

            public DateTime Timestamp { get; }

            public string Executor { get; }

            public double Result { get; }

            public int TenantId { get; }
        }
    }


    public class CalculationService : ICalculationService, IDomainService
    {
        private readonly IClock _clock;
        private readonly ICurrentTHolder<IIdentity> _identityHolder;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        public CalculationService(
            IClock clock,
            ICurrentTHolder<IIdentity> identityHolder,
            ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _clock = clock;
            _identityHolder = identityHolder;
            _tenantIdHolder = tenantIdHolder;
        }

        public ICalculationService.CalculationResult Add(double arg1, double arg2)
        {
            return new ICalculationService.CalculationResult(
                _clock.UtcNow,
                _identityHolder.Current.Name,
                arg1 + arg2,
                _tenantIdHolder.Current.Value);
        }

        public ICalculationService.CalculationResult Subtract(double arg1, double arg2)
        {
            return new ICalculationService.CalculationResult(
                _clock.UtcNow,
                _identityHolder.Current.Name,
                arg1 - arg2,
                _tenantIdHolder.Current.Value);
        }

        public ICalculationService.CalculationResult Multiply(double arg1, double arg2)
        {
            return new ICalculationService.CalculationResult(
                _clock.UtcNow,
                _identityHolder.Current.Name,
                arg1 * arg2,
                _tenantIdHolder.Current.Value);
        }

        public ICalculationService.CalculationResult Divide(double arg1, double arg2)
        {
            if (arg2 == 0d)
            {
                throw new UnprocessableException("Attempt to divide by zero")
                    .AddError("arg2", "Division by zero is not possible.");
            }


            return new ICalculationService.CalculationResult(
                _clock.UtcNow,
                _identityHolder.Current.Name,
                arg1 / arg2,
                _tenantIdHolder.Current.Value);
        }
    }
}
