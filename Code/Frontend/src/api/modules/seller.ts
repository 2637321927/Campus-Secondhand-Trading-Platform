import request from '../http'
import type {
    SellerProductQuery,
    SellerProductListDto,
    SellerProductDetailDto,
    SellerProductStatsDto,
    SellerProductCommentDto,
    ReplySellerCommentRequest
} from '../../types/api/seller'

/**
 * 获取当前用户发布的商品列表。
 *
 * 支持按关键词和商品状态筛选。
 */
export function getSellerProducts(
    query?: SellerProductQuery
) {
    return request.get<SellerProductListDto>(
        '/api/seller/products',
        {
            params: query
        }
    )
}

/**
 * 获取卖家视角的商品详情。
 *
 * 与公开商品详情不同，该接口可以返回草稿、
 * 下架商品以及其他卖家管理字段。
 */
export function getSellerProductDetail(
    productId: number
) {
    return request.get<SellerProductDetailDto>(
        `/api/seller/products/${productId}`
    )
}

/**
 * 获取当前用户某件商品的管理统计数据。
 */
export function getSellerProductStats(
    productId: number
) {
    return request.get<SellerProductStatsDto>(
        `/api/seller/products/${productId}/stats`
    )
}

/**
 * 获取当前用户某件商品下的留言列表。
 */
export function getSellerProductComments(
    productId: number
) {
    return request.get<SellerProductCommentDto[]>(
        `/api/seller/products/${productId}/comments`
    )
}

/**
 * 卖家回复指定商品留言。
 *
 * 当前接口定义没有明确响应体，因此成功后
 * 页面应重新请求留言列表。
 */
export function replySellerComment(
    productId: number,
    commentId: number,
    data: ReplySellerCommentRequest
) {
    return request.post<void>(
        `/api/seller/products/${productId}/comments/${commentId}/reply`,
        data
    )
}

/**
 * 将商品标记为已售出。
 *
 * 当前接口定义没有明确响应体，操作成功后
 * 页面应重新加载商品详情或商品列表。
 */
export function markSellerProductSold(
    productId: number
) {
    return request.patch<void>(
        `/api/seller/products/${productId}/mark-sold`
    )
}

/**
 * 将已下架或未售商品重新上架。
 *
 * 当前接口定义没有明确响应体，操作成功后
 * 页面应重新加载商品详情或商品列表。
 */
export function relistSellerProduct(
    productId: number
) {
    return request.patch<void>(
        `/api/seller/products/${productId}/relist`
    )
}