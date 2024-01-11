using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using WebAppPedalaCom.Blogic.Service;
using WebAppPedalaCom.Models;

namespace WebAppTestEmployees.Blogic.Authentication
{
    public partial class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        [GeneratedRegex("Basic (.*)")]
        private static partial Regex MyRegex();
        private CredentialWorks2024Context _CWcontext;
        private AdventureWorksLt2019Context _AWcontext;
        private ErrorLogService _errorLogService;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) 
        {
            this._CWcontext = new();
            this._AWcontext = new();
            this._errorLogService = new(_CWcontext);
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ClaimsPrincipal? claims = null;
            try
            {
                Response.Headers.Add("WWW-Authenticate", "Basic");

                if (!Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
                    return Task.FromResult(AuthenticateResult.Fail("Autorizzazione mancante: impossibile accedere al servizio"));

                string authorizationHeader = value.ToString();

                if (!MyRegex().IsMatch(authorizationHeader))
                    return Task.FromResult(AuthenticateResult.Fail("Autorizzazione non valida: Impossibile accedere al servizio"));

                string authorizationBase64 = Encoding.UTF8.GetString(Convert.FromBase64String(MyRegex().Replace(authorizationHeader, "$1")));

                string[] authorizationSplit = authorizationBase64.Split(':');

                if (authorizationSplit.Length != 2)
                    return Task.FromResult(AuthenticateResult.Fail("Autorizzazione non valida: Impossibile accedere al servizio"));

                if (!_CWcontext.CwCustomers.Any(c => c.EmailAddress == authorizationSplit[0]) ||
                   !_AWcontext.Customers.Any(c => c.EmailAddress == authorizationSplit[0]))
                    return Task.FromResult(AuthenticateResult.Fail("Autorizzazione non valida: Impossibile accedere al servizio"));

                string username = authorizationSplit[0];

                AuthenticationUser authenticationUser = new(username, "BasicAuthentication", true);

                claims = new(new ClaimsIdentity(authenticationUser));
            }
            catch(ArgumentNullException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch(ArgumentException ex)
            {
                _errorLogService.LogError(ex);
            }
            catch(Exception ex)
            {
                _errorLogService.LogError(ex);
            }

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claims ?? new(), "BasicAuthentication")));
        }
    }
}
