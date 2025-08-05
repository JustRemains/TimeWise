using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// ??????
    /// </summary>
    public interface IAppointmentService
    {
        // ??CRUD??
        Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date);
        Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Appointment>> GetAppointmentsByCategoryAsync(int categoryId);
        Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm);
        Task<Appointment> CreateAppointmentAsync(Appointment appointment);
        Task<Appointment> UpdateAppointmentAsync(Appointment appointment);
        Task<bool> DeleteAppointmentAsync(int id);

        // ??????
        Task<bool> HasConflictAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);
        Task<IEnumerable<Appointment>> GetConflictingAppointmentsAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null);

        // ????
        Task<int> GetAppointmentCountAsync();
        Task<int> GetAppointmentCountByDateAsync(DateTime date);
        Task<Dictionary<string, int>> GetAppointmentCountByCategoryAsync();
    }
}