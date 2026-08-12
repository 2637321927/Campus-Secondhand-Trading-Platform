namespace Backend.Dtos.Review;

/// <summary>
/// 创建评价请求
/// </summary>
public class CreateReviewDto
{
    /// <summary>
    /// 评分：1-5
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// 评价内容
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// 评价图片文件ID列表
    /// </summary>
    public List<long>? ImageIds { get; set; }
}

/// <summary>
/// 评价响应
/// </summary>
public class ReviewDto
{
    public int ReviewId { get; set; }
    public int Rating { get; set; }
    public string? Info { get; set; }
    public DateTime ReviewTime { get; set; }
    public long PurchaseId { get; set; }

    // 被评价方回复
    public string? ReplyInfo { get; set; }
    public DateTime? ReplyTime { get; set; }

    // 评价人信息
    public int ReviewerId { get; set; }
    public string? ReviewerName { get; set; }

    // 被评价人信息
    public int RevieweeId { get; set; }
    public string? RevieweeName { get; set; }

    // 商品信息
    public long ProductId { get; set; }
    public string? ProductName { get; set; }

    // 是否已屏蔽
    public int IsHidden { get; set; }

    // 评价图片
    public List<ReviewImageDto> Images { get; set; } = new();
}

/// <summary>
/// 评价图片响应
/// </summary>
public class ReviewImageDto
{
    public long ImgFileId { get; set; }
    public int ImgIndex { get; set; }
}

/// <summary>
/// 评价回复请求
/// </summary>
public class ReplyReviewDto
{
    /// <summary>
    /// 回复内容
    /// </summary>
    public string ReplyInfo { get; set; } = string.Empty;
}
