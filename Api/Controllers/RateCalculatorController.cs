using Microsoft.AspNetCore.Mvc;
using Application.UseCases;
using Newtonsoft.Json;

namespace Api.Controllers;

public class RateCalculatorController : ControllerBase
{
    [HttpGet("{value}/{flag}")]
    public IActionResult GetCreditCardRate(
        decimal value, string flag)
    {
        var creditCardRateCalculator = CreditCardUseCase.CreditCardRateCalculator(value, flag);
        if (creditCardRateCalculator == null)
            return BadRequest("Erro! Verifique os dados enviados.");
        return Ok(JsonConvert.SerializeObject(creditCardRateCalculator)); //ajustar serializacao corretamente depois
    }
}