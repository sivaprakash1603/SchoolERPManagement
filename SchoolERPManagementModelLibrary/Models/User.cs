using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int Roleid { get; set; }

    public bool? Isactive { get; set; }

    public DateTime? Createdat { get; set; }

    public string? Resettoken { get; set; }

    public DateTime? Resettokenexpiry { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Staffattendance> Staffattendances { get; set; } = new List<Staffattendance>();

    public virtual Parent? Parent { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }

    public virtual ICollection<Usernotification> Usernotifications { get; set; } = new List<Usernotification>();
}
