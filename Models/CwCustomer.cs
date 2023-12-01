using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAppPedalaCom.Models;

public partial class CwCustomer
{
    [Key]
    public int CustomerId { get; set; }

    public string? Title { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public string? CompanyName { get; set; }

    public string? EmailAddress { get; set; }

    public string? Phone { get; set; }

    public string? PasswordHash { get; set; }

    public string? PasswordSalt { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
