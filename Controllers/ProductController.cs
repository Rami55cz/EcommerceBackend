using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using EcommerceBackend.Data;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        /* private readonly BlobServiceClient _blobServiceClient;

        public ProductController(AppDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        } */

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpPost]
        [Authorize(Roles = "administrator")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id) return BadRequest();
            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "administrator")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /* [HttpPost("upload")]
        public async Task<IActionResult> UploadProduct([FromForm] Product product, IFormFile image)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("product-images");
            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(image.FileName);

            using var stream = image.OpenReadStream();
            await blobClient.UploadAsync(stream, true);

            product.ImageUrl = blobClient.Uri.ToString();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        } */
    }
}
