using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Parent
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public string? Relation { get; set; }

    public string? Phonenumber { get; set; }

    public virtual ICollection<Parentdocument> Parentdocuments { get; set; } = new List<Parentdocument>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual User User { get; set; } = null!;
}
