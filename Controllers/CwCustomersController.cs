using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CwCustomersController : ControllerBase
    {
        private readonly CredentialWorks2024Context _CWcontext;

        public CwCustomersController(CredentialWorks2024Context _context) => this._CWcontext = _context;

        // GET: api/CwCustomers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CwCustomer>>> GetCwCustomers()
        {
            if (_CWcontext.CwCustomers == null)
                return NotFound();

            return await _CWcontext.CwCustomers.ToListAsync();
        }

        // GET: api/CwCustomers/5
        [HttpGet("{email}")]
        public async Task<ActionResult<CwCustomer>> GetCwCustomer(string email)
        {
            if (_CWcontext.CwCustomers == null)
                return NotFound();

            var cwCustomer = await _CWcontext.CwCustomers.FirstOrDefaultAsync(e => e.EmailAddress == email);

            if (cwCustomer == null)
                return NotFound();

            return cwCustomer;
        }

        // PUT: api/CwCustomers/5
        [HttpPut("{email}")]
        public async Task<IActionResult> PutCwCustomer(string email, CwCustomer cwCustomer)
        {
            if (email != cwCustomer.EmailAddress)
                return BadRequest();

            var existingCustomer = await _CWcontext.CwCustomers.FirstOrDefaultAsync(e => e.EmailAddress == email);

            existingCustomer.UpdateCustomer(cwCustomer);

            if (existingCustomer == null)
                return NotFound();

            _CWcontext.Entry(existingCustomer).State = EntityState.Modified;

            try
            {
                await _CWcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CwCustomerExists(email))
                    return NotFound();
                else throw;
            }

            return NoContent();
        }

        // POST: api/CwCustomers
        [HttpPost]
        public async Task<ActionResult<CwCustomer>> PostCwCustomer(CwCustomer cwCustomer)
        {

            if (_CWcontext.CwCustomers == null)
                return Problem("Entity set 'CredentialWorks2024Context.CwCustomers'  is null.");

            using AdventureWorksLt2019Context _AWcontext = new();
            Customer userExistOldDB = _AWcontext.Customers.FromSqlRaw($"select * from [SalesLT].[Customer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

            if (userExistOldDB != null)
            {
                if (userExistOldDB.FkCustomerId != null)
                    //Nel caso in cui è già registrato anche nel nuovo database
                    return Problem("Esisti già pirla");
                else
                {
                    //Nel caso in cui non fosse già registrato nel nuovo databasio
                    UpdatePwdCustomers(cwCustomer);
                    await _CWcontext.SaveChangesAsync();

                    CwCustomer newUser = _CWcontext.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

                    SqlParameter emailParameter = new("@email", cwCustomer.EmailAddress);

                    SqlParameter idParameter = new("@id", newUser.CustomerId);

                    _AWcontext.Database.ExecuteSqlRaw($"UPDATE [SalesLT].[Customer] SET [NameStyle] = ''  " +
                       $",[Title] = null ,[FirstName] = ''  ,[MiddleName] = null ,[LastName] = '' ,[Suffix] = null ,[CompanyName] = null ,[SalesPerson] = null " +
                       $",[EmailAddress] = null ,[Phone] = null  ,[PasswordHash] = '' ,[PasswordSalt] = '' ,[ModifiedDate] = GETDATE() " +
                       $",[FK_Customer_id] = {idParameter.ParameterName} WHERE EmailAddress = {emailParameter.ParameterName}",
                       emailParameter, idParameter);


                    return CreatedAtAction("GetCwCustomer", new { email = cwCustomer.EmailAddress }, cwCustomer);
                }
            }

            CwCustomer userExistNewDB = _CWcontext.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

            if (userExistNewDB != null)
                return Conflict("Esisti già pirla");
            else
            {
                cwCustomer.ModifiedDate = DateTime.Now;

                UpdatePwdCustomers(cwCustomer);

                await _CWcontext.SaveChangesAsync();

                CwCustomer newUser = _CWcontext.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

                SqlParameter idParameter = new("@id", newUser.CustomerId);

                _AWcontext.Database.ExecuteSqlRaw($"INSERT INTO [SalesLT].[Customer] ([NameStyle], [FirstName], [LastName], [PasswordHash], [PasswordSalt], [ModifiedDate], [FK_Customer_id]) VALUES('', '', '', '', '', GETDATE(), {idParameter.ParameterName})", idParameter);

                return CreatedAtAction("GetCwCustomer", new { email = cwCustomer.EmailAddress }, cwCustomer);
            }
        }

        // DELETE: api/CwCustomers/5
        [HttpDelete("{email}")]
        public async Task<IActionResult> DeleteCwCustomer(string email)
        {
            if (_CWcontext.CwCustomers == null)
                return NotFound();
  
            CwCustomer cwCustomer = await _CWcontext.CwCustomers.FirstOrDefaultAsync(e => e.EmailAddress == email);

            if (cwCustomer == null)
                return NotFound();

            _CWcontext.CwCustomers.Remove(cwCustomer);
            await _CWcontext.SaveChangesAsync();

            return NoContent();
        }

        private bool CwCustomerExists(string email) => (_CWcontext.CwCustomers?.Any(e => e.EmailAddress == email)).GetValueOrDefault();

        private KeyValuePair<string, string> EncryptSaltString(string pwdNeedToHash)
        {
            byte[] byteSalt = new byte[16];
            string EncSalt, EncResult;
 
            try
            {
                RandomNumberGenerator.Fill(byteSalt);
                EncResult = Convert.ToBase64String(
                    KeyDerivation.Pbkdf2(
                        password: pwdNeedToHash,
                        salt: byteSalt,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 10000,
                        numBytesRequested: 132
                    )
                );
                EncSalt = Convert.ToBase64String(byteSalt);
            }
            catch (Exception)
            {
                throw;
            }

            return new KeyValuePair<string, string>(EncSalt, EncResult);
        }

        private void UpdatePwdCustomers(CwCustomer customer)
        {
            KeyValuePair<string, string> hashpass = EncryptSaltString(customer.PasswordHash);

            customer.PasswordHash = hashpass.Value;

            customer.PasswordSalt = hashpass.Key;

            _CWcontext.CwCustomers.Add(customer);
        }

    }
}
