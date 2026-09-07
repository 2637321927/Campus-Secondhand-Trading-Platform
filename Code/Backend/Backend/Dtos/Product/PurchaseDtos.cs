namespace Backend.Dtos.Product;

public class PurchaseDto
{
    public long PurchaseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public DateTime? CancelTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? ShippingTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public decimal ShippingFees { get; set; }
    public int ResponsibleForShip { get; set; }
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public long? ProductImageFileId { get; set; }
}
