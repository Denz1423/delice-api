using delice_api.Entities;
using delice_api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace delice_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;

    public ProductsController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [HttpGet]
    [ResponseCache(Duration = 1800)]
    public async Task<ActionResult<List<Product>>> GetProducts()
    {
        var products = await _productRepository.GetAllAsync();
        return Ok(products);
    }
}
