using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Events.Commands.CreateEvent;

namespace CleanArchitecture.Core.Mappings
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventViewModel>()
                .ForMember(dest => dest.ClubId, opt => opt.MapFrom(src => src.ClubId))
                .ForMember(dest => dest.ClubName, opt => opt.MapFrom(src => src.Club != null ? src.Club.Name : null));
            CreateMap<CreateEventCommand, Event>();
        }
    }
}
