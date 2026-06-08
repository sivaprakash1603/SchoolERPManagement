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
        );
    }

    [Fact]
    public async Task AddAssetAsync_ValidData_ShouldAddAsset()
    {
        // Arrange
        var dto = new CreateAssetDTO("Projector", 1, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 1);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Assettype { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Class { Id = 1 });

        // Act
        var result = await _assetService.AddAssetAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Assetname.Should().Be("Projector");
        _assetRepoMock.Verify(r => r.AddAsync(It.IsAny<Asset>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAssetAsync_InvalidAssetType_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new CreateAssetDTO("Projector", 999, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 1);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Assettype?)null);

        // Act
        Func<Task> action = async () => await _assetService.AddAssetAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Asset type with identifier '999' was not found.");
    }

    [Fact]
    public async Task AddAssetAsync_InvalidClass_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new CreateAssetDTO("Projector", 1, DateOnly.Parse("2023-01-01"), DateOnly.Parse("2025-01-01"), "Active", 999);
        
        _assetTypeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Assettype { Id = 1 });
        _classRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Class?)null);

        // Act
        Func<Task> action = async () => await _assetService.AddAssetAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Class with identifier '999' was not found.");
    }

    [Fact]
    public async Task ReportAssetIssueAsync_ValidAsset_ShouldUpdateStatusAndAddReport()
    {
        // Arrange
        var dto = new AssetIssueDTO(1, "Broken", "Projector stopped working");
        var asset = new Asset { Id = 1, Assetname = "Projector", Status = "Active" };

        _assetRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(asset);

        // Act
        var result = await _assetService.ReportAssetIssueAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Broken");
        asset.Status.Should().Be("Broken");

        _assetRepoMock.Verify(r => r.UpdateAsync(asset, true, It.IsAny<CancellationToken>()), Times.Once);
        _assetReportRepoMock.Verify(r => r.AddAsync(It.IsAny<Assetreport>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReportAssetIssueAsync_InvalidAsset_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var dto = new AssetIssueDTO(999, "Broken", "Projector stopped working");
        _assetRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Asset?)null);

        // Act
        Func<Task> action = async () => await _assetService.ReportAssetIssueAsync(dto, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Asset with identifier '999' was not found.");
    }

    [Fact]
    public async Task GetAssetsAsync_ShouldReturnListOfAssets()
    {
        // Arrange
        var assets = new List<Asset>
        {
            new Asset { Id = 1, Assetname = "Projector" },
            new Asset { Id = 2, Assetname = "Laptop" }
        };

        _assetRepoMock.Setup(r => r.Query(true)).Returns(assets.AsQueryable().BuildMock());

        // Act
        var result = await _assetService.GetAssetsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Assetname.Should().Be("Projector");
    }
}
