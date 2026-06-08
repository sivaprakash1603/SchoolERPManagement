using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Homework
{
    public int Id { get; set; }

    public int Subjectid { get; set; }

    public int Teacherid { get; set; }

    public int Classid { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Attachmenturl { get; set; }

    public DateTime? Createdat { get; set; }

    public DateOnly Duedate { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Homeworksubmission> Homeworksubmissions { get; set; } = new List<Homeworksubmission>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
