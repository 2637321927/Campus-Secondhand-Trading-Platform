/**
 * 创建评价请求
 */
export interface CreateReviewDto {
    rating: number
    info?: string | null
    imageIds?: number[] | null
}

/**
 * 评价图片
 */
export interface ReviewImageDto {
    imgFileId: number
    imgIndex: number
}

/**
 * 评价响应
 */
export interface ReviewDto {
    reviewId: number
    rating: number
    info: string | null
    reviewTime: string
    purchaseId: number

    replyInfo: string | null
    replyTime: string | null

    reviewerId: number
    reviewerName: string | null

    revieweeId: number
    revieweeName: string | null

    productId: number
    productName: string | null

    isHidden: number

    images: ReviewImageDto[]
}

/**
 * 评价回复请求
 */
export interface ReplyReviewDto {
    replyInfo: string
}

/**
 * 用户信誉概览
 */
export interface ReputationSummaryDto {
    userId: number
    userName: string
    credit: number
    totalReviews: number
    goodReviews: number
    neutralReviews: number
    badReviews: number
    goodRate: number
    averageRating: number
    completedSales: number
    completedPurchases: number
}

/**
 * 用户信誉明细
 */
export interface ReputationDetailDto {
    userId: number
    userName: string
    credit: number
    totalReviews: number
    goodReviews: number
    neutralReviews: number
    badReviews: number
    goodRate: number
    averageRating: number
    completedSales: number
    completedPurchases: number
    recentRatingDistribution: Record<number, number>
    hasPendingViolation: boolean
    totalWorkOrders: number
    completedWorkOrders: number
}
