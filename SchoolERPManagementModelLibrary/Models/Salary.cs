using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Salary
{
    public int Id { get; set; }

    public int? Staffid { get; set; }

    public int? Teacherid { get; set; }

    public decimal Currentsalary { get; set; }

    public DateOnly? Lastsalarydate { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
