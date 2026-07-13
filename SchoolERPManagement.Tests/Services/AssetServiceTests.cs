using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class AssetServiceTests
{
    private readonly Mock<IRepository<int, Asset>> _assetRepoMock;
    private readonly Mock<IRepository<int, Assetreport>> _assetReportRepoMock;
    private readonly Mock<IRepository<int, Assettype>> _assetTypeRepoMock;
    private readonly Mock<IRepository<int, Class>> _classRepoMock;
    private readonly AssetService _assetService;

    public AssetServiceTests()
    {
        _assetRepoMock = new Mock<IRepository<int, Asset>>();
        _assetReportRepoMock = new Mock<IRepository<int, Assetreport>>();
        _assetTypeRepoMock = new Mock<IRepository<int, Assettype>>();
        _classRepoMock = new Mock<IRepository<int, Class>>();

        _assetService = new AssetService(
            _assetRepoMock.Object,
            _assetReportRepoMock.Object,
            _assetTypeRepoMock.Object,
            _classRepoMock.Object
        ,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task AddAssetAsync_ValidData_ShouldAddAsset()
    {
        
        var dto = new CreateAssetDTO("Projector", 1, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 1);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Assettype { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });

        
        var result = await _assetService.AddAssetAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Assetname.Should().Be("Projector");
        _assetRepoMock.Verify(r => r.AddAsync(It.IsAny<Asset>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAssetAsync_InvalidAssetType_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new CreateAssetDTO("Projector", 999, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 1);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Assettype?)null);

        
        Func<Task> action = async () => await _assetService.AddAssetAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Asset type with identifier '999' was not found.");
    }

    [Fact]
    public async Task AddAssetAsync_InvalidClass_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new CreateAssetDTO("Projector", 1, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 999);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Assettype { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Class?)null);

        
        Func<Task> action = async () => await _assetService.AddAssetAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Class with identifier '999' was not found.");
    }

    [Fact]
    public async Task ReportAssetIssueAsync_ValidAsset_ShouldUpdateStatusAndAddReport()
    {
        
        var dto = new AssetIssueDTO(1, "Broken", "Projector stopped working");
        var asset = new Asset { Id = 1, Assetname = "Projector", Status = "Active" };

        _assetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);

        
        var result = await _assetService.ReportAssetIssueAsync(dto, CancellationToken.None);

        
        result.Should().NotBeNull();
        result.Status.Should().Be("broken");
        asset.Status.Should().Be("broken");

        _assetRepoMock.Verify(r => r.UpdateAsync(asset, true, It.IsAny<CancellationToken>()), Times.Once);
        _assetReportRepoMock.Verify(r => r.AddAsync(It.IsAny<Assetreport>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReportAssetIssueAsync_InvalidAsset_ShouldThrowEntityNotFoundException()
    {
        
        var dto = new AssetIssueDTO(999, "Broken", "Projector stopped working");
        _assetRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Asset?)null);

        
        Func<Task> action = async () => await _assetService.ReportAssetIssueAsync(dto, CancellationToken.None);

        
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Asset with identifier '999' was not found.");
    }

    [Fact]
    public async Task GetAssetsAsync_ShouldReturnListOfAssets()
    {
        
        var assets = new List<Asset>
        {
            new Asset { Id = 1, Assetname = "Projector" },
            new Asset { Id = 2, Assetname = "Laptop" }
        };

        _assetRepoMock.Setup(r => r.Query(true)).Returns(assets.BuildMockDbSet().Object);

        
        var result = await _assetService.GetAssetsAsync(CancellationToken.None);

        
        result.Should().HaveCount(2);
        result.First().Assetname.Should().Be("Projector");
    }

    [Fact]
    public async Task GetAssetTypesAsync_ShouldReturnListOfAssetTypes()
    {
        var assetTypes = new List<Assettype>
        {
            new Assettype { Id = 1, Typename = "Electronics" },
            new Assettype { Id = 2, Typename = "Furniture" }
        };

        _assetTypeRepoMock.Setup(r => r.Query(true)).Returns(assetTypes.BuildMockDbSet().Object);

        var result = await _assetService.GetAssetTypesAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Typename.Should().Be("Electronics");
    }

    [Fact]
    public async Task GetAssetReportsAsync_ShouldReturnListOfReports()
    {
        var reports = new List<Assetreport>
        {
            new Assetreport { Id = 1, Report = "Issue 1" },
            new Assetreport { Id = 2, Report = "Issue 2" }
        };

        _assetReportRepoMock.Setup(r => r.Query(true)).Returns(reports.BuildMockDbSet().Object);

        var result = await _assetService.GetAssetReportsAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Report.Should().Be("Issue 1");
    }

    [Fact]
    public async Task GetAssetStatsAsync_ShouldReturnCorrectStats()
    {
        var assets = new List<Asset>
        {
            new Asset { Id = 1, Status = "active" },
            new Asset { Id = 2, Status = "active" },
            new Asset { Id = 3, Status = "broken" },
            new Asset { Id = 4, Status = "under repair" },
            new Asset { Id = 5, Status = "under_repair" },
            new Asset { Id = 6, Status = null }
        };

        _assetRepoMock.Setup(r => r.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(assets);

        var result = await _assetService.GetAssetStatsAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalAssets.Should().Be(6);
        result.ActiveAssets.Should().Be(2);
        result.BrokenAssets.Should().Be(1);
        result.UnderRepairAssets.Should().Be(2);
    }
}
