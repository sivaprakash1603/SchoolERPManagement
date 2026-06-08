using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Usernotification
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Notificationid { get; set; }

    public bool? Isread { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
