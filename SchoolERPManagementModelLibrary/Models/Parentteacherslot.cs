using System;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Parentteacherslot
{
    public int Id { get; set; }

    public int Meetingid { get; set; }

    public int Teacherid { get; set; }

    public TimeOnly Starttime { get; set; }

    public TimeOnly Endtime { get; set; }

    public string Status { get; set; } = null!;

    public int? Parentid { get; set; }

    public int? Studentid { get; set; }

    public DateTime? Bookedat { get; set; }

    public virtual Parentteachermeeting Meeting { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;

    public virtual Parent? Parent { get; set; }

    public virtual Student? Student { get; set; }
}
