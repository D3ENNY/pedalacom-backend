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
        public async Task<ActionResult<IEnumerable<OrderDetailsDTOs>>> GetOrderDetails(int pageNumber = 1)
        {
            int pageSize = 9;
            object? paginationInfo = null;
            List<OrderDetailsDTOs> result = new List<OrderDetailsDTOs>();

            try
            {
                // Execute the raw SQL query to retrieve data
                var rawSqlResults = await _context.OrderDetailsDTO
                    .FromSqlRaw("EXECUTE GetProductSalesWithDetails")
                    .ToListAsync();

                // Manually perform client-side pagination using List.GetRange
                var startIndex = (pageNumber - 1) * pageSize;
                var endIndex = Math.Min(startIndex + pageSize, rawSqlResults.Count);

                if (startIndex < endIndex)
                {
                    result = rawSqlResults.GetRange(startIndex, endIndex - startIndex);
                }

                if (result.Count == 0)
                {
                    return Ok();
                }

                // Get the total number of items in the rawSqlResults
                int totalItems = rawSqlResults.Count;

                // Calculate the total number of pages
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Add paginated results and pagination information to the response
                paginationInfo = new
                {
                    pageNumber = pageNumber,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }

            return Ok(new { OrderDetails = result, PaginationInfo = paginationInfo });
        }

        [HttpGet("{id:int}")]
        [ActionName("GetOrderDetailsbyId")]

        public async Task<ActionResult<OrderDetailsDTOs>> GetOrderDetailById(int id)
        {
            OrderDetailsDTOs? orderDetail = new();
            try
            {
                var orderDetailList = await _context.OrderDetailsDTO
                .FromSqlRaw("EXECUTE GetProductSalesWithDetails")
                .ToListAsync();

                orderDetail = orderDetailList.FirstOrDefault(od => od.ProductID == id);


                if (orderDetail == null)
                {
                    return NotFound(); // Ritorna 404 se l'ordine con l'ID specificato non è stato trovato
                }

            }
            catch (DbUpdateException ex)
            {
                // Gestisci le eccezioni specifiche di Entity Framework Core
                return StatusCode(500, "Errore nell'aggiornamento del database");
            }
            catch (Exception ex)
            {
                // Gestire eventuali altre eccezioni
                return StatusCode(500, "Internal Server Error");
            }

            return Ok(orderDetail);

        }


    }
}
