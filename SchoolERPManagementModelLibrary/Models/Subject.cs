using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Subjectname { get; set; } = null!;

    public virtual ICollection<Examresult> Examresults { get; set; } = new List<Examresult>();

    public virtual ICollection<Examschedule> Examschedules { get; set; } = new List<Examschedule>();

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<Teachersubject> Teachersubjects { get; set; } = new List<Teachersubject>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual ICollection<Classsubject> Classsubjects { get; set; } = new List<Classsubject>();
}
