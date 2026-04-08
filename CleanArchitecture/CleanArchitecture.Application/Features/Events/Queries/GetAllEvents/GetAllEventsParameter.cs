using CleanArchitecture.Core.Filters;

namespace CleanArchitecture.Core.Features.Events.Queries.GetAllEvents
{
    public class GetAllEventsParameter : RequestParameter
    {
        public int? ClubId { get; set; }
    }
}
