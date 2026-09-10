import request from '../http'
import type { ProductCardDto } from '../../types/api/product'

/**
 * 首页"为你推荐"：登录按兴趣，未登录按热度兜底。
 */
export function getRecommendedProducts(count = 8) {
    return request.get<ProductCardDto[]>(
        '/api/recommend',
        { params: { count } }
    )
}

/**
 * 商品详情页"猜你想看"：同分类 + 用户偏好，返回 count 个。
 */
export function getRelatedProducts(productId: number, count = 4) {
    return request.get<ProductCardDto[]>(
        `/api/recommend/related/${productId}`,
        { params: { count } }
    )
}
