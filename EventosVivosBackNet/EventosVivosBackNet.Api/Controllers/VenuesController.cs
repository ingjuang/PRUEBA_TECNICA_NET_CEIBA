using EventosVivosBackNet.Application.Features.Venues.Queries.GetAllVenues;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivosBackNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VenuesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllVenuesQuery());
            return Ok(result);
        }
    }
}
