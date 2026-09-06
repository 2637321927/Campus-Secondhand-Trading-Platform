namespace Backend.Dtos.Reputation;

/// <summary>
/// 用户信誉概览
/// </summary>
public class ReputationSummaryDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 信誉分（NormUser.Credit）
    /// </summary>
    public int Credit { get; set; }

    /// <summary>
    /// 总评价数
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// 好评数（评分 >= 4）
    /// </summary>
    public int GoodReviews { get; set; }

    /// <summary>
    /// 中评数（评分 = 3）
    /// </summary>
    public int NeutralReviews { get; set; }

    /// <summary>
    /// 差评数（评分 <= 2）
    /// </summary>
    public int BadReviews { get; set; }

    /// <summary>
    /// 好评率（百分比，如 95.5）
    /// </summary>
    public double GoodRate { get; set; }

    /// <summary>
    /// 平均评分（1-5）
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// 作为卖家的成交数（已完成订单数）
    /// </summary>
    public int CompletedSales { get; set; }

    /// <summary>
    /// 作为买家的成交数（已完成订单数）
    /// </summary>
    public int CompletedPurchases { get; set; }
}

/// <summary>
/// 用户信誉明细
/// </summary>
public class ReputationDetailDto
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 信誉分
    /// </summary>
    public int Credit { get; set; }

    /// <summary>
    /// 总评价数
    /// </summary>
    public int TotalReviews { get; set; }

    /// <summary>
    /// 好评数
    /// </summary>
    public int GoodReviews { get; set; }

    /// <summary>
    /// 中评数
    /// </summary>
    public int NeutralReviews { get; set; }

    /// <summary>
    /// 差评数
    /// </summary>
    public int BadReviews { get; set; }

    /// <summary>
    /// 好评率
    /// </summary>
    public double GoodRate { get; set; }

    /// <summary>
    /// 平均评分
    /// </summary>
    public double AverageRating { get; set; }

    /// <summary>
    /// 作为卖家成交数
    /// </summary>
    public int CompletedSales { get; set; }

    /// <summary>
    /// 作为买家成交数
    /// </summary>
    public int CompletedPurchases { get; set; }

    /// <summary>
    /// 近30天评价分布：{ "5": 10, "4": 5, "3": 1, "2": 0, "1": 0 }
    /// </summary>
    public Dictionary<int, int> RecentRatingDistribution { get; set; } = new();

    /// <summary>
    /// 是否有未处理的违规（WorkOrder）
    /// </summary>
    public bool HasPendingViolation { get; set; }

    /// <summary>
    /// 违规记录数量概览（WorkOrder统计）
    /// </summary>
    public int TotalWorkOrders { get; set; }

    /// <summary>
    /// 已完成工单数（被处理过的违规记录）
    /// </summary>
    public int CompletedWorkOrders { get; set; }
}
