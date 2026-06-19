using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Studentparent
{
    public int Id { get; set; }

    public int Studentid { get; set; }

    public int Parentid { get; set; }

    public string Relation { get; set; } = null!;

    public bool Isprimarycontact { get; set; }

    public virtual Parent Parent { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
