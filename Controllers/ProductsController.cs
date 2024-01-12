using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Blogic.Service;
using WebAppPedalaCom.Models;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _context;
        private readonly ErrorLogService _errorLogService;

        public ProductsController(AdventureWorksLt2019Context context)
        {
            this._context = context;
            CredentialWorks2024Context CWcontext = new();
            this._errorLogService = new(CWcontext);
        }

        /*      *
         *      *
         *  GET *  
         *      *
         *      */

        // GET: api/Products
        [HttpGet]
        [ActionName("GetProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            List<Product> result = new();
            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            try
            {
                 result = await _context.Products
                    // Execute the stored procedure using raw SQL query
                    .FromSqlRaw("EXECUTE GetTopSellingProductsDetails")
                    .ToListAsync();
            }
            catch (OperationCanceledException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }

            return Ok(result);
        }

        // GET: api/Products/{id}
        [HttpGet("{id:int}")]
        [ActionName("GetProductsByID")]
        public async Task<ActionResult<Product>> GetProductsById(int id)
        {
            Product? product = null;
            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            try
            {
                 product = await _context.Products
                    .Include(prd => prd.ProductModel) // Include Table ProductModel
                    .ThenInclude(mdl => mdl.ProductModelProductDescriptions) // Include Pivot SalesOrderDetails
                    .ThenInclude(prdMD => prdMD.ProductDescription) // Link with Pivot to ProductDescription
                    .Include(prd => prd.SalesOrderDetails) // Include Table SalesOrderDetails
                    .FirstOrDefaultAsync(prd => prd.ProductId == id);
            }
            catch (OperationCanceledException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch(NullReferenceException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }


            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost("info/")]
        [ActionName("GetInfoProductsByCategory")]
        public async Task<ActionResult<IEnumerable<InfoProduct>>> GetInfoProductsByCategory([FromBody] Category[]? category = null, string searchData = "", int pageNumber = 1)
        {
            int pageSize = 6;

            List<InfoProduct>? products = null;
            object? paginationInfo = null;

            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            try
            {
                IQueryable<InfoProduct> query = _context.Products
                    .Include(prd => prd.ProductCategory)
                    .Where(prd => category == null || category.Select(c => c.ToString()).Contains(prd.ProductCategory.Name ?? string.Empty))
                    .Where(prd => EF.Functions.Like(prd.Name, $"%{searchData}%"))
                    .Select(obj => new InfoProduct
                    {
                        productName = obj.Name,
                        productId = obj.ProductId,
                        productPrice = obj.ListPrice,
                        photo = obj.ThumbNailPhoto,
                        productCategory = obj.ProductCategory.Name
                    });

                int totalItems = await query.CountAsync();

                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (pageNumber > totalPages && totalPages == 0)
                {
                    return Ok();
                }

                if (pageNumber > totalPages)
                {
                    return NotFound();
                }

                products = await query.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();
                paginationInfo = new
                {
                    pageNumber = pageNumber,
                    TotalPages = totalPages
                };
            }
            catch (OperationCanceledException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (NullReferenceException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }


            return Ok(new { Products = products, PaginationInfo = paginationInfo });
        }

        [HttpGet("info/")]
        public async Task<ActionResult<IEnumerable<object>>> GetInfoProductsByName(string searchData = "", int pageNumber = 1)
        {
            int pageSize = 12;

            List<object>? products = null;
            object? paginationInfo = null;

            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            try
            {
                IQueryable<object> query = _context.Products
                .Where(prd => EF.Functions.Like(prd.Name, $"%{searchData}%"))
                .Select(obj => new
                {
                    productName = obj.Name,
                    productId = obj.ProductId,
                    productPrice = obj.ListPrice,
                    photo = obj.ThumbNailPhoto,
                    productCategory = obj.ProductCategory.Name,
                    productCode = obj.ProductNumber
                });

                int totalItems = await query.CountAsync();

                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (pageNumber > totalPages && totalPages == 0)
                    return Ok();

                if (pageNumber > totalPages)
                    return NotFound();

                products = await query.Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync();
                paginationInfo = new
                {
                    pageNumber = pageNumber,
                    TotalPages = totalPages
                };
            }
            catch (OperationCanceledException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (NullReferenceException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }

            return Ok(new { Products = products, PaginationInfo = paginationInfo });
        }



        /*      *
         *      *
         *  PUT *  
         *      *
         *      */

        // PUT: api/Products/{id}


        /*
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {

            string[] arr = product.ThumbnailPhotoFileName.Split(",");
            
            if(arr.Length == 2)
            {
                product.ThumbnailPhotoFileName = arr[1];
            }
            else
            {
                product.ThumbnailPhotoFileName = arr[0];
            }
            

            Product newProduct = new()
            {
                ProductId = product.ProductId,
                Color = product.Color,
                ListPrice = product.ListPrice,
                ModifiedDate = DateTime.Now,
                Name = product.Name,
                ProductCategoryId = product.ProductCategoryId,
                ProductNumber = product.ProductNumber,
                Size = product.Size,
                StandardCost = product.StandardCost,
                ThumbNailPhoto = Convert.FromBase64String(product.ThumbnailPhotoFileName),
                Weight = product.Weight,
                SellStartDate = DateTime.Now,
            };

           
            _context.Entry(newProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                    return NotFound();

                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
            }

            return NoContent();
        }
            */

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {

            string[] arr = product.ThumbnailPhotoFileName.Split(",");

            Product newProduct = new()
            {
                ProductId = product.ProductId,
                Color = product.Color,
                ListPrice = product.ListPrice,
                ModifiedDate = DateTime.Now,
                Name = product.Name,
                ProductCategoryId = product.ProductCategoryId,
                ProductNumber = product.ProductNumber,
                Size = product.Size,
                StandardCost = product.StandardCost,
                ThumbNailPhoto = arr.Length == 2 ? Convert.FromBase64String(arr[1]) : Convert.FromBase64String(arr[0]),
                Weight = product.Weight,
                SellStartDate = DateTime.Now,
                Rowguid = Guid.NewGuid()
            };


            if (id != product.ProductId)
                return BadRequest();

            _context.Entry(newProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProductExists(id))
                    return NotFound();

                _errorLogService.LogError(ex);
            }
            catch (Exception ex)
            {
                _errorLogService.LogError(ex);
                await Console.Error.WriteLineAsync(ex.StackTrace);
            }

            return NoContent();
        }

        
        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            Product newProduct = new()
            {
                ProductId = product.ProductId,
                Color = product.Color,
                ListPrice = product.ListPrice,
                ModifiedDate = DateTime.Now, 
                Name = product.Name,
                ProductCategoryId = product.ProductCategoryId,
                ProductNumber = product.ProductNumber,
                Size = product.Size,
                StandardCost = product.StandardCost,
                ThumbNailPhoto = Convert.FromBase64String(product.ThumbnailPhotoFileName.Split(",")[1]),
                Weight = product.Weight,
                SellStartDate = DateTime.Now,
                Rowguid = Guid.NewGuid()
            };

            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            _context.Products.Add(newProduct);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _errorLogService.LogError(ex);
            }

            return CreatedAtAction("GetProducts", new { id = newProduct.ProductId }, newProduct);
        }
        

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            Product product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _errorLogService.LogError(ex);
            }

            return NoContent();
        }

        private bool ProductExists(int id) => (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();

    }
}
