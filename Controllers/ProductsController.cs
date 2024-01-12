using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Text.Json;
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
        public async Task<ActionResult<Product>> PostProduct(List<object>data)
        {
            string? model = null, description = null;
            Product product = null;

            if (_context.Products == null)
            {
                _errorLogService.LogError(new ArgumentNullException());
                return StatusCode(500, "Internal Server Error\n_context.Product is Null");
            }

            if (data == null || data.Count < 2)
                return StatusCode(500, "Internal Server Error\n dara is Null");

            if (data[0] is JsonElement jsonElement && data[1] is JsonElement array)
            {
                JsonElement first = array.EnumerateArray().FirstOrDefault();
                JsonElement last = array.EnumerateArray().LastOrDefault();

                if (!first.Equals(default(JsonElement)) && !last.Equals(default(JsonElement)))
                {
                    model = first.GetString();
                    description = last.GetString();
                }
                product = new()
                {
                    ProductId = jsonElement.GetProperty("productId").GetInt16(),
                    Color = jsonElement.GetProperty("color").GetString(),
                    ListPrice = jsonElement.GetProperty("listPrice").GetInt16(),
                    ModifiedDate = DateTime.Now,
                    Name = jsonElement.GetProperty("name").GetString() ?? string.Empty,
                    ProductCategoryId = jsonElement.GetProperty("productCategoryId").GetInt16(),
                    ProductNumber = jsonElement.GetProperty("productNumber").GetString() ?? string.Empty,
                    Size = jsonElement.GetProperty("size").GetString(),
                    StandardCost = jsonElement.GetProperty("standardCost").GetInt16(),
                    ThumbNailPhoto = Convert.FromBase64String(jsonElement.GetProperty("thumbnailPhotoFileName").GetString().Split(",")[1]),
                    Weight = jsonElement.GetProperty("weight").GetInt16(),
                    SellStartDate = DateTime.Now,
                    Rowguid = Guid.NewGuid()
                };
            }
            using (IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    ProductModel existingModel = await _context.ProductModels.FirstOrDefaultAsync(x => x.Name == model);
                    if (existingModel == null)
                    {
                        _context.ProductModels.Add(new ProductModel() { Name = model, Rowguid = Guid.NewGuid(), ModifiedDate = DateTime.Now });
                        await _context.SaveChangesAsync();
                    }
                    ProductDescription existingDescription = await _context.ProductDescriptions.FirstOrDefaultAsync(x => x.Description == description);
                    if (existingDescription == null)
                    {
                        _context.ProductDescriptions.Add(new ProductDescription() { Description = description, Rowguid = Guid.NewGuid(), ModifiedDate = DateTime.Now });
                        await _context.SaveChangesAsync();
                    }
                    int productModelId = (existingModel != null) ? existingModel.ProductModelId : _context.ProductModels.Single(x => x.Name == model).ProductModelId;
                    int productDescId = (existingDescription != null) ? existingDescription.ProductDescriptionId : _context.ProductDescriptions.Single(x => x.Description == description).ProductDescriptionId;
                    _context.ProductModelProductDescriptions.Add(new ProductModelProductDescription()
                    {
                        ProductModelId = productModelId,
                        ProductDescriptionId = productDescId,
                        Culture = "????",
                        Rowguid = Guid.NewGuid(),
                        ModifiedDate = DateTime.Now
                    });
                    product.ProductModelId = productModelId;
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _errorLogService.LogError(ex);
                    await Console.Error.WriteLineAsync("error -> " + ex.Message);
                }
            }
            

            return CreatedAtAction("GetProducts", new { id = product.ProductId }, product);
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
