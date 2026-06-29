using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Classsubject
{
    public int Id { get; set; }

    public int Classid { get; set; }

    public int Subjectid { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
