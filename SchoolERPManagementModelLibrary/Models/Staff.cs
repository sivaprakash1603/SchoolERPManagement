using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Staff
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Stafftypeid { get; set; }

    public string Name { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

    public virtual Stafftype Stafftype { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
