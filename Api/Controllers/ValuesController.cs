using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ValuesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<Message>> Get([FromQuery] string value)
        {
            return Ok(await _mediator.Send(new Request(value)));
        }
    }
}