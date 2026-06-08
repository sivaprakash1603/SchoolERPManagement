using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Parentdocument
{
    public int Id { get; set; }

    public int Parentid { get; set; }

    public string Documenttype { get; set; } = null!;

    public string Bloburl { get; set; } = null!;

    public DateTime? Uploadedat { get; set; }

    public string Status { get; set; } = "Pending";

    public virtual Parent Parent { get; set; } = null!;
}
