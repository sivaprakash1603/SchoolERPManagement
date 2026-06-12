using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using SchoolERPManagementBLLibrary.Profiles;

namespace SchoolERPManagement.Tests.Helpers;

public static class TestHelper
{
    private static IMapper? _mapper;

    public static IMapper GetMapper()
    {
        if (_mapper == null)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile(new AppMappingProfile()));
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();
        }
        return _mapper;
    }
}
