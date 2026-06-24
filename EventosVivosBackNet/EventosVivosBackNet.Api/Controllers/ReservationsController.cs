using EventosVivosBackNet.Application.Features.Reservations.Commands.CancelReservation;
using EventosVivosBackNet.Application.Features.Reservations.Commands.ConfirmPayment;
using EventosVivosBackNet.Application.Features.Reservations.Commands.CreateReservation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivosBackNet.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.EsExitoso)
                return BadRequest(result);
            return Created(string.Empty, result);
        }

        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> ConfirmPayment(long id)
        {
            var command = new ConfirmPaymentCommand { ReservationId = id };
            var result = await _mediator.Send(command);
            if (!result.EsExitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(long id)
        {
            var command = new CancelReservationCommand { ReservationId = id };
            var result = await _mediator.Send(command);
            if (!result.EsExitoso)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
