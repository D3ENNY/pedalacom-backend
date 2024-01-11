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
        public LoginController(AdventureWorksLt2019Context context)
        { 
            this._AWcontext = context;
            CredentialWorks2024Context CWcontext = new();
            this._errorLogService = new(CWcontext);
        }

        [BasicAutorizationAttributes]
        [HttpPost]
        public IActionResult Auth(User user)
        {
            Customer userExistOldDB = _AWcontext.Customers.FromSqlRaw($"select * from [SalesLT].[Customer] where EmailAddress = @email", new SqlParameter("@email", user.EmailAddress)).FirstOrDefault();
            if (userExistOldDB != null)
            {
                return BadRequest("OldUser");
            }


            using (var authorizationDB = new CredentialWorks2024Context())
            {

                var fullUser = authorizationDB.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email", new SqlParameter("@email", user.EmailAddress)).SingleOrDefault();

                if(fullUser != null)
                {
                    string sale = fullUser.PasswordSalt;

                    byte[] saleSaltato = Convert.FromBase64String(sale);

                    byte[] EncResult =
                    KeyDerivation.Pbkdf2(
                        password: user.PasswordHash,
                        salt: saleSaltato,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 10000,
                        numBytesRequested: 132
                    );

                    string PasswordHashed = Convert.ToBase64String(EncResult );

                    var utente = authorizationDB.CwCustomers.FromSqlRaw($"select * from [dbo].[CwCustomer] where EmailAddress = @email and PasswordHash = @password", new SqlParameter("@email", user.EmailAddress), new SqlParameter("@password", PasswordHashed)).SingleOrDefault();

                    if( utente != null)
                    {
                        
                        return Ok(new
                        {
                            utente.FirstName, utente.CustomerId 
                        });
                    }
                    else
                    {
                        return BadRequest("wrongPassword");
                    }
                }
                else
                {
                   return BadRequest("userNotFound");
                }


            }
         }
    }

    public class User
    {
        public string EmailAddress { get; set; }

        public string PasswordHash { get; set; }
    }
}

