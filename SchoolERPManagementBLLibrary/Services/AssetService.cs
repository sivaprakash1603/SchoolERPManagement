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

    public AssetService(
        IRepository<int, Asset> assetRepository,
        IRepository<int, Assetreport> assetReportRepository,
        IRepository<int, Assettype> assetTypeRepository,
        IRepository<int, Class> classRepository)
    {
        _assetRepository = assetRepository;
        _assetReportRepository = assetReportRepository;
        _assetTypeRepository = assetTypeRepository;
        _classRepository = classRepository;
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
            Status = dto.Status,
            Assignedclassid = dto.AssignedClassId
        };

        await _assetRepository.AddAsync(asset, save: true, ct: cancellationToken);
        return new AssetResponseDTO(asset.Id, asset.Assetname, asset.Assettypeid, asset.Purchasedate, asset.Warrantyexpiry, asset.Status, asset.Assignedclassid);
    }

    public async Task<AssetReportResponseDTO> ReportAssetIssueAsync(AssetIssueDTO dto, CancellationToken cancellationToken)
    {
        var asset = await _assetRepository.GetByIdAsync(dto.AssetId);
        if (asset is null)
        {
            throw new EntityNotFoundException("Asset", dto.AssetId.ToString());
        }

        asset.Status = dto.Status;
        await _assetRepository.UpdateAsync(asset, save: true, ct: cancellationToken);

        var report = new Assetreport
        {
            Assetid = dto.AssetId,
            Status = dto.Status,
            Report = dto.Report,
            Createdat = DateTime.UtcNow
        };

        await _assetReportRepository.AddAsync(report, save: true, ct: cancellationToken);
        return new AssetReportResponseDTO(report.Id, report.Assetid, report.Status, report.Report, report.Createdat);
    }

    public async Task<IReadOnlyList<AssetResponseDTO>> GetAssetsAsync(CancellationToken cancellationToken)
    {
        return await _assetRepository.Query(true)
            .Select(asset => new AssetResponseDTO(asset.Id, asset.Assetname, asset.Assettypeid, asset.Purchasedate, asset.Warrantyexpiry, asset.Status, asset.Assignedclassid))
            .ToListAsync(cancellationToken);
    }
}
