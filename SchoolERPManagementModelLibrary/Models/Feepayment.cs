using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Feepayment
{
    public int Id { get; set; }

    public int Studentid { get; set; }

    public decimal Amountpaid { get; set; }

    public int Feestructureid { get; set; }

    public DateTime? Paymentdate { get; set; }

    public string? Paymentmethod { get; set; }

    public string? Transactionid { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Feestructure Feestructure { get; set; } = null!;
}
