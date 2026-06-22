export interface MonthlyRevenueDTO {
  month: string;
  amount: number;
}

export interface RecentTransactionDTO {
  studentName: string;
  amount: number;
  date: string;
  status: string;
}

export interface AdminDashboardDTO {
  totalStudents: number;
  totalTeachers: number;
  totalParents: number;
  totalRevenue: number;
  totalClasses: number;
  totalAssets: number;
  studentAttendanceRate: number;
  staffAttendanceRate: number;
  revenueTrends: MonthlyRevenueDTO[];
  recentTransactions: RecentTransactionDTO[];
}

export interface TeacherTimetableSlotDTO {
  id: number;
  dayOfWeek: string;
  startTime: string;
  endTime: string;
  className: string;
  section: string;
  subjectName: string;
}

export interface TeacherHomeworkDTO {
  id: number;
  title: string;
  className: string;
  section: string;
  subjectName: string;
  dueDate: string;
  submissionsCount: number;
  totalStudentsCount: number;
}

export interface TeacherDashboardDTO {
  totalStudents: number;
  totalClasses: number;
  totalSubjects: number;
  pendingHomeworkCount: number;
  studentAttendanceRate: number;
  todaySchedule: TeacherTimetableSlotDTO[];
  recentHomework: TeacherHomeworkDTO[];
}
