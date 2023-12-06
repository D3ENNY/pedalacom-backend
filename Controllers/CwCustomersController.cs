using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppTestEmployees.Blogic.Authentication;
using WebAppPedalaCom.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace WebAppPedalaCom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CwCustomersController : ControllerBase
    {
        private readonly CredentialWorks2024Context _context;

        public CwCustomersController(CredentialWorks2024Context context)
        {
            _context = context;
        }

        // GET: api/CwCustomers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CwCustomer>>> GetCwCustomers()
        {
            if (_context.CwCustomers == null)
            {
                return NotFound();
            }
            return await _context.CwCustomers.ToListAsync();
        }

        // GET: api/CwCustomers/5
        [HttpGet("{email}")]
        public async Task<ActionResult<CwCustomer>> GetCwCustomer(string email)
        {
            if (_context.CwCustomers == null)
            {
                return NotFound();
            }
            var cwCustomer = await _context.CwCustomers.Where(e => e.EmailAddress == email).FirstOrDefaultAsync();

            if (cwCustomer == null)
            {
                return NotFound();
            }

            return cwCustomer;
        }

        // PUT: api/CwCustomers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCwCustomer(int id, CwCustomer cwCustomer)
        {
            if (id != cwCustomer.CustomerId)
            {
                return BadRequest();
            }

            _context.Entry(cwCustomer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CwCustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CwCustomers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CwCustomer>> PostCwCustomer(CwCustomer cwCustomer)
        {

            if (_context.CwCustomers == null)
            {
                return Problem("Entity set 'CredentialWorks2024Context.CwCustomers'  is null.");
            }
            KeyValuePair<string, string> hashpass;
            using (var _authorizationDB = new AdventureWorksLt2019Context())
            {
                var userExistOldDB = _authorizationDB.Customers.FromSqlRaw($"select * from [SalesLT].[Customer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

                if (userExistOldDB != null)
                {
                    if (userExistOldDB.FkCustomerId != null)
                    {
                        //Nel caso in cui è già registrato anche nel nuovo database
                        return Problem("Esisti già pirla");
                    }
                    else
                    {
                        //Nel caso in cui non fosse già registrato nel nuovo databasio

                        hashpass = EncryptSaltString(cwCustomer.PasswordHash);

                        cwCustomer.PasswordHash = hashpass.Value;

                        cwCustomer.PasswordSalt = hashpass.Key;

                        _context.CwCustomers.Add(cwCustomer);

                        await _context.SaveChangesAsync();

                        var newUser = _context.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

                        var emailParameter = new SqlParameter("@email", cwCustomer.EmailAddress);

                        var idParameter = new SqlParameter("@id", newUser.CustomerId);

                        _authorizationDB.Database.ExecuteSqlRaw($"UPDATE [SalesLT].[Customer] SET [NameStyle] = ''  " +
                           $",[Title] = null ,[FirstName] = ''  ,[MiddleName] = null ,[LastName] = '' ,[Suffix] = null ,[CompanyName] = null ,[SalesPerson] = null " +
                           $",[EmailAddress] = null ,[Phone] = null  ,[PasswordHash] = '' ,[PasswordSalt] = '' ,[ModifiedDate] = GETDATE() " +
                           $",[FK_Customer_id] = {idParameter.ParameterName} WHERE EmailAddress = {emailParameter.ParameterName}",
                           emailParameter, idParameter);


                        return CreatedAtAction("GetCwCustomer", new { email = cwCustomer.EmailAddress }, cwCustomer);
                    }
                }
            }

            var userExistNewDB = _context.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

            if (userExistNewDB != null)
            {
                return Problem("Esisti già pirla");
            }
            else
            {
                hashpass = EncryptSaltString(cwCustomer.PasswordHash);

                cwCustomer.PasswordHash = hashpass.Value;

                cwCustomer.PasswordSalt = hashpass.Key;

                _context.CwCustomers.Add(cwCustomer);

                await _context.SaveChangesAsync();

                var newUser = _context.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", cwCustomer.EmailAddress)).FirstOrDefault();

                using (var _authorizationDB = new AdventureWorksLt2019Context())
                {
                    var idParameter = new SqlParameter("@id", newUser.CustomerId);

                    _authorizationDB.Database.ExecuteSqlRaw($"INSERT INTO [SalesLT].[Customer] ([NameStyle], [FirstName], [LastName], [PasswordHash], [PasswordSalt], [ModifiedDate], [FK_Customer_id]) VALUES('', '', '', '', '', GETDATE(), {idParameter.ParameterName})",idParameter);
                }

                return CreatedAtAction("GetCwCustomer", new { email = cwCustomer.EmailAddress }, cwCustomer);
            }

        }


        // DELETE: api/CwCustomers/5
        [HttpDelete("{email}")]
        public async Task<IActionResult> DeleteCwCustomer(string email)
        {
            if (_context.CwCustomers == null)
            {
                return NotFound();
            }
            var cwCustomer = await _context.CwCustomers.Where(e => e.EmailAddress == email).FirstOrDefaultAsync();
            if (cwCustomer == null)
            {
                return NotFound();
            }

            _context.CwCustomers.Remove(cwCustomer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CwCustomerExists(int id)
        {
            return (_context.CwCustomers?.Any(e => e.CustomerId == id)).GetValueOrDefault();
        }
        private KeyValuePair<string, string> EncryptSaltString(string pwdNeedToHash)
        {
            byte[] byteSalt = new byte[16];
            string EncResult = string.Empty;
            string EncSalt = string.Empty;
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
            catch (System.Exception)
            {
                throw;
            }

            return new KeyValuePair<string, string>(EncSalt, EncResult);
        }

        private void insertNewUser(User user)
        {
            //da fare quando denny si sveglia
        }

    }
}
