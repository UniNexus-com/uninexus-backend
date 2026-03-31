using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Core.Mappings
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventViewModel>();
            CreateMap<CleanArchitecture.Core.Features.Events.Commands.CreateEvent.CreateEventCommand, Event>();
        }
    }
}
