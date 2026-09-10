using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// 聚合当前用户的 浏览(1) / 收藏(3) / 成交(4) 行为，加权到商品所属小分类；
/// 偏好得分会向上折算到大分类（父分类取 60%）；
/// 候选 = 在售商品，按"分类偏好分 + 浏览量"排序；
/// 分数为 0 的候选自然按浏览量充当热度兜底；排除自己发布的、已收藏、已购买。
/// 详情页"猜你想看"：同小分类权重高、同大分类次之，叠加用户历史偏好分。
/// </summary>
public class RecommendService : IRecommendService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductViewRepository _viewRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly ICategoryRepository _categoryRepo;

    public RecommendService(
        IProductRepository productRepo,
        IProductViewRepository viewRepo,
        ICollectionRepository collectionRepo,
        IPurchaseRepository purchaseRepo,
        ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _viewRepo = viewRepo;
        _collectionRepo = collectionRepo;
        _purchaseRepo = purchaseRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<List<ProductCardDto>> RecommendForUserAsync(int? userId, int count)
    {
        var ctx = await BuildContextAsync(userId);
        var all = await LoadCandidatesAsync();
        ctx.ViewCounts = await _viewRepo.GetViewCountsAsync(all.Select(p => p.ProductId));

        var scored = ScoreCandidates(all, ctx,
            candidateLeaf => ctx.LeafScore.GetValueOrDefault(candidateLeaf, 0)
                             + (ctx.ParentOf.TryGetValue(candidateLeaf, out var parent)
                                 ? ctx.LeafScore.GetValueOrDefault(parent, 0) * 0.6
                                 : 0));

        var excluded = ctx.ExcludedIds;
        return BuildResult(scored, all, excluded, ctx.ViewCounts, count);
    }

    public async Task<List<ProductCardDto>> GetRelatedAsync(int? userId, long productId, int count)
    {
        var target = await _productRepo.GetByIdAsync(productId);
        var ctx = await BuildContextAsync(userId);

        var all = await LoadCandidatesAsync();
        ctx.ViewCounts = await _viewRepo.GetViewCountsAsync(all.Select(p => p.ProductId));
        var excluded = new HashSet<long>(ctx.ExcludedIds) { productId };

        var targetLeaf = target?.CategoryId;
        long? targetParent = targetLeaf.HasValue && ctx.ParentOf.TryGetValue(targetLeaf.Value, out var tp)
            ? tp
            : null;

        // 相关度 = 同小分类(3) > 同大分类(1.5) + 用户偏好分(0.5) + 热度微调
        var scored = ScoreCandidates(all, ctx,
            candidateLeaf =>
            {
                double rel = 0;
                if (targetLeaf.HasValue && candidateLeaf == targetLeaf.Value)
                    rel += 3;
                else if (targetParent.HasValue && ctx.ParentOf.TryGetValue(candidateLeaf, out var p) && p == targetParent.Value)
                    rel += 1.5;

                rel += 0.5 * ctx.LeafScore.GetValueOrDefault(candidateLeaf, 0);
                return rel;
            });

        return BuildResult(scored, all, excluded, ctx.ViewCounts, count);
    }

    private async Task<Ctx> BuildContextAsync(int? userId)
    {
        var ctx = new Ctx();
        var categories = await _categoryRepo.GetAllAsync();
        ctx.ParentOf = categories
            .Where(c => c.ParentId.HasValue)
            .ToDictionary(c => c.CategoryId, c => c.ParentId!.Value);

        if (!userId.HasValue)
            return ctx;

        var uid = userId.Value;

        // 收藏
        var favorites = await _collectionRepo.GetByUserIdAsync(uid);
        foreach (var f in favorites)
        {
            ctx.FavoriteIds.Add(f.ProductId);
            ctx.SignalProductIds.Add(f.ProductId);
        }

        // 浏览历史
        var views = await _viewRepo.GetByUserIdAsync(uid);
        foreach (var v in views)
            ctx.SignalProductIds.Add(v.ProductId);

        // 成交（去掉取消的订单）
        var purchases = await _purchaseRepo.GetByBuyerIdAsync(uid);
        foreach (var p in purchases)
        {
            if (string.Equals(p.Status, "cancel", StringComparison.OrdinalIgnoreCase))
                continue;
            ctx.PurchasedIds.Add(p.ProductId);
            ctx.SignalProductIds.Add(p.ProductId);
        }

        if (ctx.SignalProductIds.Count == 0)
            return ctx;

        // 取行为商品所属分类（一次投影，避免逐个 include）
        var ids = ctx.SignalProductIds.ToList();
        var metas = await _productRepo.Query()
            .Where(p => ids.Contains(p.ProductId))
            .Select(p => new { p.ProductId, p.CategoryId, p.UserId })
            .ToListAsync();

        var productOwner = metas.ToDictionary(m => m.ProductId, m => m.UserId);
        var categoryOf = metas
            .Where(m => ctx.LeafScore.ContainsKey(m.CategoryId) == false || true)
            .ToDictionary(m => m.ProductId, m => m.CategoryId);

        // 行为权重：收藏3 / 浏览1 / 成交4（成交/收藏集合中各自累加）
        foreach (var pid in favorites.Select(f => f.ProductId))
            if (categoryOf.TryGetValue(pid, out var leaf))
                ctx.LeafScore[leaf] = ctx.LeafScore.GetValueOrDefault(leaf) + 3;

        foreach (var pid in views.Select(v => v.ProductId))
            if (categoryOf.TryGetValue(pid, out var leaf))
                ctx.LeafScore[leaf] = ctx.LeafScore.GetValueOrDefault(leaf) + 1;

        foreach (var pid in ctx.PurchasedIds)
            if (categoryOf.TryGetValue(pid, out var leaf))
                ctx.LeafScore[leaf] = ctx.LeafScore.GetValueOrDefault(leaf) + 4;

        // 排除自己发布的（行为商品里含自己卖的浏览也排除，但直接拿 product owner）
        ctx.OwnedIds = metas
            .Where(m => m.UserId == uid)
            .Select(m => m.ProductId)
            .ToHashSet();

        return ctx;
    }

    private async Task<List<Product>> LoadCandidatesAsync()
        => await _productRepo.Query()
            .Where(p => p.Status == ProductStatus.Available)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

    private static List<(Product Product, double Score)> ScoreCandidates(
        List<Product> all,
        Ctx ctx,
        Func<long, double> categoryScoreOf)
        => all
            .Select(p => (Product: p, Score: categoryScoreOf(p.CategoryId)))
            .ToList();

    private static List<ProductCardDto> BuildResult(
        List<(Product Product, double Score)> scored,
        List<Product> all,
        HashSet<long> excluded,
        Dictionary<long, int> viewCounts,
        int count)
    {
        var candidate = scored
            .Where(item => !excluded.Contains(item.Product.ProductId))
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => viewCounts.GetValueOrDefault(item.Product.ProductId, 0))
            .ThenBy(item => item.Product.ProductId)
            .Take(count)
            .Select(item => ProductService.ToProductCard(
                item.Product,
                viewCounts.GetValueOrDefault(item.Product.ProductId, 0)))
            .ToList();

        // 兜底：分数为 0（无行为偏好）时也能有结果——上述排序已让热门自然补位
        return candidate;
    }

    private sealed class Ctx
    {
        public Dictionary<long, double> LeafScore { get; } = new();
        public Dictionary<long, long> ParentOf { get; set; } = new();
        public HashSet<long> FavoriteIds { get; } = new();
        public HashSet<long> PurchasedIds { get; } = new();
        public HashSet<long> OwnedIds { get; set; } = new();
        public HashSet<long> SignalProductIds { get; } = new();

        public Dictionary<long, int> ViewCounts { get; set; } = new();

        public HashSet<long> ExcludedIds
        {
            get
            {
                var set = new HashSet<long>();
                foreach (var id in FavoriteIds) set.Add(id);
                foreach (var id in PurchasedIds) set.Add(id);
                foreach (var id in OwnedIds) set.Add(id);
                return set;
            }
        }
    }
}
