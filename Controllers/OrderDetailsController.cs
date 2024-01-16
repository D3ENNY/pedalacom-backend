using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Models;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;

        public OrderDetailsController(AdventureWorksLt2019Context context)
        {
            _context = context;
        }

        [HttpGet]
        [ActionName("GetOrderDetails")]
        public async Task<ActionResult<IEnumerable<OrderDetailsDTOs>>> GetOrderDetails()
        {
            List<OrderDetailsDTOs> result = new List<OrderDetailsDTOs>();

            try
            {
                result = await _context.OrderDetailsDTO
                    .FromSqlRaw("EXECUTE GetProductSalesWithDetails")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }

            return Ok(result);
        }
    }
}
