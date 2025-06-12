// utad.reFresh.core/Controllers/ProductController.cs

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProductController : ControllerBase
{
    private readonly OFFService _offservice;

    public ProductController(OFFService openFoodFactsService)
    {
        _offservice = openFoodFactsService;
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetProductByBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return BadRequest("Barcode is required.");

        var result = await _offservice.GetProductIngredientsAsync(barcode);
        if (result == null)
            return NotFound("Product not found.");

        return Ok(new
        {
            result.Barcode,
            result.ProductName,
            Ingredients = result.Ingredients.Select(i => new
            {
                i.Id,
                i.Name,
                i.ImageUrl
            })
        });
    }
}