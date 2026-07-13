import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ParentTeacherMeetingService,
  PtmMeetingResponseDTO,
  PtmSlotResponseDTO,
} from '../../services/parent-teacher-meeting.service';
import { ParentService } from '../../services/parent.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-parent-teacher-meetings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './parent-teacher-meetings.html',
})
export class ParentTeacherMeetings implements OnInit {
  private ptmService = inject(ParentTeacherMeetingService);
  private parentService = inject(ParentService);
  private toastService = inject(ToastService);

  meetings = signal<PtmMeetingResponseDTO[]>([]);
  selectedMeeting = signal<PtmMeetingResponseDTO | null>(null);
  slots = signal<PtmSlotResponseDTO[]>([]);
  selectedTeacherId = signal<number | null>(null);
  teachers = signal<{ id: number; name: string }[]>([]);

  myBookings = signal<PtmSlotResponseDTO[]>([]);
  children = signal<any[]>([]);
  selectedStudentId = signal<number | null>(null);

  loading = signal(false);
  slotsLoading = signal(false);
  bookingLoading = signal(false);
  userRole = signal<string>('');
  currentUserId = signal<number | null>(null);
  view = signal<'list' | 'slots' | 'bookings'>('list');

  ngOnInit() {
    this.userRole.set(sessionStorage.getItem('role') || '');
    const uidStr = sessionStorage.getItem('userId');
    this.currentUserId.set(uidStr ? parseInt(uidStr, 10) : null);
    this.fetchMeetings();
    if (this.userRole() === 'Parent') {
      this.fetchMyBookings();
      this.fetchParentAndChildren();
    }
  }

  fetchMeetings() {
    this.loading.set(true);
    this.ptmService.getUpcoming().subscribe({
      next: (data) => this.meetings.set(data),
      error: () => this.toastService.error('Failed to load meetings.'),
      complete: () => this.loading.set(false),
    });
  }

  fetchMyBookings() {
    this.ptmService.getMyBookings().subscribe({
      next: (data) => this.myBookings.set(data),
    });
  }

  fetchParentAndChildren() {
    const uid = this.currentUserId();
    if (!uid) return;
    this.parentService.getParentByUserId(uid).subscribe({
      next: (parent) => {
        this.parentService.getParentChildren(parent.id).subscribe({
          next: (kids) => this.children.set(kids),
        });
      },
    });
  }

  selectMeeting(meeting: PtmMeetingResponseDTO) {
    this.selectedMeeting.set(meeting);
    this.selectedTeacherId.set(null);
    this.teachers.set([]);
    this.view.set('slots');
    this.loadSlots();
  }

  onChildChange(studentId: any) {
    const parsedId = typeof studentId === 'string' ? parseInt(studentId, 10) : studentId;
    this.selectedStudentId.set(parsedId || null);
    this.selectedTeacherId.set(null);
    this.loadSlots();
  }

  loadSlots() {
    const meeting = this.selectedMeeting();
    const studentId = this.selectedStudentId();
    if (!meeting) return;
    if (this.userRole() === 'Parent' && !studentId) {
      this.slots.set([]);
      this.teachers.set([]);
      return;
    }
    this.slotsLoading.set(true);
    this.ptmService.getSlots(meeting.id, undefined, studentId || undefined).subscribe({
      next: (data) => {
        this.slots.set(data);
        const unique = new Map<number, string>();
        data.forEach((s) => unique.set(s.teacherId, s.teacherName));
        this.teachers.set(Array.from(unique, ([id, name]) => ({ id, name })));
      },
      error: () => this.toastService.error('Failed to load slots.'),
      complete: () => this.slotsLoading.set(false),
    });
  }

  filterByTeacher(teacherId: number | null) {
    this.selectedTeacherId.set(teacherId);
  }

  get filteredSlots(): PtmSlotResponseDTO[] {
    const tid = this.selectedTeacherId();
    return tid ? this.slots().filter((s) => s.teacherId === tid) : this.slots();
  }

  bookSlot(slot: PtmSlotResponseDTO) {
    const studentId = this.selectedStudentId();
    if (!studentId) {
      this.toastService.warning('Please select a child first.');
      return;
    }
    this.bookingLoading.set(true);
    this.ptmService.bookSlot({ slotId: slot.id, studentId }).subscribe({
      next: () => {
        this.toastService.success('Slot booked successfully!');
        this.bookingLoading.set(false);
        this.loadSlots();
        this.fetchMyBookings();
      },
      error: (err) => {
        this.bookingLoading.set(false);
        this.toastService.error(err.error?.message || 'Failed to book slot.');
      },
    });
  }

  cancelBooking(booking: PtmSlotResponseDTO) {
    this.ptmService.cancelSlot(booking.id).subscribe({
      next: () => {
        this.toastService.success('Booking cancelled.');
        this.fetchMyBookings();
        this.loadSlots();
      },
      error: (err) => this.toastService.error(err.error?.message || 'Failed to cancel.'),
    });
  }

  backToList() {
    this.view.set('list');
    this.selectedMeeting.set(null);
    this.fetchMeetings();
  }

  showBookings() {
    this.view.set('bookings');
  }
}
