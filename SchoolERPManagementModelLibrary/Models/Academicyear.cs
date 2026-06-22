using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Academicyear
{
    public int Id { get; set; }

    public string Yearname { get; set; } = null!;

    public DateOnly Startdate { get; set; }

    public DateOnly Enddate { get; set; }

    public bool? Iscurrent { get; set; }

    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Feestructure> Feestructures { get; set; } = new List<Feestructure>();

    public virtual ICollection<Studentenrollment> Studentenrollments { get; set; } = new List<Studentenrollment>();

    public virtual ICollection<Academiccalendar> Academiccalendars { get; set; } = new List<Academiccalendar>();
}
