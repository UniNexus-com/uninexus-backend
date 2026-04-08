namespace CleanArchitecture.Core.DTOs.Inventory
{
    public class AssetViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Condition { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public decimal? Value { get; set; }
        public string SerialNo { get; set; }
        public string Description { get; set; }
        public int? ClubId { get; set; }
    }
}
