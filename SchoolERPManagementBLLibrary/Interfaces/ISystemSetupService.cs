using System.Threading.Tasks;

namespace SchoolERPManagementBLLibrary.Interfaces
{
    public interface ISystemSetupService
    {
        Task<bool> IsSetupCompleteAsync();
    }
}
