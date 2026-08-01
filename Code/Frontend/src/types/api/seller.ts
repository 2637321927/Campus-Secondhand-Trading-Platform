import type {
    ProductDto,
    ProductStatus
} from './product'

import type {
    ProductCommentDto
} from './comment'

/**
 * “我的商品”列表查询条件。
 */
export interface SellerProductQuery {
    keyword?: string
    status?: ProductStatus
}

/**
 * 卖家商品列表。
 */
export type SellerProductListDto = ProductDto[]

/**
 * 卖家视角的商品详情。
 */
export type SellerProductDetailDto = ProductDto

/**
 * 当前用户自己商品的管理统计数据。
 */
export interface SellerProductStatsDto {
    viewCount: number
    collectionCount: number
    commentCount: number

    /**
     * 接口描述提到分享统计，但尚未明确是否一定返回。
     */
    shareCount?: number
}

/**
 * 卖家视角的商品留言列表。
 */
export type SellerProductCommentDto = ProductCommentDto

/**
 * 卖家回复商品留言的请求体。
 */
export interface ReplySellerCommentRequest {
    content: string
}