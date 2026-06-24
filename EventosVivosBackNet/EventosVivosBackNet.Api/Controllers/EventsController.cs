using EventosVivosBackNet.Application.Features.Events.Commands.CreateEvent;
using EventosVivosBackNet.Application.Features.Events.Queries.GetEventsFiltered;
using EventosVivosBackNet.Application.Features.Events.Queries.GetOccupancyReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivosBackNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.EsExitoso)
                return BadRequest(result);
            return Created(string.Empty, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? eventType,
            [FromQuery] DateTime? startDateFrom,
            [FromQuery] DateTime? startDateTo,
            [FromQuery] long? venueId,
            [FromQuery] string? status,
            [FromQuery] string? titleSearch)
        {
            var query = new GetEventsFilteredQuery
            {
                EventType = eventType,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                VenueId = venueId,
                Status = status,
                TitleSearch = titleSearch
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}/occupancy")]
        public async Task<IActionResult> GetOccupancyReport(long id)
        {
            var query = new GetOccupancyReportQuery { EventId = id };
            var result = await _mediator.Send(query);
            if (!result.EsExitoso)
                return NotFound(result);
            return Ok(result);
        }
    }
}
