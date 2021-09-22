using Backend.Fx.AspNetCore.Tests.SampleApp.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Fx.AspNetCore.Tests.SampleApp.Controllers
{
    [Route("api/[controller]")]
    public class CalculationsController : Controller
    {
        private readonly ICalculationService _calculationService;

        public CalculationsController(ICalculationService calculationService)
        {
            _calculationService = calculationService;
        }

        [HttpPost("addition/{arg1}/{arg2}")]
        public IActionResult Addition(double arg1, double arg2)
        {
            return Ok(_calculationService.Add(arg1, arg2));
        }

        [HttpPost("subtraction/{arg1}/{arg2}")]
        public IActionResult Subtraction(double arg1, double arg2)
        {
            return Ok(_calculationService.Subtract(arg1, arg2));
        }

        [HttpPost("multiplication/{arg1}/{arg2}")]
        public IActionResult Multiplication(double arg1, double arg2)
        {
            return Ok(_calculationService.Multiply(arg1, arg2));
        }

        [HttpPost("division/{arg1}/{arg2}")]
        public IActionResult Division(double arg1, double arg2)
        {
            return Ok(_calculationService.Divide(arg1, arg2));
        }
    }
}
