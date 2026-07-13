using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;
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

public class AcademicCalendarService : IAcademicCalendarService
{
    private readonly IRepository<int, Academiccalendar> _calendarRepository;
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IRepository<int, Parentteachermeeting> _meetingRepository;
    private readonly IParentTeacherMeetingService _meetingService;
    private readonly IMapper _mapper;

    public AcademicCalendarService(
        IRepository<int, Academiccalendar> calendarRepository,
        IRepository<int, Academicyear> academicYearRepository,
        IRepository<int, Parentteachermeeting> meetingRepository,
        IParentTeacherMeetingService meetingService,
        IMapper mapper)
    {
        _calendarRepository = calendarRepository;
        _academicYearRepository = academicYearRepository;
        _meetingRepository = meetingRepository;
        _meetingService = meetingService;
        _mapper = mapper;
    }

    public async Task<CalendarEventResponseDTO> CreateCalendarEventAsync(CreateCalendarEventDTO dto, CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(dto.AcademicYearId);
        if (academicYear == null)
            throw new EntityNotFoundException("AcademicYear", dto.AcademicYearId.ToString());

        if (dto.Date < academicYear.Startdate || dto.Date > academicYear.Enddate)
            throw new BusinessRuleException("The event date must fall within the selected academic year start and end dates.");

        if (dto.EndDate.HasValue)
        {
            if (dto.EndDate.Value < academicYear.Startdate || dto.EndDate.Value > academicYear.Enddate)
                throw new BusinessRuleException("The event end date must fall within the selected academic year start and end dates.");
            if (dto.EndDate.Value < dto.Date)
                throw new BusinessRuleException("The event end date cannot be earlier than the start date.");
        }

        if (dto.IsParentTeacherMeeting)
        {
            if (dto.Date.DayOfWeek == DayOfWeek.Sunday)
                throw new BusinessRuleException("Parent-Teacher Meetings cannot be scheduled on Sundays.");
            if (!dto.PmtStartTime.HasValue || !dto.PmtEndTime.HasValue)
                throw new BusinessRuleException("Start time and end time are required for Parent-Teacher Meetings.");
            if (dto.PmtStartTime.Value >= dto.PmtEndTime.Value)
                throw new BusinessRuleException("Start time must be before end time.");
            if (dto.EndDate.HasValue)
                throw new BusinessRuleException("Parent-Teacher Meetings cannot span multiple days.");
        }

        var startDate = dto.Date;
        var endDate = dto.EndDate ?? dto.Date;
        var eventList = new List<Academiccalendar>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var existingOnDate = await _calendarRepository.Query(true)
                .FirstOrDefaultAsync(x => x.Academicyearid == dto.AcademicYearId && x.Date == date, cancellationToken);

            if (existingOnDate != null)
            {
                // If it's a single date, throw duplicate error
                if (!dto.EndDate.HasValue)
                {
                    throw new DuplicateEntityException("AcademicCalendar", "Date", dto.Date.ToString("yyyy-MM-dd"));
                }
                // For ranges, skip to prevent crash
                continue;
            }

            var calendarEvent = new Academiccalendar
            {
                Academicyearid = dto.AcademicYearId,
                Date = date,
                Description = dto.Description,
                Isholiday = dto.IsHoliday
            };
            await _calendarRepository.AddAsync(calendarEvent, save: true, ct: cancellationToken);
            eventList.Add(calendarEvent);
        }

        if (!eventList.Any())
        {
            throw new BusinessRuleException("All dates in the specified range already have registered events/holidays.");
        }

        // Create PTM meeting if this is a Parent-Teacher Meeting event
        if (dto.IsParentTeacherMeeting)
        {
            var meeting = new Parentteachermeeting
            {
                Academiccalendarid = eventList.First().Id,
                Academicyearid = dto.AcademicYearId,
                Eventdate = dto.Date,
                Starttime = dto.PmtStartTime!.Value,
                Endtime = dto.PmtEndTime!.Value,
                Slotdurationminutes = 15,
                Description = dto.Description,
                Isactive = true,
                Createdat = DateTime.UtcNow
            };
            await _meetingRepository.AddAsync(meeting, save: true, ct: cancellationToken);
            await _meetingService.GenerateSlotsAsync(meeting.Id, cancellationToken);
        }

        return _mapper.Map<CalendarEventResponseDTO>(eventList.First());
    }

    public async Task DeleteCalendarEventAsync(int id, CancellationToken cancellationToken)
    {
        var calendarEvent = await _calendarRepository.GetByIdAsync(id);
        if (calendarEvent == null)
            throw new EntityNotFoundException("AcademicCalendar", id.ToString());

        await _calendarRepository.DeleteAsync(calendarEvent, save: true, ct: cancellationToken);
    }

    public async Task<AcademicCalendarSummaryDTO> GetAcademicCalendarSummaryAsync(int academicYearId, CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(academicYearId);
        if (academicYear == null)
            throw new EntityNotFoundException("AcademicYear", academicYearId.ToString());

        var events = await _calendarRepository.Query(true)
            .Where(x => x.Academicyearid == academicYearId)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        // Perform calendar calculations
        DateOnly start = academicYear.Startdate;
        DateOnly end = academicYear.Enddate;
        int totalDays = 0;
        int weekendDays = 0;
        int weekdayHolidaysCount = 0;

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            totalDays++;
            var dayOfWeek = date.DayOfWeek;
            bool isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
            if (isWeekend)
            {
                weekendDays++;
            }
            else
            {
                var isMarkedHoliday = events.Any(e => e.Date == date && e.Isholiday);
                if (isMarkedHoliday)
                {
                    weekdayHolidaysCount++;
                }
            }
        }

        int workingDays = totalDays - weekendDays - weekdayHolidaysCount;
        var mappedEvents = _mapper.Map<IReadOnlyList<CalendarEventResponseDTO>>(events);

        return new AcademicCalendarSummaryDTO(
            mappedEvents,
            start,
            end,
            totalDays,
            weekendDays,
            weekdayHolidaysCount,
            workingDays
        );
    }
}
