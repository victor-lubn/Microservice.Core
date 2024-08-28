using AutoMapper;

namespace Lueben.Microservice.EntityFunction.Tests
{
    public class TestMappingProfile : Profile
    {
        public TestMappingProfile()
        {
            CreateMap<TestModel, CreateTestCommand>();
            CreateMap<TestEntity, TestModel>();
            CreateMap<TestModel, TestEntity>();
            CreateMap<TestModel, TestCommand>();
            CreateMap<TestModel, TestCommand>();
        }
    }
}
