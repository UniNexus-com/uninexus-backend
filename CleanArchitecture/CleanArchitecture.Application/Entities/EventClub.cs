namespace CleanArchitecture.Core.Entities
{
    /// <summary>
    /// Bir etkinliği düzenleyen kulüpler (sıralı çoktan çoğa ilişki).
    /// </summary>
    public class EventClub
    {
        public int EventId { get; set; }
        public Event Event { get; set; }

        public int ClubId { get; set; }
        public Club Club { get; set; }

        /// <summary>Ortak etkinlikte gösterim sırası.</summary>
        public int SortOrder { get; set; }
    }
}
