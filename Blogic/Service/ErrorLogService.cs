using WebAppPedalaCom.Models;

namespace WebAppPedalaCom.Blogic.Service
{
    public interface IErrorLogService
    {
        void LogError(Exception ex);
    }

    public class ErrorLogService : IErrorLogService
    {

        private readonly  CredentialWorks2024Context _credentialWorks2024Context;

        public ErrorLogService(CredentialWorks2024Context context) => _credentialWorks2024Context = context;


        public void LogError(Exception ex)
        {
            _credentialWorks2024Context.ErrorLogs.Add(new ErrorLog()
            {
                TimeStamp = DateTime.Now,
                ErrorCode = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            });
            _credentialWorks2024Context.SaveChanges();
        }
    }
}
