namespace SchoolERPManagementBLLibrary.DTOs.Teacher;

public record AutoAssignResultDTO(
    int TotalAssignmentsMade,
    List<string> Messages
);
