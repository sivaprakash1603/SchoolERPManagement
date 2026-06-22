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
    private readonly IMapper _mapper;

    public AcademicCalendarService(
        IRepository<int, Academiccalendar> calendarRepository,
        IRepository<int, Academicyear> academicYearRepository,
        IMapper mapper)
    {
        _calendarRepository = calendarRepository;
        _academicYearRepository = academicYearRepository;
        _mapper = mapper;
    }

    public async Task<CalendarEventResponseDTO> CreateCalendarEventAsync(CreateCalendarEventDTO dto, CancellationToken cancellationToken)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(dto.AcademicYearId);
        if (academicYear == null)
            throw new EntityNotFoundException("AcademicYear", dto.AcademicYearId.ToString());

        if (dto.Date < academicYear.Startdate || dto.Date > academicYear.Enddate)
            throw new BusinessRuleException("The event date must fall within the selected academic year start and end dates.");

        var existing = await _calendarRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Academicyearid == dto.AcademicYearId && x.Date == dto.Date, cancellationToken);

        if (existing != null)
            throw new DuplicateEntityException("AcademicCalendar", "Date", dto.Date.ToString("yyyy-MM-dd"));

        var calendarEvent = new Academiccalendar
        {
            Academicyearid = dto.AcademicYearId,
            Date = dto.Date,
            Description = dto.Description,
            Isholiday = dto.IsHoliday
        };

        await _calendarRepository.AddAsync(calendarEvent, save: true, ct: cancellationToken);
        return _mapper.Map<CalendarEventResponseDTO>(calendarEvent);
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
