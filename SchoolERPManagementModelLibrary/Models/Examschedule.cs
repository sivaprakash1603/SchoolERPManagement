using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Examschedule
{
    public int Id { get; set; }

    public int Examid { get; set; }

    public int Subjectid { get; set; }

    public int Classid { get; set; }

    public DateOnly Examdate { get; set; }

    public int Durationminutes { get; set; }

    public string? Session { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Exam Exam { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
