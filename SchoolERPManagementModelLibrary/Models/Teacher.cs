using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Teacher
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public DateOnly? Joiningdate { get; set; }

    public string? Qualifications { get; set; }

    public int? SubjectSpecialtyId { get; set; }

    public virtual Subject? SubjectSpecialty { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Homework> Homeworks { get; set; } = new List<Homework>();

    public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

    public virtual ICollection<Teacherdocument> Teacherdocuments { get; set; } = new List<Teacherdocument>();

    public virtual ICollection<Teachersubject> Teachersubjects { get; set; } = new List<Teachersubject>();

    public virtual ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();

    public virtual User User { get; set; } = null!;
}
