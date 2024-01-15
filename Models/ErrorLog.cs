namespace WebAppPedalaCom.Models;

public class ErrorLog
{
    public int Id { get; set; }

    public DateTime TimeStamp { get; set; }

    public string? ErrorCode { get; set; }

    public string Message { get; set; }

    public string StackTrace { get; set; }
}