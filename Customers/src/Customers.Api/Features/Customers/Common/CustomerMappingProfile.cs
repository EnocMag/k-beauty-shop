using AutoMapper;
using Customers.Domain.Entities;

namespace Customers.Features.Customers.Common;

public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerResponse>();
    }
}
