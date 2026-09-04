using delice_api.Entities;

namespace delice_api.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();
}
