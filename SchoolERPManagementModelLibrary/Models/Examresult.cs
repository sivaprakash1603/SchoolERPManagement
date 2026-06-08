using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Examresult
{
    public int Id { get; set; }

    public int Examid { get; set; }

    public int Subjectid { get; set; }

    public int Studentid { get; set; }

    public decimal? Marks { get; set; }

    public string? Uploadedcorrectedanswersheeturl { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
