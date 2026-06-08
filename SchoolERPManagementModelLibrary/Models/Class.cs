using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Class
{
    public int Id { get; set; }

    public string Classname { get; set; } = null!;

    public string Section { get; set; } = null!;

    public int? Classteacherid { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();

    public virtual Teacher? Classteacher { get; set; }

    public virtual ICollection<Examschedule> Examschedules { get; set; } = new List<Examschedule>();

    public virtual ICollection<Feestructure> Feestructures { get; set; } = new List<Feestructure>();

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<Studentenrollment> Studentenrollments { get; set; } = new List<Studentenrollment>();

    public virtual ICollection<Teachersubject> Teachersubjects { get; set; } = new List<Teachersubject>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
