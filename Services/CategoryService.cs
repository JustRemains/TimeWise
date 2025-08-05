using Microsoft.EntityFrameworkCore;
using TimeWise.Data;
using TimeWise.Models;

namespace TimeWise.Services
{
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly TimeWiseDbContext _context;
        private readonly Random _random = new();

        public CategoryService(TimeWiseDbContext context)
        {
            _context = context;
        }

        #region ??CRUD??

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.IsDefault ? 0 : 1)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            // ???????
            if (await GetCategoryByNameAsync(category.Name) != null)
                throw new InvalidOperationException($"Category '{category.Name}' already exists.");

            // ?????????????????
            if (string.IsNullOrEmpty(category.ColorHex) || category.ColorHex == "#CCCCCC")
                category.ColorHex = await GenerateUniqueColorAsync();

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            // ???????????????
            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower() && c.Id != category.Id);
            
            if (existing != null)
                throw new InvalidOperationException($"Category '{category.Name}' already exists.");

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !await CanDeleteCategoryAsync(id))
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region ??????

        public async Task<IEnumerable<Category>> GetDefaultCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsDefault)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CanDeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            // ????????
            if (category.IsDefault)
                return false;

            // ????????????
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.CategoryId == id);

            return !hasAppointments;
        }

        #endregion

        #region ????

        public async Task<string> GenerateUniqueColorAsync()
        {
            var usedColors = await _context.Categories
                .Select(c => c.ColorHex)
                .ToListAsync();

            string newColor;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                newColor = GenerateRandomLightColor();
                attempts++;
                
                if (attempts >= maxAttempts)
                {
                    // ??????????????????
                    var timestamp = DateTime.Now.Ticks % 1000000;
                    var r = (byte)(180 + (timestamp % 75));
                    var g = (byte)(180 + ((timestamp / 100) % 75));
                    var b = (byte)(180 + ((timestamp / 10000) % 75));
                    newColor = $"#{r:X2}{g:X2}{b:X2}";
                    break;
                }
            }
            while (usedColors.Contains(newColor));

            return newColor;
        }

        public async Task<bool> IsColorUsedAsync(string colorHex)
        {
            return await _context.Categories
                .AnyAsync(c => c.ColorHex == colorHex);
        }

        private string GenerateRandomLightColor()
        {
            // ?????RGB??180-255???
            var r = (byte)_random.Next(180, 256);
            var g = (byte)_random.Next(180, 256);
            var b = (byte)_random.Next(180, 256);
            
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        #endregion
    }
}