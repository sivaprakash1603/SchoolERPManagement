using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Staffattendance
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public DateOnly Date { get; set; }

    public string Status { get; set; } = null!;

    public string Attendancetype { get; set; } = null!;

    public string? Remarks { get; set; }

    public virtual User User { get; set; } = null!;
}
