using EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos;
using EventosVivosBackNet.Domain.Entities;
using EventosVivosBackNet.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace EventosVivosBackNet.Infrastructure.Repositories.DbEventosVivos
{
    internal class DbEventosVivosRepository : IDbEventosVivosRepository
    {
        private readonly AppDbContext _context;

        public DbEventosVivosRepository(AppDbContext context)
        {
            _context = context;
        }

        // Events

        public async Task<Event?> GetEventByIdAsync(long id)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Event?> GetEventWithVenueByIdAsync(long id)
        {
            return await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetEventsFilteredAsync(
            string? eventType,
            DateTime? startDateFrom,
            DateTime? startDateTo,
            long? venueId,
            string? status,
            string? titleSearch)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (startDateFrom.HasValue)
                query = query.Where(e => e.StartAt >= startDateFrom.Value);

            if (startDateTo.HasValue)
                query = query.Where(e => e.StartAt <= startDateTo.Value);

            if (venueId.HasValue)
                query = query.Where(e => e.VenueId == venueId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            if (!string.IsNullOrWhiteSpace(titleSearch))
                query = query.Where(e => EF.Functions.ILike(e.Title, $"%{titleSearch}%"));

            return await query.OrderByDescending(e => e.StartAt).ToListAsync();
        }

        public async Task<bool> ExistsOverlappingEventAsync(long venueId, DateTime startAt, DateTime endAt, long? excludeEventId = null)
        {
            var query = _context.Events
                .Where(e => e.VenueId == venueId)
                .Where(e => e.Status == "activo")
                .Where(e => e.StartAt < endAt && e.EndAt > startAt);

            if (excludeEventId.HasValue)
                query = query.Where(e => e.Id != excludeEventId.Value);

            return await query.AnyAsync();
        }

        public async Task AddEventAsync(Event entity)
        {
            await _context.Events.AddAsync(entity);
        }

        public void UpdateEvent(Event entity)
        {
            _context.Events.Update(entity);
        }

        // Venues

        public async Task<Venue?> GetVenueByIdAsync(long id)
        {
            return await _context.Venues.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<List<Venue>> GetAllVenuesAsync()
        {
            return await _context.Venues.OrderBy(v => v.Name).ToListAsync();
        }

        // Reservations

        public async Task<Reservation?> GetReservationByIdAsync(long id)
        {
            return await _context.Reservations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<int> GetConfirmedReservationsQuantityByEventAsync(long eventId)
        {
            return await _context.Reservations
                .Where(r => r.EventId == eventId)
                .Where(r => r.Status == "confirmada" || r.Status == "pendiente_pago")
                .SumAsync(r => r.Quantity);
        }

        public async Task<bool> ExistsReservationCodeAsync(string code)
        {
            return await _context.Reservations.AnyAsync(r => r.ReservationCode == code);
        }

        public async Task AddReservationAsync(Reservation entity)
        {
            await _context.Reservations.AddAsync(entity);
        }

        public void UpdateReservation(Reservation entity)
        {
            _context.Reservations.Update(entity);
        }

        // ReservationLosses

        public async Task AddReservationLossAsync(ReservationLoss entity)
        {
            await _context.ReservationLosses.AddAsync(entity);
        }

        public async Task<int> GetLostQuantityByEventAsync(long eventId)
        {
            return await _context.ReservationLosses
                .Where(rl => rl.EventId == eventId)
                .SumAsync(rl => rl.Quantity);
        }
    }
}
