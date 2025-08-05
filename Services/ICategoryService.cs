using TimeWise.Models;

namespace TimeWise.Services
{
    /// <summary>
    /// ??????
    /// </summary>
    public interface ICategoryService
    {
        // ??CRUD??
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);

        // ??????
        Task<IEnumerable<Category>> GetDefaultCategoriesAsync();
        Task<bool> CanDeleteCategoryAsync(int id);

        // ????
        Task<string> GenerateUniqueColorAsync();
        Task<bool> IsColorUsedAsync(string colorHex);
    }
}