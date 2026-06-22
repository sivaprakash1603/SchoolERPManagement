using System;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Academiccalendar
{
    public int Id { get; set; }

    public int Academicyearid { get; set; }

    public DateOnly Date { get; set; }

    public string Description { get; set; } = null!;

    public bool Isholiday { get; set; }

    public virtual Academicyear Academicyear { get; set; } = null!;
}
