using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Contexts;

namespace SchoolERPManagementBLLibrary.Services
{
    public class SystemSetupService : ISystemSetupService
    {
        private readonly SchoolERPDbContext _context;

        public SystemSetupService(SchoolERPDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsSetupCompleteAsync()
        {
            // The system is considered setup if there is at least one Academic Year.
            return await _context.Academicyears.AnyAsync();
        }
    }
}
