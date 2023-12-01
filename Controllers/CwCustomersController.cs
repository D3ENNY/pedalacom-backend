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
            KeyValuePair<string, string> hashpass = EncryptSaltString(cwCustomer.PasswordHash);

            cwCustomer.PasswordHash = hashpass.Value;

            cwCustomer.PasswordSalt = hashpass.Key;

            _context.CwCustomers.Add(cwCustomer);
                await _context.SaveChangesAsync();
        

            return CreatedAtAction("GetCwCustomer", new { email = cwCustomer.EmailAddress }, cwCustomer);
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
    }
}
