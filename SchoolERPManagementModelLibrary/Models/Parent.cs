using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Parent
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public string? Phonenumber { get; set; }

    public virtual ICollection<Parentdocument> Parentdocuments { get; set; } = new List<Parentdocument>();

    public virtual ICollection<Studentparent> Studentparents { get; set; } = new List<Studentparent>();

    public virtual User User { get; set; } = null!;
}
