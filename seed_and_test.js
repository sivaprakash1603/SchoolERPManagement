const http = require('http');
const https = require('https');

const API_BASE = 'http://localhost:5203/api';
let adminToken = '';
let teacherToken = '';
let parentToken = '';
let studentToken = '';

// Shared Context
const ctx = {
    academicYearId: null,
    subjectId: null,
    classId: null,
    teacherId: null,
    teacherUserId: null,
    studentId: null,
    parentId: null,
    studentUserId: null,
    parentUserId: null,
    feeStructureId: null,
};

async function req(method, endpoint, body = null, token = adminToken) {
    const url = `${API_BASE}${endpoint}`;
    const options = {
        method,
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        }
    };
    if (token) {
        options.headers['Authorization'] = `Bearer ${token}`;
    }
    if (body) {
        options.body = JSON.stringify(body);
    }
    const res = await fetch(url, options);
    const text = await res.text();
    let data;
    try {
        data = text ? JSON.parse(text) : null;
    } catch {
        data = text;
    }
    if (!res.ok) {
        console.error(`❌ [${method}] ${endpoint} FAILED: ${res.status}`);
        console.error(data);
        throw new Error(`Request failed: ${res.status}`);
    }
    console.log(`✅ [${method}] ${endpoint} SUCCESS`);
    return data;
}

async function runTests() {
    console.log("=== STARTING API TESTS & SEEDING ===\n");

    try {
        console.log("1. AUTHENTICATION");
        // Reset Admin Password
        const forgotRes = await req('POST', '/Auth/forgot-password', { Email: 'admin@schoolerp.com' }, null);
        console.log("Forgot Password Response:", forgotRes);
        const resetToken = forgotRes.token || forgotRes.Token;
        await req('POST', '/Auth/reset-password', { Email: 'admin@schoolerp.com', ResetToken: resetToken, Token: resetToken, NewPassword: 'Admin@123' }, null);

        // Login as Admin
        const loginRes = await req('POST', '/Auth/login', { Username: 'admin', Password: 'Admin@123' }, null);
        console.log("Login Response:", loginRes);
        adminToken = loginRes.accessToken;
        console.log(`   Logged in as Admin.`);

        console.log("\n2. ACADEMIC SETUP");
        const years = await req('GET', '/AcademicYears');
        if (years.length > 0) {
            ctx.academicYearId = years[0].id || years[0].Id;
        } else {
            const year = await req('POST', '/AcademicYears', { YearName: '2026-2027', StartDate: '2026-06-01', EndDate: '2027-05-31', IsActive: true });
            ctx.academicYearId = year.id || year.Id;
        }

        const subjects = await req('GET', '/Subjects');
        if (subjects.length > 0) {
            ctx.subjectId = subjects[0].id || subjects[0].Id;
        } else {
            const sub = await req('POST', '/Subjects', { SubjectName: 'Mathematics', SubjectCode: 'MATH101' });
            ctx.subjectId = sub.id || sub.Id;
        }

        const classes = await req('GET', '/Classes');
        console.log("Classes Response:", classes);
        if (classes.length > 0) {
            ctx.classId = classes[0].id || classes[0].Id;
        } else {
            const cls = await req('POST', '/Classes', { ClassName: 'Grade 10', Section: 'A' });
            ctx.classId = cls.id || cls.Id;
        }

        console.log("\n3. USERS (TEACHER, PARENT, STUDENT)");
        const newTeacher = await req('POST', '/Teachers', {
            Name: 'John Doe Teacher',
            Email: `teacher_${Date.now()}@schoolerp.com`,
            Phonenumber: `${Date.now()}`.substring(3),
            Qualifications: 'M.Sc Mathematics'
        });
        ctx.teacherId = newTeacher.id || newTeacher.Id;
        ctx.teacherUserId = newTeacher.userId || newTeacher.UserId;
        const teacherPassword = newTeacher.generatedPassword || newTeacher.GeneratedPassword;

        // Map teacher to subject
        await req('POST', '/Teachers/assign-subject', {
            TeacherId: ctx.teacherId,
            SubjectId: ctx.subjectId,
            ClassId: ctx.classId
        });

        const newParent = await req('POST', '/Parents', {
            Name: 'Alice Smith Parent',
            Email: `parent_${Date.now()}@parent.com`,
            Phonenumber: `${Date.now()}`.substring(3),
            Relation: 'Mother'
        });
        ctx.parentId = newParent.id || newParent.Id;
        ctx.parentUserId = newParent.userId || newParent.UserId;
        const parentPassword = newParent.generatedPassword || newParent.GeneratedPassword;

        console.log("Context before creating student:", ctx);

        const newStudent = await req('POST', '/Students', {
            Name: 'Bob Smith Student',
            Email: `student_${Date.now()}@student.com`,
            ClassId: ctx.classId,
            AcademicYearId: ctx.academicYearId,
            ParentId: ctx.parentId
        });
        ctx.studentId = newStudent.id || newStudent.Id;
        ctx.studentUserId = newStudent.userId || newStudent.UserId;
        const studentPassword = newStudent.generatedPassword || newStudent.GeneratedPassword;

        console.log("\n4. FEES");
        const feeStruct = await req('POST', '/Fees/structure', {
            ClassId: ctx.classId,
            AcademicYearId: ctx.academicYearId,
            FeeName: 'Tuition Fee',
            TotalAmount: 15000.00
        });
        ctx.feeStructureId = feeStruct.id || feeStruct.Id;

        // Admin manual pay fee
        await req('POST', '/Fees/pay', {
            StudentId: ctx.studentId,
            FeeStructureId: ctx.feeStructureId,
            AmountPaid: 5000.00,
            PaymentDate: new Date().toISOString(),
            PaymentMethod: 'Cash',
            TransactionId: `TXN-${Date.now()}`
        });

        const feeHistory = await req('GET', `/Fees/student/${ctx.studentId}/history`);
        if (feeHistory.length === 0) throw new Error("Fee history is empty");

        console.log("\n5. ASSETS");
        const asset = await req('POST', '/Assets', {
            AssetName: 'Chemistry Lab Kit',
            Description: 'Complete toolkit for 10th grade',
            AssetTypeId: 1, // Hardware/Equipment
            PurchaseDate: '2026-01-01',
            Cost: 2000.00,
            Status: 'Active',
            Location: 'Lab 1'
        });

        console.log("\n6. ATTENDANCE");
        // We will log in as the teacher to take student attendance
        const tLogin = await req('POST', '/Auth/login', { Username: newTeacher.username || newTeacher.Username, Password: teacherPassword }, null);
        teacherToken = tLogin.accessToken;

        // Assuming there's a POST attendance logic
        const curDate = new Date().toISOString().split('T')[0];
        await req('POST', '/Attendance', {
            StudentId: ctx.studentId,
            Date: curDate,
            Status: 'Present',
            Remarks: 'Good'
        }, teacherToken);

        // Admin marks Staff attendance
        await req('POST', '/StaffAttendance', {
            UserId: ctx.teacherUserId,
            Date: curDate,
            Status: 'Present',
            AttendanceType: 'Full Day',
            Remarks: 'On Time'
        });

        console.log("\n7. HOMEWORK");
        const hwDate = new Date();
        hwDate.setDate(hwDate.getDate() + 3);
        const hwFd = new FormData();
        hwFd.append('ClassId', ctx.classId);
        hwFd.append('SubjectId', ctx.subjectId);
        hwFd.append('TeacherId', ctx.teacherId);
        hwFd.append('Title', 'Algebra Basics');
        hwFd.append('Description', 'Solve chapters 1 and 2');
        hwFd.append('DueDate', hwDate.toISOString().split('T')[0]);

        const hwRes = await fetch(`${API_BASE}/Homework`, {
            method: 'POST',
            body: hwFd,
            headers: {
                'Authorization': `Bearer ${teacherToken}`
            }
        });
        if (!hwRes.ok) throw new Error("Homework failed: " + hwRes.status);
        const hw = await hwRes.json();

        console.log("\n8. EXAMS");
        const exam = await req('POST', '/Exams', {
            Examname: 'Mid Term 2026',
            AcademicyearId: ctx.academicYearId
        });

        await req('POST', '/Exams/publish-result', {
            ExamId: exam.id || exam.Id,
            SubjectId: ctx.subjectId,
            StudentId: ctx.studentId,
            Marks: 85.5
        }, teacherToken);

        const examResults = await req('GET', `/Exams/student/${ctx.studentId}/results`, null, teacherToken);

        console.log("\n9. DASHBOARD");
        const dash = await req('GET', '/Dashboard/admin');
        console.log("   Dashboard Summary:", dash);

        console.log("\n10. REPORTS");
        const feesReport = await req('GET', '/Reports/fees');
        console.log("   Fees Report:", feesReport);

        const studentAttReport = await req('GET', '/Reports/attendance/students');
        console.log("   Student Attendance Report:", studentAttReport);

        const staffAttReport = await req('GET', '/Reports/attendance/staff');
        console.log("   Staff Attendance Report:", staffAttReport);

        const examReport = await req('GET', `/Reports/exams/${exam.id || exam.Id}`);
        console.log("   Exam Performance Report:", examReport);

        const assetReport = await req('GET', '/Reports/assets');
        console.log("   Asset Inventory Report:", assetReport);

        console.log("\n11. DYNAMIC QUERIES");
        const feesQuery = await req('GET', '/Reports/fees/query?PageNumber=1&PageSize=5&SortBy=AmountPaid&SortDirection=desc');
        console.log("   Paged Fees Query (Desc by Amount):", feesQuery);

        const studentAttQuery = await req('GET', `/Reports/attendance/students/query?StudentId=${ctx.studentId}&Status=Present`);
        console.log("   Student Attendance Query:", studentAttQuery);

        const examResultQuery = await req('GET', `/Reports/exams/results/query?ExamId=${exam.id || exam.Id}&MinMarks=50`);
        console.log("   Exam Results Query (>50 marks):", examResultQuery);

        console.log("\n✅ ALL ENDPOINTS TESTED AND DATA SEEDED SUCCESSFULLY!");
    } catch (e) {
        console.error("\n❌ SEEDING FAILED:", e.message);
    }
}

runTests();
