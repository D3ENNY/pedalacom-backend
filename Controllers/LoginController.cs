using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebAppPedalaCom.Blogic.Service;
using WebAppPedalaCom.Models;
using WebAppTestEmployees.Blogic.Authentication;

namespace WebAppPedalaCom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AdventureWorksLt2019Context _AWcontext;
        private readonly ErrorLogService _errorLogService;
        private readonly CredentialWorks2024Context _CWcontext;
        public LoginController(AdventureWorksLt2019Context context)
        {
            this._AWcontext = context;
            this._CWcontext = new();
            this._errorLogService = new(_CWcontext);
        }

        [BasicAutorizationAttributes]
        [HttpPost]
        public IActionResult Auth(User user)
        {
            Customer? userExistOldDB = _AWcontext.Customers.FromSqlRaw($"select * from [SalesLT].[Customer] where EmailAddress = @email", new SqlParameter("@email", user.EmailAddress)).FirstOrDefault();

            if (userExistOldDB != null)
                return Conflict("user already exist");

            CwCustomer? fullUser = _CWcontext.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", user.EmailAddress)).SingleOrDefault();

            if (fullUser != null)
            {
                string? passwordHashed = PasswordHash(user, fullUser.PasswordSalt ?? "");

                if (passwordHashed == null)
                    return StatusCode(500, "Internal Server Error\npassword hashed is Null");

                CwCustomer? utente = _CWcontext.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email and PasswordHash = @password", new SqlParameter("@email", user.EmailAddress), new SqlParameter("@password", passwordHashed)).SingleOrDefault();

                if (utente != null)
                    return Ok(new
                    {
                        utente.FirstName,
                        utente.CustomerId
                    });
                return BadRequest("wrong password");
            }
            return NotFound("user not found");
        }

        private string? PasswordHash(User user, string sale)
        {
            try
            {
                byte[] EncResult =
                    KeyDerivation.Pbkdf2(
                        password: user.PasswordHash,
                        salt: Convert.FromBase64String(sale),
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 10000,
                        numBytesRequested: 132
                    );

                return Convert.ToBase64String(EncResult);
            }
            catch(ArgumentException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch(Exception ex)
            {
                _errorLogService.LogError(ex);
            }
            return null;
        }
    }

    public class User
    {
        public string? EmailAddress { get; set; }

        public string? PasswordHash { get; set; }
    }
}