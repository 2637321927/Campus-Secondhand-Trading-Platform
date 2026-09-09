namespace Backend.Dtos.Admin;

public class AdminOrderPageDto
{
    public List<AdminOrderDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}

public class AdminOrderDto
{
    public long OrderId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreateTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? ShippingTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public decimal ShippingFees { get; set; }
    public decimal ProductPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public long? ProductCoverImageId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? ReceivingAddress { get; set; }
    public string? TrackingNumber { get; set; }
}

public class AdminOrderStatisticsDto
{
    public int TotalOrders { get; set; }
    public int PendingCount { get; set; }
    public int PaidCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int ShippingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
}
