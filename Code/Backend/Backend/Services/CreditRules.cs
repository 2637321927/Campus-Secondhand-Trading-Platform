namespace Backend.Services;

/// <summary>
/// 信誉值调整规则常量：集中定义加减分值上下界，避免魔法数字散落各处。
/// </summary>
public static class CreditRules
{
    public const int Min = 0;
    public const int Max = 200;

    public const int OrderCompleted = 5;   // 订单成功（卖家）
    public const int GoodReview = 2;       // 好评 4-5（卖家）
    public const int BadReview = -3;       // 差评 1-2（卖家）
    public const int ReportAccepted = -5;  // 举报成立（被举报人）
    public const int AccountPenalty = -10; // 封禁/禁言/限制发布
    public const int Warned = -5;          // 警告
    public const int AppealRejected = -5;  // 申诉驳回（申诉人）
}
