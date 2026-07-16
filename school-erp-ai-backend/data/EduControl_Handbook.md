# EduControl School ERP Management Handbook
**Comprehensive Guide to the EduControl Platform**

*Version 2.0.4 | Effective Date: July 16, 2026*

Welcome to the EduControl School ERP Management System. This document is the ultimate, definitive guide for all users—Administrators, Teachers, Parents, and Students. It covers every aspect of the platform, from strict legal compliance and data security policies to minute, step-by-step instructions for every module, feature, and workflow within the system. Please read this handbook carefully to ensure you are utilizing the platform securely, efficiently, and within the bounds of our terms.

---

## Part I: Legal, Privacy, and Compliance Policies

### 1. Comprehensive Privacy Policy
EduControl is fundamentally committed to the privacy, security, and integrity of all educational records and personal data. This privacy policy applies to all modules of the EduControl platform.

#### 1.1 Data Collection and Usage
- **Personally Identifiable Information (PII):** We collect names, dates of birth, addresses, government identification numbers (where mandated by local law), and contact information (phone numbers, email addresses).
- **Educational Records:** This includes continuous academic performance metrics, daily attendance logs, behavioral reports, examination results, homework submissions, and teacher evaluations.
- **Financial Data:** We track fee components, payment histories, outstanding balances, and transaction references. Note: Actual credit card processing is offloaded to Stripe. We do not store PANs (Primary Account Numbers) or CVVs on our servers.
- **Usage Metrics:** We automatically log IP addresses, browser types, session durations, and interaction events to maintain security, optimize UI/UX, and investigate potential abuse.

#### 1.2 Regulatory Compliance Framework
- **FERPA (Family Educational Rights and Privacy Act):** We comply entirely with FERPA. Educational records are strictly partitioned. Only authorized school officials with a legitimate educational interest may access a student's records without parental consent. Parents retain the right to inspect, review, and request amendments to these records.
- **COPPA (Children's Online Privacy Protection Act):** We do not knowingly collect personal information from children under 13 without verifiable parental consent. Student accounts are generated exclusively via administrative provisioning tied to verified parent accounts.
- **GDPR (General Data Protection Regulation):** For institutions operating within the EU, we act as a Data Processor. We enforce the rights of Data Subjects (Right to Access, Right to be Forgotten, Right to Rectification). All European data is localized to EU-based data centers.

#### 1.3 Data Security Protocols
- **Encryption:** All data in transit is encrypted using TLS 1.3. All data at rest is encrypted using AES-256 block-level encryption on our PostgreSQL database instances.
- **Role-Based Access Control (RBAC):** Access control is hardcoded at the API controller level. A student token cannot access another student's data. A teacher token cannot access administrative financial reports.
- **Data Retention and Destruction:** Upon a student's graduation or withdrawal, records are archived for the mandated statutory period (typically 5-7 years, depending on jurisdiction) before undergoing cryptographic shredding.

### 2. Terms of Service and Usage Agreement

By logging into the EduControl platform, you electronically consent to be legally bound by the following terms. If you do not agree, you must immediately cease using the platform.

#### 2.1 Acceptable Use Policy (AUP)
- **Authorized Use:** The platform is provided exclusively for the administration of educational activities, academic monitoring, and school-related financial transactions.
- **Prohibited Conduct:** Users are expressly forbidden from:
  1. Attempting to bypass, disable, or circumvent any security mechanisms, rate limits, or authentication flows.
  2. Utilizing the platform to transmit malware, trojans, or unauthorized mass communications (spam).
  3. Engaging in cyberbullying, harassment, or posting defamatory material via the homework submissions, chat modules, or meeting notes.
  4. Scraping, data mining, or utilizing automated bots to extract PII from the system.

#### 2.2 Intellectual Property Rights
- **Platform Ownership:** EduControl retains all intellectual property rights, copyrights, and trademarks associated with the software architecture, UI design, source code, and underlying algorithms.
- **User Content:** Users retain ownership of the content they upload (e.g., student homework, teacher lesson plans). However, by uploading, users grant EduControl a limited, non-exclusive license to host, process, and display this content for the sole purpose of operating the service.

#### 2.3 Limitation of Liability & Indemnification
- **Service Interruptions:** EduControl aims for 99.9% uptime. However, we are not liable for academic or financial damages resulting from temporary service outages, scheduled maintenance, or acts of God.
- **Indemnification:** You agree to indemnify and hold harmless EduControl, its affiliates, and its employees from any claims, damages, or legal fees arising from your breach of this agreement or your misuse of the platform.
- **Disclaimer of Warranties:** The software is provided "AS IS" and "AS AVAILABLE" without express or implied warranties of merchantability or fitness for a particular purpose.

#### 2.4 Termination of Service
- The school administration reserves the unilateral right to suspend or permanently terminate access for any user found to be in violation of these Terms of Service.

---

## Part II: The Administrator Manual

Administrators are the backbone of the EduControl ecosystem. You possess elevated privileges that allow you to dictate the structural integrity of the school's digital presence.

### 3. Core System Configuration

#### 3.1 Initializing the Academic Calendar
Before any academic activity can commence, the academic year must be formally defined.
1. Authenticate using your Admin credentials and bypass the 2FA prompt.
2. Navigate to the **Academic Calendar** module located in the primary navigation sidebar.
3. Click the **Initialize New Year** button located in the top right quadrant of the screen.
4. Define the start and end dates (e.g., September 1, 2026 – June 30, 2027).
5. Specify the holiday schedule by clicking on the integrated calendar widget to highlight non-instructional days.
6. Click **Commit Configuration**. This creates the baseline framework against which attendance and timetables are validated.

#### 3.2 Defining Classes and Subjects
A robust taxonomy is required to map students to curriculum.
1. Navigate to the **Classes** module.
2. Click **Create Class Taxonomy**. Enter the Grade level (e.g., Grade 10) and Section identifier (e.g., Section A). Save the record.
3. Next, navigate to the **Subjects** module.
4. Define standard subjects (e.g., Advanced Calculus, World History).
5. Return to the **Classes** module, select a specific class, and click **Map Subjects**. Check the boxes for all subjects that belong to this class's curriculum.

### 4. Human Resources & User Onboarding

#### 4.1 Onboarding Teaching Staff
1. Navigate to the **Teachers** module under the HR grouping in the sidebar.
2. Click **Onboard New Teacher**.
3. You must provide their full legal name, date of birth, primary contact number, and institutional email address.
4. Input their academic qualifications and primary department.
5. Upon clicking **Provision Account**, the system dynamically generates a secure, randomized password and dispatches an automated welcome email containing login instructions and a mandate to change their password upon first login.

#### 4.2 Onboarding Students and Parents
1. Navigate to the **Students** module.
2. Click **Enroll New Student**.
3. Enter the student's demographic details, prior academic history, and assign them to an active Class and Section.
4. In the Parent/Guardian subset of the form, you must input the parent's email address.
5. If the parent's email already exists in the system (e.g., they have an older sibling enrolled), the system will automatically perform a relational link. If it does not exist, a new Parent account is created.
6. Click **Finalize Enrollment**.

### 5. Advanced Financial Management

#### 5.1 Architecting Fee Structures
1. Navigate to the **Fees** module.
2. Click **Manage Fee Structures**.
3. Select the target Class and Academic Year.
4. Add line-item components. For example: Click **Add Component**, type "Tuition Fee", enter the numerical value "20000", and specify the exact due date.
5. Repeat for ancillary fees (e.g., Library Fee, Laboratory Fee, Transport Fee).
6. Click **Publish Structure**. The system immediately recalculates outstanding balances for all students mapped to this class.

#### 5.2 Processing Manual Offline Payments
If a parent opts to pay via physical currency or bank draft at the administrative desk:
1. Navigate to the **Fees** dashboard.
2. Use the universal search bar to locate the specific student by their Registration Number.
3. Click on the student's row to open their comprehensive financial ledger.
4. Locate the specific pending fee component and click **Record Manual Payment**.
5. Select the payment modality (Cash, Cheque, Demand Draft) and input the transaction reference number if applicable.
6. Click **Process Receipt**. The system will generate a PDF receipt for the parent and update the ledger to reflect a zero balance for that component.

### 6. Asset Management & Tracking
1. Navigate to **Assets** in the sidebar.
2. Click **Register Asset**. Enter the SKU/Barcode, Asset Category (e.g., IT Hardware, Furniture), Purchase Date, and Initial Valuation.
3. Assign the asset to a specific room or staff member using the Location mapping tool.
4. The system will automatically calculate linear depreciation based on the purchase date and standard depreciation schedules.

### 7. AI SQL Database Query Tool
For complex reporting requirements that transcend the standard dashboards:
1. Navigate to the **Admin AI Query** terminal.
2. Type a natural language request. Example: *"Show me the names of all students in Grade 10-A who have an attendance rate below 75% and owe more than $500 in unpaid tuition fees."*
3. The AI agent translates this into a highly optimized PostgreSQL query, executes it against a read-replica database, and renders the output in an interactive data table.
4. You may export these results to CSV or PDF for immediate administrative action.

---

## Part III: The Teacher Manual

Teachers act as the frontline facilitators of the EduControl platform. Precision in your data entry is critical, as it directly influences parent dashboards and administrative analytics.

### 8. Academic Scheduling and Timetables
1. Upon logging in, your default landing page is the **Dashboard**, which provides a holistic overview of your day.
2. Click on the **Timetable** module.
3. The interface displays a chronological matrix of your teaching obligations. Each block indicates the Time Slot, the Class/Section, and the specific Room Number.
4. Any administrative overrides (e.g., substitution assignments) will be highlighted in yellow.

### 9. Managing Daily Attendance
Consistent attendance tracking is legally mandated and critical for student safety.
1. Navigate to the **Attendance** module.
2. The system defaults to the current date. Ensure the date is correct.
3. Select the Class you are currently teaching from the dropdown menu.
4. The system populates a roster of all active students in that section.
5. By default, all students are marked "Present".
6. To alter a status, click the toggle switch next to a student's name to cycle through "Absent" or "Late".
7. Once you have verified the roster against the physical classroom, click the **Submit Final Attendance** button.
8. **CRITICAL NOTE:** Once submitted, attendance data is locked. Modifications require a formal request to the Administrator. Parents of absent students are notified instantly.

### 10. Homework and Assignments Lifecycle

#### 10.1 Creating an Assignment
1. Navigate to the **Homework** module.
2. Click the primary action button labeled **Create New Assignment**.
3. Select the target Class and Subject.
4. Enter a descriptive Title (e.g., "Algebraic Expressions: Chapter 4 Review").
5. In the Rich Text Editor, provide explicit, detailed instructions. You may use formatting tools (bold, bullet points) to enhance readability.
6. Select the exact Date and Time for the submission deadline.
7. Click **Publish Assignment**. The assignment immediately synchronizes with the student and parent dashboards.

#### 10.2 Grading Submissions
1. In the **Homework** module, locate the previously published assignment under the "Active Assignments" tab.
2. Click on the assignment row to open the submission matrix.
3. You will see a list of all students. The status column will read "Pending", "Submitted", or "Overdue".
4. Click on a student whose status is "Submitted".
5. The system will display the student's text input and provide hyperlinks to download any attached files (e.g., PDFs, Word Documents).
6. Review the material. In the evaluation sidebar, input the numerical grade and provide constructive textual feedback.
7. Click **Save Grade**. The student is immediately notified of their evaluation.

### 11. Examinations and Report Cards
1. Navigate to the **Exams** module.
2. Select the specific Examination Term (e.g., "Mid-Term 1") and the Class.
3. The system presents a specialized spreadsheet-style interface.
4. Input the raw marks achieved by each student. The system actively validates the input against the maximum possible score to prevent typographical errors.
5. Once all marks are entered, click **Finalize Subject Results**.
6. When all teachers have finalized their subjects, the Administrator compiles the data, and the system automatically generates digital PDF report cards available to parents.

### 12. Parent-Teacher Meetings Configuration
1. Navigate to the **Parent-Teacher Meetings** module.
2. Select the **My Availability** tab.
3. Define the date range for upcoming conferences (e.g., November 10th to November 14th).
4. Specify your active hours (e.g., 3:00 PM to 5:00 PM) and define the duration of each slot (e.g., 15 minutes).
5. The system dynamically generates bookable slots. You will receive notifications as parents reserve their times.

---

## Part IV: The Parent Manual

The Parent Portal is designed to provide ultimate transparency into your child's academic journey and simplify your administrative responsibilities.

### 13. System Navigation & Multiple Child Management
1. **Login:** Access the portal using the credentials provided during enrollment. Ensure you bookmark the login page for rapid access.
2. **Dashboard Overview:** The dashboard acts as a central hub, presenting aggregated metrics such as average attendance, pending assignments, and unread notifications.
3. **Switching Contexts:** If you have multiple dependents enrolled in the institution, you do not need separate accounts. Look at the global header at the top of the interface. You will see a dropdown menu labeled "Select Child". Click this dropdown to seamlessly switch the context of the entire application. All subsequent actions will apply only to the selected child.

### 14. Financial Obligations & Online Payments
EduControl integrates directly with Stripe to provide a seamless, bank-grade secure payment experience.
1. Navigate to the **Fees** module via the left sidebar.
2. The interface is divided into two sections: "Outstanding Dues" and "Payment History".
3. Under "Outstanding Dues", you will see a detailed breakdown of all pending fee components (e.g., Tuition, Library).
4. Identify the component you wish to clear and click the corresponding **Pay Online** button.
5. The system will securely redirect you to the Stripe Checkout environment.
6. Input your payment methodology (Credit Card, Debit Card, or supported localized bank transfer options).
7. Confirm the transaction. Upon successful authorization, Stripe will redirect you back to the EduControl portal.
8. The fee ledger updates instantaneously in real-time, removing the balance and generating a downloadable digital receipt in the "Payment History" tab.

### 15. Monitoring Academic Progress
1. **Attendance Tracking:** Navigate to the **Attendance** module. A visual calendar displays your child's daily status. Green indicates present, red indicates absent. You can filter by month or academic term.
2. **Report Cards:** Navigate to the **Exams** module. Upon completion of an academic term, official PDF report cards will be listed here. Click the **Download** icon to save the document locally.

### 16. Booking Parent-Teacher Conferences
1. Navigate to the **Parent-Teacher Meetings** module.
2. From the dropdown list, select the specific subject teacher you wish to consult with.
3. A calendar widget will populate with available green slots representing the teacher's availability.
4. Click on a preferred 15-minute slot.
5. A confirmation modal will appear. Review the date and time, then click **Confirm Booking**.
6. Both you and the teacher will receive email confirmations, and the slot will be greyed out for other parents.

### 17. Document Uploads (KYC & Medical)
1. Navigate to the **Documents** module.
2. This section is used to submit critical paperwork (e.g., Vaccination Records, Identity Proofs, Absence Medical Certificates).
3. Click **Upload New Document**.
4. Select the Document Type from the dropdown, choose the file from your local machine, and click **Submit**.
5. The document status will remain "Pending Verification" until an Administrator reviews and approves it.

---

## Part V: The Student Manual

The Student Portal empowers learners to take ownership of their academic responsibilities through a streamlined, distraction-free interface.

### 18. Daily Scheduling
1. **The Dashboard:** Upon login, the dashboard immediately presents a summary of the next upcoming class and any homework due within the next 24 hours.
2. **The Timetable:** Navigate to the **Timetable** module. This provides a comprehensive weekly view of all classes. Use this module to organize your daily study materials and ensure you are prepared for the correct subjects.

### 19. The Homework Workflow
Submitting assignments on time is critical to academic success.
1. Navigate to the **Homework** module.
2. The interface defaults to the "Pending Assignments" tab, sorted by due date, with the most urgent tasks at the top.
3. Click on a specific assignment to open the detail view.
4. Carefully read the instructions provided by your teacher.
5. **Submission Methodology:**
   - *Text Submission:* If the assignment requires a short essay or mathematical answers, you may type directly into the provided Rich Text Editor.
   - *File Submission:* If the assignment requires you to upload a completed document (PDF, DOCX, JPG), click the **Upload File** button, select the file from your device, and wait for the progress bar to complete.
6. Once you are satisfied with your work, click the prominent **Submit Assignment** button.
7. **Verification:** The assignment will move to the "Completed" tab, and the status will update to "Submitted". You cannot edit an assignment after submission unless the teacher explicitly unlocks it.
8. **Reviewing Grades:** Once the teacher evaluates your work, navigate back to this assignment to read their feedback and view your final score.

### 20. Attendance and Examination Reviews
1. **Attendance:** Navigate to the **Attendance** module to view your own attendance record. This helps you ensure you are meeting the minimum required attendance thresholds for the semester.
2. **Exams:** Navigate to the **Exams** module to view your finalized term marks. This module provides a historical record of your academic trajectory throughout the year.

---

## Part VI: Exhaustive Frequently Asked Questions (FAQ)

The AI Chatbot is extensively trained on the following scenarios to provide immediate, autonomous assistance.

**Q: I have completely forgotten my password and cannot access the system. What is the exact recovery procedure?**
A: Do not panic. Navigate to the primary login portal. Below the credential inputs, click the hyperlink labeled "Forgot Password?". You will be prompted to enter your registered email address. The system will dispatch a Time-Based One-Time Password (OTP) to your inbox. Retrieve this OTP, enter it into the verification field, and you will be permitted to define a new, secure password.

**Q: I am attempting to view my child's Fee Component Name, but the table cell is entirely blank. Is the system broken?**
A: This is a known caching issue that occasionally occurs after backend system updates. Please execute a "Hard Refresh" on your browser (Ctrl + F5 on Windows, Cmd + Shift + R on Mac). If the component name remains blank after clearing your cache, please escalate the issue to the school's IT support team, as it may indicate a data mapping discrepancy in the PostgreSQL database.

**Q: What are the exact financial penalties for missing a scheduled fee payment deadline?**
A: EduControl enforces a strict automated penalty system. If a fee component is not cleared by midnight on the 5th of the month, a late fee of $10.00 is applied automatically on the 6th. An additional $10.00 is compounded every subsequent day the balance remains unpaid. If the balance remains uncleared for 30 consecutive days, the system automatically suspends portal access for both the parent and the student until the debt is reconciled.

**Q: Am I restricted to paying fees through the online Stripe gateway?**
A: No. While the Stripe gateway is highly recommended for its security and instant ledger reconciliation, parents may opt to pay offline. You must visit the administrative office in person during operating hours. You may pay via physical Cash, Cheque, or Demand Draft. The administrator will utilize the "Record Manual Payment" tool to log your transaction, at which point your digital ledger will reflect the payment.

**Q: My child was diagnosed with an illness and will be absent for an extended period. How do I properly report this?**
A: Minor absences (1-2 days) can be reported via a direct message to the class teacher using the portal's communication tools. For extended medical absences, you must navigate to the **Documents** module in the Parent Portal. Click "Upload New Document", select "Medical Certificate" as the category, and upload a scanned copy of the physician's note. This ensures the administration can properly excuse the absences without triggering disciplinary protocols.

**Q: How do I navigate the Parent-Teacher Meeting interface if I have multiple children enrolled in different grade levels?**
A: The system is designed to handle this seamlessly. Ensure you first utilize the global "Select Child" dropdown menu at the top of the interface to select the specific child whose academics you wish to discuss. Once selected, navigating to the **Parent-Teacher Meetings** module will exclusively display the teachers assigned to that specific child's class and subjects.

**Q: The AI SQL Query Tool is generating syntax errors when I ask complex statistical questions. How should I format my prompts?**
A: The AI tool translates Natural Language to SQL. For optimal results, use explicit, deterministic language. Instead of asking "Who is doing bad in school?", ask "Select all students in Grade 10 where the average exam score is less than 50%." Ensure you reference explicit entities (Students, Fees, Attendance) rather than colloquial terms.

**Q: Who assumes liability if a Stripe payment is charged to my credit card, but the EduControl ledger does not update?**
A: As per Section 2.3 of the Terms of Service, EduControl utilizes Stripe for transaction processing via webhooks. If a webhook fails due to a network interruption, the funds may be debited but not reflected in the UI. In this rare scenario, do not attempt to pay a second time. Forward your Stripe email receipt to `finance@educontrol.local`. An administrator will manually verify the Stripe dashboard and reconcile your ledger within 24 hours.

**Q: How do I contact the technical support team for catastrophic system failures?**
A: For critical infrastructure issues (e.g., 500 Internal Server Errors, complete inability to authenticate), please bypass the portal and email the IT Support desk directly at `support@educontrol.local`. Please include your browser version, operating system, and a screenshot of the specific error console if possible.
