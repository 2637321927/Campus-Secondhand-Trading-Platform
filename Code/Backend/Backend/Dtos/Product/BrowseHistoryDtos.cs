namespace Backend.Dtos.Product;

public class BrowseHistoryDto
{
    public long ViewId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public long? ProductImageFileId { get; set; }
    public DateTime ViewTime { get; set; }
}
