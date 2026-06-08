using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Assetreport
{
    public int Id { get; set; }

    public int Assetid { get; set; }

    public string Report { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Asset Asset { get; set; } = null!;
}
