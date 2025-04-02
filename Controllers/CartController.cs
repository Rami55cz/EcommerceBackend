using Microsoft.AspNetCore.Mvc;
using EcommerceBackend.Models;
using Microsoft.EntityFrameworkCore;
using EcommerceBackend.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<CartItem>>> GetCartItems(int userId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<CartItem>> AddToCart(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
            return Ok(cartItem);
        }

        [HttpPut("{productId}")]
        [Authorize]
        public async Task<ActionResult> UpdateQuantity(int productId, [FromBody] int newQuantity)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!int.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID");
            }
            var item = await _context.CartItems.
                FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);;
            if (item == null || item.UserId != userId)
                return NotFound();

            item.Quantity = newQuantity;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{productId}")]
        [Authorize]
        public async Task<ActionResult> RemoveFromCart(int productId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(!int.TryParse(userIdString, out var userId))
            {
                return BadRequest("Invalid user ID");
            }
            var item = await _context.CartItems.
            FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);
            if (item == null || item.UserId != userId)
                return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}