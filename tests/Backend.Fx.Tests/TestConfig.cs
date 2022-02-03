//
// using Backend.Fx.Logging;
// using Backend.Fx.Tests;
// using MarcWittke.Xunit.AssemblyFixture;
// using Serilog;
// using Serilog.Events;
// using Xunit;
//
// [assembly: TestFramework("MarcWittke.Xunit.AssemblyFixture.XunitTestFrameworkWithAssemblyFixture", "MarcWittke.Xunit.AssemblyFixture")]
// [assembly: AssemblyFixture(typeof(TestLoggingFixture))]
//
// namespace Backend.Fx.Tests
// {
//     public class TestLoggingFixture
//     {
//         public TestLoggingFixture()
//         {
//             LoggerConfiguration loggerConfiguration = new LoggerConfiguration().Enrich.FromLogContext();
//             
//                                             
//             
//             LogManager.Init();
//             Log.Logger = new LoggerConfiguration()
//                          // add the xunit test output sink to the serilog logger
//                          // https://github.com/trbenning/serilog-sinks-xunit#serilog-sinks-xunit
//                          .WriteTo.TestOutput(toh)
//                          .CreateLogger();
//         }
//     }
// }