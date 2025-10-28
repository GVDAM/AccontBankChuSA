using AccountsChu.Domain.Commands.Account;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;

namespace AccountsChu.API.Controllers.v1
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _sender;

        public AccountController(IMediator sender)
        {
            _sender = sender;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateAccountCommand account)
        {
            var result = await _sender.Send(account);

            if (result.IsSuccess)
                return StatusCode(201, result.Data);
            else
                return BadRequest(result);
        }

        [HttpPost("ted")]
        [Description("Somente opção de TED disponível para trasnferência")]
        public async Task<IActionResult> Desactivate([FromBody] TedTransactionAccountCommand transaction)
        {
            var result = await _sender.Send(transaction);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Extrato da conta do usuário logado
        /// </summary>
        /// <returns></returns>
        [HttpGet("extrato")]
        public async Task<IActionResult> GetStatementAccount()
        {
            var result = await _sender.Send(new StatementAccountCommand());

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
