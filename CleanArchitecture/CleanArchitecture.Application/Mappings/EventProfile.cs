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
            CreateMap<Event, EventViewModel>();
            CreateMap<CreateEventCommand, Event>();
        }
    }
}
