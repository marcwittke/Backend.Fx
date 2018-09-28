namespace Backend.Fx.Testing.AssemblyFixtures
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fx.Logging;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class XunitTestCollectionRunnerWithAssemblyFixture : XunitTestCollectionRunner
    {
        private static readonly ILogger Logger = LogManager.Create("Xunit.Runner");
        private readonly Dictionary<Type, object> _assemblyFixtureMappings;
        private readonly IMessageSink _diagnosticMessageSink;

        public XunitTestCollectionRunnerWithAssemblyFixture(Dictionary<Type, object> assemblyFixtureMappings,
                                                            ITestCollection testCollection,
                                                            IEnumerable<IXunitTestCase> testCases,
                                                            IMessageSink diagnosticMessageSink,
                                                            IMessageBus messageBus,
                                                            ITestCaseOrderer testCaseOrderer,
                                                            ExceptionAggregator aggregator,
                                                            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this._assemblyFixtureMappings = assemblyFixtureMappings;
            this._diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override async Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            // Don't want to use .Concat + .ToDictionary because of the possibility of overriding types,
            // so instead we'll just let collection fixtures override assembly fixtures.
            var combinedFixtures = new Dictionary<Type, object>(_assemblyFixtureMappings);
            foreach (var kvp in CollectionFixtureMappings)
            {
                combinedFixtures[kvp.Key] = kvp.Value;
            }

            using (Logger.InfoDuration($"Running Test Class {testClass.Class.Name}"))
            {
                var xunitTestClassRunner = new XunitTestClassRunner(testClass, @class, testCases, _diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, combinedFixtures);
                return await xunitTestClassRunner.RunAsync();
            }
        }
    } }