using System;

namespace SchoolERPManagementBLLibrary.DTOs.Dashboard;

public record RecentTransactionDTO(
    string StudentName,
    decimal Amount,
    DateTime Date,
    string Status
);
