using System.Linq;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Event;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Features.Events.Commands.CreateEvent;

namespace CleanArchitecture.Core.Mappings
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventViewModel>()
                .ForMember(dest => dest.OrganizerKind, opt => opt.Ignore())
                .ForMember(dest => dest.HostClubs, opt => opt.Ignore())
                .ForMember(dest => dest.ClubId, opt => opt.Ignore())
                .ForMember(dest => dest.ClubName, opt => opt.Ignore())
                .ForMember(dest => dest.IsRegistered, opt => opt.Ignore())
                .AfterMap(MapOrganizerFields);
            CreateMap<CreateEventCommand, Event>()
                .ForMember(dest => dest.EventClubs, opt => opt.Ignore());
        }

        private static void MapOrganizerFields(Event src, EventViewModel dest)
        {
            var hosts = (src.EventClubs ?? Enumerable.Empty<EventClub>())
                .OrderBy(ec => ec.SortOrder)
                .Select(ec => new EventHostBriefDto
                {
                    ClubId = ec.ClubId,
                    Name = ec.Club?.Name,
                    SortOrder = ec.SortOrder
                })
                .ToList();

            dest.HostClubs = hosts;
            var kind = EventOrganizerKindRules.FromHostCount(hosts.Count);
            dest.OrganizerKind = kind.ToString();

            switch (hosts.Count)
            {
                case 0:
                    dest.ClubId = null;
                    dest.ClubName = null;
                    break;
                case 1:
                    dest.ClubId = hosts[0].ClubId;
                    dest.ClubName = hosts[0].Name;
                    break;
                default:
                    dest.ClubId = null;
                    dest.ClubName = string.Join(" · ", hosts.Select(h => h.Name));
                    break;
            }
        }
    }
}
