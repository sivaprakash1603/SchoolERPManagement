using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Homeworksubmission
{
    public int Id { get; set; }

    public int Homeworkid { get; set; }

    public int Studentid { get; set; }

    public string? Uploadedfileurl { get; set; }

    public string? Verificationstatus { get; set; }

    public decimal? Marks { get; set; }

    public string? Remarks { get; set; }

    public DateTime? Submittedat { get; set; }

    public virtual Homework Homework { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
