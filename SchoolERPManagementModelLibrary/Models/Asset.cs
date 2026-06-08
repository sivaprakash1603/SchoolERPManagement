using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Asset
{
    public int Id { get; set; }

    public string Assetname { get; set; } = null!;

    public int? Assettypeid { get; set; }

    public DateOnly? Purchasedate { get; set; }

    public DateOnly? Warrantyexpiry { get; set; }

    public string? Status { get; set; }

    public int? Assignedclassid { get; set; }

    public virtual ICollection<Assetreport> Assetreports { get; set; } = new List<Assetreport>();

    public virtual Assettype? Assettype { get; set; }

    public virtual Class? Assignedclass { get; set; }
}
