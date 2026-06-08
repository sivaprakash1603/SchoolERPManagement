using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Studentenrollment
{
    public int Id { get; set; }

    public int Studentid { get; set; }

    public int Classid { get; set; }

    public int Academicyearid { get; set; }

    public int? Rollnumber { get; set; }

    public virtual Academicyear Academicyear { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
