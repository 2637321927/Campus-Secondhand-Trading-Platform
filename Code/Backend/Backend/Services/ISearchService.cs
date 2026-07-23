using Backend.Dtos.Product;

namespace Backend.Services;

/// <summary>
/// 搜索引擎服务 — 关键词分词 → TermGraph 查询扩展 → 数据库搜索 → 排序分页
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 核心搜索入口
    /// </summary>
    Task<SearchResultDto> SearchAsync(SearchRequestDto request);

    /// <summary>
    /// 通知搜索引擎有新商品创建，增量更新 TermGraph
    /// </summary>
    Task NotifyProductCreatedAsync(long productId);

    /// <summary>
    /// 从全部历史商品全量重建 TermGraph（首次部署或手动触发）
    /// </summary>
    Task RebuildGraphAsync();
}
