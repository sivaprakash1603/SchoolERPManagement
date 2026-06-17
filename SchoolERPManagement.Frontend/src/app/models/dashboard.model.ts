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
