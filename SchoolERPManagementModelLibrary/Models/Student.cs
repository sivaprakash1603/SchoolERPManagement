using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Student
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Regno { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    
    public string LastName { get; set; } = null!;

    public string? Gender { get; set; }

    public string? Bloodgroup { get; set; }

    public DateOnly? Dateofbirth { get; set; }

    public DateOnly? Admissiondate { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Examresult> Examresults { get; set; } = new List<Examresult>();

    public virtual ICollection<Feepayment> Feepayments { get; set; } = new List<Feepayment>();

    public virtual ICollection<Homeworksubmission> Homeworksubmissions { get; set; } = new List<Homeworksubmission>();

    public virtual ICollection<Studentparent> Studentparents { get; set; } = new List<Studentparent>();

    public virtual ICollection<Studentdocument> Studentdocuments { get; set; } = new List<Studentdocument>();

    public virtual ICollection<Studentenrollment> Studentenrollments { get; set; } = new List<Studentenrollment>();

    public virtual User User { get; set; } = null!;
}
