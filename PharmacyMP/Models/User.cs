using System;
using System.Collections.Generic;

namespace PharmacyMP.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public int? RoleId { get; set; }

    public string? Name { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Role? Role { get; set; }
}
