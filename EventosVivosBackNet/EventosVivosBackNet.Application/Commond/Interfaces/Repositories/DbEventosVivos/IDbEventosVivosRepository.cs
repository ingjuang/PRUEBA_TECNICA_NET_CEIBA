using EventosVivosBackNet.Domain.Entities;

namespace EventosVivosBackNet.Application.Commond.Interfaces.Repositories.DbEventosVivos
{
    public interface IDbEventosVivosRepository
    {
        // Events
        Task<Event?> GetEventByIdAsync(long id);
        Task<Event?> GetEventWithVenueByIdAsync(long id);
        Task<List<Event>> GetEventsFilteredAsync(
            string? eventType,
            DateTime? startDateFrom,
            DateTime? startDateTo,
            long? venueId,
            string? status,
            string? titleSearch);
        Task<bool> ExistsOverlappingEventAsync(long venueId, DateTime startAt, DateTime endAt, long? excludeEventId = null);
        Task AddEventAsync(Event entity);
        void UpdateEvent(Event entity);

        // Venues
        Task<Venue?> GetVenueByIdAsync(long id);
        Task<List<Venue>> GetAllVenuesAsync();

        // Reservations
        Task<Reservation?> GetReservationByIdAsync(long id);
        Task<int> GetConfirmedReservationsQuantityByEventAsync(long eventId);
        Task<bool> ExistsReservationCodeAsync(string code);
        Task AddReservationAsync(Reservation entity);
        void UpdateReservation(Reservation entity);

        // ReservationLosses
        Task AddReservationLossAsync(ReservationLoss entity);
        Task<int> GetLostQuantityByEventAsync(long eventId);
    }
}
