using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Models;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public CartController(AdventureWorksLt2019Context context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetAll()
        {
            List<Cart> result = new();
            if (this._context != null)
            {
                result = await _context.Carts
                    .FromSqlRaw("SELECT * from carts")
                    .ToListAsync();
                return result;
            }
            else
                return BadRequest();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Cart>>> Getcart(int id)
        {
            List<Cart>? product = null;
            if (this._context != null)
            {
                IQueryable<Cart> query = from cart in this._context.Carts
                                         join customer in this._context.Customers
                                         on cart.CustomerId equals customer.CustomerId
                                         where customer.FkCustomerId == id
                                         select cart;
                product = await query.ToListAsync();
                return product;
            }
            else
                return BadRequest();
        }

        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart(PostCartRequest cart)
        {

            SqlParameter customerIdParameter = new("@customerId", cart.CustomerId);
            SqlParameter productIdParameter = new("@productId", cart.ProductId);
            SqlParameter quantityParameter = new("@quantity", cart.Quantity);

            _context.Database.ExecuteSqlRaw($"\r\ndeclare @id int\r\nselect distinct @id = customer_id from [dbo].[carts] as cart\r\njoin [SalesLT].[Customer] as customer on cart.customer_id = customer.CustomerID\r\nwhere FK_Customer_id = {customerIdParameter.ParameterName}\r\n\r\nselect @id\r\n\r\nINSERT INTO [dbo].[carts]\r\n           ([customer_id]\r\n           ,[product_id]\r\n           ,[quantity])\r\n     VALUES\r\n           (@id\r\n           ,{productIdParameter.ParameterName},\r\n           {quantityParameter.ParameterName})", customerIdParameter, productIdParameter, quantityParameter);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FirstOrDefaultAsync(e => e.Id == id);
            if (cart == null)
                return NotFound();

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
