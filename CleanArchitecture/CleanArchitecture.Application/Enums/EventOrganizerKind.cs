namespace CleanArchitecture.Core.Enums
{
    /// <summary>
    /// Etiketleme için; gerçek kaynak <see cref="Entities.EventClub"/> bağlantılarıdır:
    /// 0 satır Üniversite, 1 satır tek kulüp, 2+ ortak kulüpler.
    /// </summary>
    public enum EventOrganizerKind
    {
        University,
        Club,
        JointClub
    }

    public static class EventOrganizerKindRules
    {
        public static EventOrganizerKind FromHostCount(int count) =>
            count <= 0 ? EventOrganizerKind.University :
            count == 1 ? EventOrganizerKind.Club : EventOrganizerKind.JointClub;
    }
}
