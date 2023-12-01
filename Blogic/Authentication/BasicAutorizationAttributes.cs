using Microsoft.AspNetCore.Authorization;

namespace WebAppTestEmployees.Blogic.Authentication
{
    public class BasicAutorizationAttributes : AuthorizeAttribute
    {
        public BasicAutorizationAttributes()
        {
            Policy = "BasicAuthentication";

        }
    }
}
