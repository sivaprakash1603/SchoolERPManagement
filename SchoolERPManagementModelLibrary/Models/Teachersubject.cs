using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Teachersubject
{
    public int Id { get; set; }

    public int Teacherid { get; set; }

    public int Subjectid { get; set; }

    public int Classid { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
