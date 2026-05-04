namespace CleanArchitecture.Core.Enums
{
    /// <summary>
    /// Etkinlik görünürlük seviyeleri (Event.Visibility alanı için string sabitler).
    /// Geri uyumluluk için string saklanır; null/boş veya bilinmeyen değerler <see cref="Public"/> kabul edilir.
    /// </summary>
    public static class EventVisibility
    {
        /// <summary>Tüm öğrenciler (varsayılan).</summary>
        public const string Public = "Public";

        /// <summary>Sadece host kulüp(ler)in aktif üyeleri ve SKS adminleri.</summary>
        public const string MembersOnly = "MembersOnly";

        /// <summary>Sadece host kulüp(ler)de "Manage Events" yetkisi olan liderler ve SKS adminleri.</summary>
        public const string Private = "Private";

        public static bool IsKnown(string value) =>
            value == Public || value == MembersOnly || value == Private;
    }
}
