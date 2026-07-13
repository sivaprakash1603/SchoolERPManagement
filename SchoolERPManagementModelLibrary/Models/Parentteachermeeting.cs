using System;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Parentteachermeeting
{
    public int Id { get; set; }

    public int? Academiccalendarid { get; set; }

    public int Academicyearid { get; set; }

    public DateOnly Eventdate { get; set; }

    public TimeOnly Starttime { get; set; }

    public TimeOnly Endtime { get; set; }

    public int Slotdurationminutes { get; set; }

    public string Description { get; set; } = null!;

    public bool Isactive { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Academiccalendar? Academiccalendar { get; set; }

    public virtual Academicyear Academicyear { get; set; } = null!;

    public virtual ICollection<Parentteacherslot> Parentteacherslots { get; set; } = new List<Parentteacherslot>();
}
