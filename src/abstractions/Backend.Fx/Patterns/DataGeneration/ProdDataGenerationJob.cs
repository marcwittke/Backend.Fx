using System.Collections.Generic;
using Backend.Fx.Patterns.Jobs;
using Backend.Fx.Patterns.UnitOfWork;

namespace Backend.Fx.Patterns.DataGeneration
{
    public class ProdDataGenerationJob : IJob
    {
        private readonly IEnumerable<DataGenerator> _dataGenerators;
        private readonly ICanFlush _canFlush;

        public ProdDataGenerationJob(IEnumerable<DataGenerator> dataGenerators, ICanFlush canFlush)
        {
            _dataGenerators = dataGenerators;
            _canFlush = canFlush;
        }

        public void Run()
        {
            var dataGeneratorContext = new DataGeneratorContext(_dataGenerators, _canFlush);
            dataGeneratorContext.RunProductiveDataGenerators();
        }
    }
}