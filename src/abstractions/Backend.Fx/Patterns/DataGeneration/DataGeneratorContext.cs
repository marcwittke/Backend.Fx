namespace Backend.Fx.Patterns.DataGeneration
{
    using System.Collections.Generic;
    using System.Linq;
    using Logging;
    using Patterns.UnitOfWork;

    public class DataGeneratorContext
    {
        private static readonly ILogger Logger = LogManager.Create<DataGeneratorContext>();
        private readonly IEnumerable<IDataGenerator> _dataGenerators;
        private readonly ICanFlush _canFlush;

        public DataGeneratorContext(IEnumerable<IDataGenerator> dataGenerators, ICanFlush canFlush)
        {
            _dataGenerators = dataGenerators;
            _canFlush = canFlush;
        }

        public void RunProductiveDataGenerators()
        {
            Logger.Info("Loading productive data into database");
            RunDataGenerators<IProductiveDataGenerator>();
        }

        public void RunDemoDataGenerators()
        {
            Logger.Info("Loading demonstration data into database");
            RunDataGenerators<IDemoDataGenerator>();
        }

        private void RunDataGenerators<TDataGenerator>()
        {
            foreach (var dataGenerator in _dataGenerators.Where(dg => dg is TDataGenerator).OrderBy(dg => dg.Priority))
            {
                dataGenerator.Generate();
                _canFlush.Flush();
            }
        }
    }
}
