using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProductService : IProductService
{
    
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IProductViewRepository _productViewRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IProdImageService _prodImage;
    private readonly ISearchService _searchService;
    private readonly IBaseUserRepository _baseUserRepo;

    public ProductService(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        IProductViewRepository productViewRepo,
        IPurchaseRepository purchaseRepo,
        IProdImageService prodImageService,
        ISearchService searchService,
        IBaseUserRepository baseUserRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _productViewRepo = productViewRepo;
        _purchaseRepo = purchaseRepo;
        _prodImage = prodImageService;
        _searchService = searchService;
        _baseUserRepo = baseUserRepo;
    }

    public async Task<ProductDto?> GetByIdAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;
        if (userId != -1 && product.UserId != userId) await RecordViewAsync(productId, userId);
        var viewCount = await _productViewRepo.GetViewCountAsync(productId);
        return ToDto(product, viewCount);

    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        return await GetByStatusesAsync(new[] { ProductStatus.Available });
    }

    public async Task<List<ProductDto>> GetByStatusesAsync(IEnumerable<ProductStatus> statuses)
    {
        var products = await _productRepo.GetByStatusesAsync(statuses);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(product => product.ProductId));

        return products
            .OrderByDescending(product => product.ReleaseDate)
            .Select(product => ToDto(
                product,
                viewCounts.GetValueOrDefault(product.ProductId, 0)))
            .ToList();
    }

    public async Task RecordViewAsync(long productId, int userId)
    {
        await _productViewRepo.AddAsync(new ProductView
        {
            ProductId = productId,
            UserId = userId,
            ViewTime = DateTime.Now
        });
        await _productViewRepo.SaveAsync();
    }

    public async Task<List<long>> GetProductIdsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetByUserIdAsync(userId);

        return products
            .OrderByDescending(product => product.ReleaseDate)
            .Select(product => product.ProductId)
            .ToList();
    }

    public async Task<List<ProductDto>> GetProductsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetByUserIdAsync(userId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(p => p.ProductId));

        return products
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => ToDto(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    public async Task<List<ProductDto>> GetSoldProductsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetSoldByUserIdAsync(userId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(p => p.ProductId));

        return products
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => ToDto(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    public async Task<ProductDto?> CreateAsync(int userId, CreateProductDto dto)
    {
        var user = await _baseUserRepo.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("用户不存在");

        if (user.AccountStatus == AccountStatus.Banned ||
            user.AccountStatus == AccountStatus.PublishRestricted)
            throw new UnauthorizedAccessException("当前账号状态不允许发布商品");

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new ArgumentException("分类不存在");
        if (category.ParentId == null)
            throw new ArgumentException("商品必须发布到具体小分类，不能直接选择一级大分类");

        var product = new Product
        {

            Name = dto.Name,
            Price = dto.Price,
            Info = dto.Info,
            // 新发布的商品必须先由管理员审核，审核通过后才会公开展示。
            Status = ProductStatus.PendingReview,
            UserId = userId,
            ReleaseDate = DateTime.Now,
            CategoryId = dto.CategoryId,
            ShippingType = dto.ShippingType,
            ShippingFee = dto.ShippingFee,
            AllowPickup = dto.AllowPickup

        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveAsync();

        await _searchService.NotifyProductCreatedAsync(product.ProductId);

        if (dto.Images != null && dto.Images.Count > 0)
        {
            await _prodImage.UploadProductImagesAsync(dto.Images, product.ProductId, userId);
        }

        return ToDto(product);
        
    }

    public async Task<ProductDto?> UpdateAsync(long productId, int userId, UpdateProductDto dto)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.UserId != userId)
        {

            throw new UnauthorizedAccessException("You do not have permission to update this product.");

        }

        var targetCategory = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new ArgumentException("分类不存在");
        if (targetCategory.ParentId == null)
            throw new ArgumentException("商品分类必须是小分类，不能直接选择一级大分类");

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Info = dto.Info;
        product.CategoryId = dto.CategoryId;
        product.ShippingType = dto.ShippingType;
        product.ShippingFee = dto.ShippingFee;
        product.AllowPickup = dto.AllowPickup;

        // 驳回商品经卖家修改后重新进入审核队列，避免商品永久停留在“已驳回”状态。
        // 其他状态的编辑不改变既有的上架或交易状态。
        if (product.Status == ProductStatus.Rejected)
        {
            product.Status = ProductStatus.PendingReview;
            product.RejectReason = null;
            product.ReviewedByAdminId = null;
            product.ReviewedAt = null;
        }

        if (dto.toRemoveImageIds != null && dto.toRemoveImageIds.Count > 0)
        {

            await _prodImage.DeleteProductImagesAsync(dto.toRemoveImageIds);

            foreach (var imgId in dto.toRemoveImageIds)
            {
                var img = product.Images.FirstOrDefault(i => i.ImgFileId == imgId);
                if (img != null)
                    product.Images.Remove(img);
            }

        }

        if (dto.newImages != null && dto.newImages.Count > 0)
        {

            await _prodImage.UploadProductImagesAsync(dto.newImages, productId, product.UserId);

        }

        var index = 0;
        foreach (var img in product.Images.OrderBy(i => i.ImgIndex))
        {
            img.ImgIndex = index++;
        }

        _productRepo.Update(product);
        await _productRepo.SaveAsync();

        return ToDto(product);

    }

    public async Task<ProductDto?> UpdateStatusAsync(long productId, int userId, ProductStatus target)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to update this product.");

        // 用户端只允许在 在售/已售/已下架 之间流转，待审核/驳回/交易中由审核或订单流程管理
        if (target is not (ProductStatus.Available or ProductStatus.Sold or ProductStatus.Removed))
            throw new ArgumentException("不允许设置该状态");

        var current = product.Status;
        var allowed = (current, target) switch
        {
            (ProductStatus.Available, ProductStatus.Sold) => true,
            (ProductStatus.Available, ProductStatus.Removed) => true,
            (ProductStatus.Sold, ProductStatus.Available) => true,
            (ProductStatus.Removed, ProductStatus.Available) => true,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException("当前状态不允许该操作");

        // 只有进行中的订单才会阻止卖家手工改变售卖状态。
        // 已完成订单是历史记录，不应阻止商品开启新一轮售卖。
        if (target == ProductStatus.Sold ||
            (current == ProductStatus.Sold && target == ProductStatus.Available))
        {
            var orders = await _purchaseRepo.GetByProductIdAsync(productId);
            var hasActiveOrder = orders.Any(o =>
                o.Status != "cancel" && o.Status != "success");

            if (hasActiveOrder)
                throw new InvalidOperationException("该商品存在进行中的订单，不能修改售卖状态");
        }

        // 重新上架前检查账号状态
        if (target == ProductStatus.Available && current != ProductStatus.Available)
        {
            var user = await _baseUserRepo.GetByIdAsync(userId);
            if (user != null &&
                (user.AccountStatus == AccountStatus.Banned ||
                 user.AccountStatus == AccountStatus.PublishRestricted))
                throw new UnauthorizedAccessException("当前账号状态不允许发布商品");
        }

        product.Status = target;
        _productRepo.Update(product);
        await _productRepo.SaveAsync();

        return ToDto(product);
    }

    public async Task<List<ProductCardDto>> QueryProductCardsAsync(
        ProductFilter? filter,
        ProductOrder? order,
        int count)
    {

        IQueryable<Product> query = _productRepo.Query()
            .Where(p => p.Status == ProductStatus.Available);

        if (filter != null)
            query = filter(query);

        if (order != null)
            query = order(query);

        var products = await query
            .Take(count)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        var ids = products.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);

        return products.Select(p =>
            ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0))
        ).ToList();

    }

    public async Task<bool> DeleteAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return false;

        if (product.UserId != userId)
        {
            
            throw new UnauthorizedAccessException("You do not have permission to delete this product.");

        }

        // purchase.product_id 使用 Restrict 外键。先检查关联订单，避免先删图片
        // 再因商品删除失败，造成“商品还在但图片没了”的半删除状态。
        var relatedOrders = await _purchaseRepo.GetByProductIdAsync(productId);
        if (relatedOrders.Count > 0)
            throw new InvalidOperationException("商品存在关联订单，无法删除；如需停止展示，请将商品下架");

        var imageIds = product.Images.Select(i => i.ImgFileId).ToList();
        // 先删除商品并让数据库级联删除图片关联记录；只有商品删除成功后才清理文件，
        // 这样外键冲突不会留下“商品还在但图片已被清掉”的不一致状态。
        _productRepo.Delete(product);
        try
        {
            await _productRepo.SaveAsync();
        }
        catch (DbUpdateException)
        {
            // 订单可能在预检查后并发创建，保持商品和图片完整并返回可理解的业务错误。
            throw new InvalidOperationException("商品存在关联订单，无法删除；如需停止展示，请将商品下架");
        }

        if (imageIds.Count > 0)
            await _prodImage.DeleteProductImagesAsync(imageIds);
        return true;

    }

    public static readonly ProductOrder Latest = q =>
        q.OrderByDescending(p => p.ReleaseDate);

    public static readonly ProductOrder Hottest = q =>
        q.OrderByDescending(p => p.Views.Count)
         .ThenByDescending(p => p.ReleaseDate);

    private static ProductDto ToDto(Product p, int viewCount = 0) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        Info = p.Info,
        Status = p.Status,
        RejectReason = p.RejectReason,
        ReleaseDate = p.ReleaseDate,
        UserId = p.UserId,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.CategoryName,
        ViewCount = viewCount,
        ShippingType = p.ShippingType,
        ShippingFee = p.ShippingFee,
        AllowPickup = p.AllowPickup,
        Images = p.Images?.Select(i => new ProductImageDto
        {
            ImgFileId = i.ImgFileId,
            ImgIndex = i.ImgIndex
        }).ToList() ?? new()
    };

    public static ProductCardDto ToProductCard(Product p, int viewCount = 0) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        CoverImageFileId = p.Images?
            .OrderBy(i => i.ImgIndex)
            .FirstOrDefault()?.ImgFileId,
        SellerName = p.Seller?.UserName ?? "",
        ReleaseDate = p.ReleaseDate,
        ViewCount = viewCount
    };

}
