using AutoMapper;
using matdev.Application.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace matdev.UnitTests;

internal static class TestMapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(
            cfg =>
            {
                cfg.LicenseKey = "";
                cfg.AddProfile<ApplicationMappingProfile>();
            },
            NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
        return config.CreateMapper();
    }
}
