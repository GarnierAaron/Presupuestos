using AutoMapper;
using Presupuestos.Application.Dto.Budgets;
using Presupuestos.Application.Dto.Items;
using Presupuestos.Application.Dto.Services;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Item, ItemDto>();
        CreateMap<Service, ServiceDto>()
            .ForMember(d => d.ServiceItems, o => o.Ignore());
        CreateMap<Budget, BudgetDto>()
            .ForMember(d => d.Details, o => o.Ignore());
    }
}
