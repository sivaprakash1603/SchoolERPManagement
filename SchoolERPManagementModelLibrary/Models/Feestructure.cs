using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Feestructure
{
    public int Id { get; set; }

    public int Classid { get; set; }

    public int Academicyearid { get; set; }

    public decimal Totalamount { get; set; }

    public string? Feename { get; set; }

    public virtual Academicyear Academicyear { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;
}
