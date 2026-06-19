using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class AssetService : IAssetService
{
    private readonly IRepository<int, Asset> _assetRepository;
    private readonly IRepository<int, Assetreport> _assetReportRepository;
    private readonly IRepository<int, Assettype> _assetTypeRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IMapper _mapper;

    public AssetService(
        IRepository<int, Asset> assetRepository,
        IRepository<int, Assetreport> assetReportRepository,
        IRepository<int, Assettype> assetTypeRepository,
        IRepository<int, Class> classRepository,
        IMapper mapper)
    {
        _assetRepository = assetRepository;
        _assetReportRepository = assetReportRepository;
        _assetTypeRepository = assetTypeRepository;
        _classRepository = classRepository;
        _mapper = mapper;
    }

    public async Task<AssetResponseDTO> AddAssetAsync(CreateAssetDTO dto, CancellationToken cancellationToken)
    {
        if (dto.AssettypeId.HasValue && await _assetTypeRepository.GetByIdAsync(dto.AssettypeId.Value) is null)
        {
            throw new EntityNotFoundException("Asset type", dto.AssettypeId.Value.ToString());
        }

        if (dto.AssignedClassId.HasValue && await _classRepository.GetByIdAsync(dto.AssignedClassId.Value) is null)
        {
            throw new EntityNotFoundException("Class", dto.AssignedClassId.Value.ToString());
        }

        var asset = new Asset
        {
            Assetname = dto.Assetname,
            Assettypeid = dto.AssettypeId,
            Purchasedate = dto.Purchasedate,
            Warrantyexpiry = dto.Warrantyexpiry,
            Status = dto.Status?.ToLower(),
            Assignedclassid = dto.AssignedClassId
        };

        await _assetRepository.AddAsync(asset, save: true, ct: cancellationToken);
        return _mapper.Map<AssetResponseDTO>(asset);
    }

    public async Task<AssetReportResponseDTO> ReportAssetIssueAsync(AssetIssueDTO dto, CancellationToken cancellationToken)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
        if (asset is null)
        {
            throw new EntityNotFoundException("Asset", dto.AssetId.ToString());
        }

        asset.Status = dto.Status?.ToLower();
        await _assetRepository.UpdateAsync(asset, save: true, ct: cancellationToken);

        var report = new Assetreport
        {
            Assetid = dto.AssetId,
            Status = dto.Status?.ToLower(),
            Report = dto.Report,
            Createdat = DateTime.UtcNow
        };

        await _assetReportRepository.AddAsync(report, save: true, ct: cancellationToken);
        return _mapper.Map<AssetReportResponseDTO>(report);
    }

    public async Task<IReadOnlyList<AssetResponseDTO>> GetAssetsAsync(CancellationToken cancellationToken)
    {
        var items = await _assetRepository.Query(true).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<AssetResponseDTO>>(items);
    }
}
