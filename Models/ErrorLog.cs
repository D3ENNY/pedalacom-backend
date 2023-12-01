using System;
using System.Collections.Generic;

namespace WebAppPedalaCom.Models;

public partial class ErrorLog
{
    public int ErrorLogId { get; set; }

    public DateTime ErrorTime { get; set; }

    public string UserName { get; set; } = null!;

    public int ErrorNumber { get; set; }

    public string? ErrorProcedure { get; set; }

    public string ErrorMessage { get; set; } = null!;
}
