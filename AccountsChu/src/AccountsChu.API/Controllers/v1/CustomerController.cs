using AccountsChu.Domain.Commands.Customer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using MediatR;

namespace AccountsChu.API.Controllers.v1
{
    [AllowAnonymous]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [Description("Criar um usuário para poder utilizar o sistema")]
        public async Task<IActionResult> Post([FromBody] CreateCustomerCommand customer)
        {
            var result = await _mediator.Send(customer);

            if (result.IsSuccess)
                return StatusCode(201, result);

            return BadRequest(result);
        }
        
        [HttpPost("login")]
        [Description("Fazer Login na plataforma")]
        public async Task<IActionResult> Login([FromBody] LoginCustomerCommand customer)
        {
            var result = await _mediator.Send(customer);

            if (result.IsSuccess)
                return Ok(result);                

            return NotFound(result.Message);
        }
    }
}
