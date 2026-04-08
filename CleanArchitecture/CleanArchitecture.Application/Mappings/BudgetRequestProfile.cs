using AutoMapper;
using CleanArchitecture.Core.DTOs.Finance;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Finance.Commands.CreateBudgetRequest;

namespace CleanArchitecture.Core.Mappings
{
    public class BudgetRequestProfile : Profile
    {
        public BudgetRequestProfile()
        {
            CreateMap<BudgetRequest, BudgetRequestViewModel>();
            CreateMap<CreateBudgetRequestCommand, BudgetRequest>();
        }
    }
}
