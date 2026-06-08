using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Exam
{
    public int Id { get; set; }

    public string Examname { get; set; } = null!;

    public int? Academicyearid { get; set; }

    public virtual Academicyear? Academicyear { get; set; }

    public virtual ICollection<Examresult> Examresults { get; set; } = new List<Examresult>();

    public virtual ICollection<Examschedule> Examschedules { get; set; } = new List<Examschedule>();
}
