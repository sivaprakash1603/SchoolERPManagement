using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Attendance
{
    public int Id { get; set; }

    public int Studentid { get; set; }

    public DateOnly Date { get; set; }

    public string Status { get; set; } = null!;

    public int? Markedbyteacherid { get; set; }

    public string? Remarks { get; set; }

    public virtual Teacher? Markedbyteacher { get; set; }

    public virtual Student Student { get; set; } = null!;
}
