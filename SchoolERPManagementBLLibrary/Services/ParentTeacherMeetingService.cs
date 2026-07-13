using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.ParentTeacherMeeting;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolERPManagementBLLibrary.Services;

public class ParentTeacherMeetingService : IParentTeacherMeetingService
{
    private readonly IRepository<int, Parentteachermeeting> _meetingRepository;
    private readonly IRepository<int, Parentteacherslot> _slotRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, Parent> _parentRepository;

    public ParentTeacherMeetingService(
        IRepository<int, Parentteachermeeting> meetingRepository,
        IRepository<int, Parentteacherslot> slotRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Parent> parentRepository)
    {
        _meetingRepository = meetingRepository;
        _slotRepository = slotRepository;
        _teacherRepository = teacherRepository;
        _parentRepository = parentRepository;
    }

    public async Task GenerateSlotsAsync(int meetingId, CancellationToken ct)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null)
            throw new EntityNotFoundException("ParentTeacherMeeting", meetingId.ToString());

        var existingSlots = await _slotRepository.Query(true)
            .AnyAsync(x => x.Meetingid == meetingId, ct);
        if (existingSlots)
            return;

        var teachers = await _teacherRepository.Query(true)
            .Where(t => t.User.Isactive == true)
            .ToListAsync(ct);

        var slots = new List<Parentteacherslot>();
        var slotDuration = TimeSpan.FromMinutes(meeting.Slotdurationminutes);
        var current = meeting.Starttime;

        while (current < meeting.Endtime)
        {
            var slotStart = current;
            var slotEnd = current.Add(slotDuration);
            if (slotEnd > meeting.Endtime)
                break;

            foreach (var teacher in teachers)
            {
                slots.Add(new Parentteacherslot
                {
                    Meetingid = meetingId,
                    Teacherid = teacher.Id,
                    Starttime = slotStart,
                    Endtime = slotEnd,
                    Status = "Available"
                });
            }

            current = slotEnd;
        }

        foreach (var slot in slots)
        {
            await _slotRepository.AddAsync(slot, save: false, ct: ct);
        }
        await _slotRepository.SaveChangesAsync(ct);
    }

    public async Task<List<PtmMeetingResponseDTO>> GetUpcomingMeetingsAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var meetings = await _meetingRepository.Query(true)
            .Where(m => m.Isactive && m.Eventdate >= today)
            .OrderBy(m => m.Eventdate)
            .ToListAsync(ct);

        return meetings.Select(m => new PtmMeetingResponseDTO(
            m.Id,
            m.Academiccalendarid,
            m.Eventdate,
            m.Starttime,
            m.Endtime,
            m.Description,
            m.Isactive
        )).ToList();
    }

    public async Task<List<PtmSlotResponseDTO>> GetMeetingSlotsAsync(int meetingId, int? teacherId, int? studentId, CancellationToken ct)
    {
        var query = _slotRepository.Query(true)
            .Where(s => s.Meetingid == meetingId);

        if (teacherId.HasValue)
            query = query.Where(s => s.Teacherid == teacherId.Value);

        if (studentId.HasValue)
        {
            var subjectTeacherIds = await _parentRepository.Query(true)
                .SelectMany(p => p.Studentparents)
                .Where(sp => sp.Studentid == studentId.Value)
                .SelectMany(sp => sp.Student.Studentenrollments)
                .SelectMany(e => e.Class.Teachersubjects)
                .Select(ts => ts.Teacherid)
                .Distinct()
                .ToListAsync(ct);

            var classTeacherIds = await _parentRepository.Query(true)
                .SelectMany(p => p.Studentparents)
                .Where(sp => sp.Studentid == studentId.Value)
                .SelectMany(sp => sp.Student.Studentenrollments)
                .Select(e => e.Class.Classteacherid)
                .Where(id => id != null)
                .Select(id => id!.Value)
                .Distinct()
                .ToListAsync(ct);

            var teacherIds = subjectTeacherIds.Union(classTeacherIds).ToList();
            if (teacherIds.Count == 0)
                return new List<PtmSlotResponseDTO>();

            query = query.Where(s => teacherIds.Contains(s.Teacherid));
        }

        var slots = await query
            .Include(s => s.Teacher)
            .Include(s => s.Parent)
            .Include(s => s.Student)
            .OrderBy(s => s.Teacherid)
            .ThenBy(s => s.Starttime)
            .ToListAsync(ct);

        return slots.Select(ToSlotDTO).ToList();
    }

    public async Task<PtmSlotResponseDTO> BookSlotAsync(int slotId, int parentId, int studentId, CancellationToken ct)
    {
        var slot = await _slotRepository.Query(true)
            .Include(s => s.Teacher)
            .Include(s => s.Parent)
            .Include(s => s.Student)
            .Include(s => s.Meeting)
            .FirstOrDefaultAsync(s => s.Id == slotId, ct);
        if (slot == null)
            throw new EntityNotFoundException("ParentTeacherSlot", slotId.ToString());

        if (!slot.Meeting.Isactive)
            throw new BusinessRuleException("This meeting is no longer accepting bookings.");

        if (slot.Status != "Available")
            throw new BusinessRuleException("This slot has already been booked.");

        var alreadyBooked = await _slotRepository.Query(true)
            .AnyAsync(s => s.Meetingid == slot.Meetingid
                && s.Teacherid == slot.Teacherid
                && s.Parentid == parentId
                && s.Status == "Booked", ct);
        if (alreadyBooked)
            throw new BusinessRuleException("You have already booked a slot with this teacher.");

        var parent = await _parentRepository.Query(true)
            .Include(p => p.Studentparents)
            .FirstOrDefaultAsync(p => p.Id == parentId, ct);
        if (parent == null)
            throw new EntityNotFoundException("Parent", parentId.ToString());

        if (!parent.Studentparents.Any(sp => sp.Studentid == studentId))
            throw new BusinessRuleException("You can only book slots for your own children.");

        slot.Status = "Booked";
        slot.Parentid = parentId;
        slot.Studentid = studentId;
        slot.Bookedat = DateTime.UtcNow;
        await _slotRepository.UpdateAsync(slot, save: true, ct: ct);

        return new PtmSlotResponseDTO(
            slot.Id,
            slot.Meetingid,
            slot.Teacherid,
            slot.Teacher.Name,
            slot.Starttime,
            slot.Endtime,
            slot.Status,
            slot.Parentid,
            slot.Parent?.Name,
            slot.Studentid,
            slot.Student?.Name
        );
    }

    public async Task CancelBookingAsync(int slotId, int userId, CancellationToken ct)
    {
        var slot = await _slotRepository.Query(true)
            .FirstOrDefaultAsync(s => s.Id == slotId, ct);
        if (slot == null)
            throw new EntityNotFoundException("ParentTeacherSlot", slotId.ToString());

        var parent = await _parentRepository.Query(true)
            .FirstOrDefaultAsync(p => p.Userid == userId, ct);

        if (parent != null && slot.Parentid.HasValue && slot.Parentid != parent.Id)
            throw new BusinessRuleException("You can only cancel your own bookings.");

        slot.Status = "Available";
        slot.Parentid = null;
        slot.Studentid = null;
        slot.Bookedat = null;
        await _slotRepository.UpdateAsync(slot, save: true, ct: ct);
    }

    public async Task<List<PtmSlotResponseDTO>> GetMyBookingsAsync(int parentId, CancellationToken ct)
    {
        var slots = await _slotRepository.Query(true)
            .Include(s => s.Teacher)
            .Include(s => s.Parent)
            .Include(s => s.Student)
            .Where(s => s.Parentid == parentId && s.Status == "Booked")
            .OrderBy(s => s.Starttime)
            .ToListAsync(ct);

        return slots.Select(s => ToSlotDTO(s)).ToList();
    }

    private static PtmSlotResponseDTO ToSlotDTO(Parentteacherslot s)
    {
        return new PtmSlotResponseDTO(
            s.Id,
            s.Meetingid,
            s.Teacherid,
            s.Teacher.Name,
            s.Starttime,
            s.Endtime,
            s.Status,
            s.Parentid,
            s.Parent?.Name,
            s.Studentid,
            s.Student?.Name
        );
    }
}
