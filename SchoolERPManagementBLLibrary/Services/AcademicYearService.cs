using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.AcademicYear;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class AcademicYearService : IAcademicYearService
{
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IMapper _mapper;

    public AcademicYearService(IRepository<int, Academicyear> academicYearRepository, IMapper mapper)
    {
        _academicYearRepository = academicYearRepository;
        _mapper = mapper;
    }

    public async Task<AcademicYearResponseDTO> CreateAcademicYearAsync(CreateAcademicYearDTO dto, CancellationToken cancellationToken)
    {
        var academicYear = new Academicyear
        {
            Yearname = dto.YearName,
            Startdate = dto.StartDate,
            Enddate = dto.EndDate,
            Iscurrent = false
        };

        await _academicYearRepository.AddAsync(academicYear, save: true, ct: cancellationToken);
        return _mapper.Map<AcademicYearResponseDTO>(academicYear);
    }

    public async Task<IReadOnlyList<AcademicYearResponseDTO>> GetAllAcademicYearsAsync(CancellationToken cancellationToken)
    {
        var items = await _academicYearRepository.Query(true)
            .OrderByDescending(a => a.Startdate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<AcademicYearResponseDTO>>(items);
    }

    public async Task SetCurrentAcademicYearAsync(int id, CancellationToken cancellationToken)
    {
        var newCurrent = await _academicYearRepository.GetByIdAsync(id);
        if (newCurrent is null)
        {
            throw new EntityNotFoundException("AcademicYear", id.ToString());
        }

        var currentYears = await _academicYearRepository.Query(false)
            .Where(a => a.Iscurrent == true)
            .ToListAsync(cancellationToken);

        foreach (var year in currentYears)
        {
            year.Iscurrent = false;
            await _academicYearRepository.UpdateAsync(year, save: false, ct: cancellationToken);
        }

        newCurrent.Iscurrent = true;
        await _academicYearRepository.UpdateAsync(newCurrent, save: true, ct: cancellationToken);
    }
}
