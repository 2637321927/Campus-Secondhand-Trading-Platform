import request from '../http'
import type { ProductCardDto } from '../../types/api/product'

import type {
    BatchDeleteCollectionsResponse,
    CollectionCountResponse,
    CollectionStatusDto,
} from '../../types/api/collection'

export function getCollectionStatus(productId: number) {
    return request.get<CollectionStatusDto>(
        `/api/collections/${productId}`
    )
}

/**
 * 收藏或取消收藏指定商品。
 *
 * 后端使用 Toggle 逻辑：
 * 未收藏 → 收藏
 * 已收藏 → 取消收藏
 */
export function toggleCollection(productId: number) {
    return request.post<CollectionStatusDto>(
        `/api/collections/${productId}`
    )
}

/**
 * 获取当前用户的全部收藏商品。
 */
export function getCollections() {
    return request.get<ProductCardDto[]>(
        '/api/collections'
    )
}

/**
 * 获取当前用户的收藏总数。
 */
export function getCollectionCount() {
    return request.get<CollectionCountResponse>(
        '/api/collections/count'
    )
}

/**
 * 在当前用户的收藏列表中搜索商品。
 */
export function searchCollections(
    keyword: string
) {
    return request.get<ProductCardDto[]>(
        '/api/collections/search',
        {
            params: {
                keyword
            }
        }
    )
}

/**
 * 批量取消收藏。
 *
 * 请求体示例：
 * [1, 3, 5]
 */
export function batchDeleteCollections(
    productIds: number[]
) {
    return request.delete<BatchDeleteCollectionsResponse>(
        '/api/collections',
        {
            data: productIds
        }
    )
}