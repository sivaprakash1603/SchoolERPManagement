using System;
using System.Collections.Generic;

namespace SchoolERPManagementModelLibrary.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int? Createdbyuserid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual User? Createdbyuser { get; set; }

    public virtual ICollection<Usernotification> Usernotifications { get; set; } = new List<Usernotification>();
}
