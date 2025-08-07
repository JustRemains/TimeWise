using Microsoft.EntityFrameworkCore;
using TimeWise.Data;
using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// ??????
    /// </summary>
    public class AppointmentService : IAppointmentService
    {
        private readonly TimeWiseDbContext _context;

        public AppointmentService(TimeWiseDbContext context)
        {
            _context = context;
        }

        #region ??CRUD??

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Category)
                .Include(a => a.AppointmentCategories)
                    .ThenInclude(ac => ac.Category)
                .OrderBy(a => a.Date)
                .ToListAsync();
            
            // ??????????????SQLite TimeSpan????
            return appointments.OrderBy(a => a.Date).ThenBy(a => a.StartTime).ToList();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Category)
                .Include(a => a.AppointmentCategories)
                    .ThenInclude(ac => ac.Category)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
        {
            var targetDate = date.Date;
            var appointments = await _context.Appointments
                .Include(a => a.Category)
                .Include(a => a.AppointmentCategories)
                    .ThenInclude(ac => ac.Category)
                .Where(a => a.Date.Date == targetDate)
                .ToListAsync();
            
            // ???????????
            return appointments.OrderBy(a => a.StartTime).ToList();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            
            var appointments = await _context.Appointments
                .Include(a => a.Category)
                .Include(a => a.AppointmentCategories)
                    .ThenInclude(ac => ac.Category)
                .Where(a => a.Date.Date >= start && a.Date.Date <= end)
                .OrderBy(a => a.Date)
                .ToListAsync();
            
            // ??????????????
            return appointments.OrderBy(a => a.Date).ThenBy(a => a.StartTime).ToList();
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByCategoryAsync(int categoryId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Category)
                .Where(a => a.CategoryId == categoryId)
                .OrderBy(a => a.Date)
                .ToListAsync();
            
            // ??????????????
            return appointments.OrderBy(a => a.Date).ThenBy(a => a.StartTime).ToList();
        }

        public async Task<IEnumerable<Appointment>> SearchAppointmentsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAppointmentsAsync();

            var term = searchTerm.ToLower();
            var appointments = await _context.Appointments
                .Include(a => a.Category)
                .Where(a => a.Title.ToLower().Contains(term) ||
                           (a.Description != null && a.Description.ToLower().Contains(term)) ||
                           a.Category.Name.ToLower().Contains(term))
                .OrderBy(a => a.Date)
                .ToListAsync();
            
            // ??????????????
            return appointments.OrderBy(a => a.Date).ThenBy(a => a.StartTime).ToList();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            // ??????
            if (appointment.EndTime <= appointment.StartTime)
                throw new ArgumentException("End time must be after start time.");

            // ????
            if (await HasConflictAsync(appointment.Date, appointment.StartTime, appointment.EndTime))
                throw new InvalidOperationException("The appointment conflicts with an existing appointment.");

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            
            return await GetAppointmentByIdAsync(appointment.Id) ?? appointment;
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)
        {
            // ??????
            if (appointment.EndTime <= appointment.StartTime)
                throw new ArgumentException("End time must be after start time.");

            // ????????????
            if (await HasConflictAsync(appointment.Date, appointment.StartTime, appointment.EndTime, appointment.Id))
                throw new InvalidOperationException("The appointment conflicts with an existing appointment.");

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            
            return await GetAppointmentByIdAsync(appointment.Id) ?? appointment;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region ??????

        public async Task<bool> HasConflictAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var conflicts = await GetConflictingAppointmentsAsync(date, startTime, endTime, excludeId);
            return conflicts.Any();
        }

        public async Task<IEnumerable<Appointment>> GetConflictingAppointmentsAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var targetDate = date.Date;
            var query = _context.Appointments
                .Include(a => a.Category)
                .Where(a => a.Date.Date == targetDate);

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            // ??????????
            var appointments = await query.ToListAsync();
            
            // ????????
            // ????????????????????????
            return appointments.Where(a => startTime < a.EndTime && endTime > a.StartTime).ToList();
        }

        #endregion

        #region ????

        public async Task<int> GetAppointmentCountAsync()
        {
            return await _context.Appointments.CountAsync();
        }

        public async Task<int> GetAppointmentCountByDateAsync(DateTime date)
        {
            var targetDate = date.Date;
            return await _context.Appointments
                .CountAsync(a => a.Date.Date == targetDate);
        }

        public async Task<Dictionary<string, int>> GetAppointmentCountByCategoryAsync()
        {
            return await _context.Appointments
                .Include(a => a.Category)
                .GroupBy(a => a.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        #endregion

        #region 多Category支持

        public async Task<Appointment> CreateAppointmentWithCategoriesAsync(Appointment appointment, List<int> categoryIds)
        {
            // 首先创建appointment
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // 然后添加多category关系
            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds.Distinct())
                {
                    var appointmentCategory = new AppointmentCategory
                    {
                        AppointmentId = appointment.Id,
                        CategoryId = categoryId
                    };
                    _context.AppointmentCategories.Add(appointmentCategory);
                }
                await _context.SaveChangesAsync();
            }

            // 重新加载包含所有关系的appointment
            return await GetAppointmentByIdAsync(appointment.Id) ?? appointment;
        }

        public async Task<Appointment> UpdateAppointmentCategoriesAsync(int appointmentId, List<int> categoryIds)
        {
            // 删除现有的category关系
            var existingCategories = await _context.AppointmentCategories
                .Where(ac => ac.AppointmentId == appointmentId)
                .ToListAsync();
            
            _context.AppointmentCategories.RemoveRange(existingCategories);

            // 添加新的category关系
            if (categoryIds != null && categoryIds.Any())
            {
                foreach (var categoryId in categoryIds.Distinct())
                {
                    var appointmentCategory = new AppointmentCategory
                    {
                        AppointmentId = appointmentId,
                        CategoryId = categoryId
                    };
                    _context.AppointmentCategories.Add(appointmentCategory);
                }
            }

            await _context.SaveChangesAsync();

            // 重新加载包含所有关系的appointment
            return await GetAppointmentByIdAsync(appointmentId) ?? new Appointment();
        }

        public async Task<IEnumerable<Category>> GetAppointmentCategoriesAsync(int appointmentId)
        {
            return await _context.AppointmentCategories
                .Where(ac => ac.AppointmentId == appointmentId)
                .Include(ac => ac.Category)
                .Select(ac => ac.Category)
                .ToListAsync();
        }

        #endregion
    }
}