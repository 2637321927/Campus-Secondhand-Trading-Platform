import request from '../http'
import type {
    CreateReviewDto,
    ReputationDetailDto,
    ReputationSummaryDto,
    ReviewDto,
    ReplyReviewDto
} from '../../types/api/review'

// ===== 模块10：评价与信誉 =====

/**
 * 对已完成订单进行评价
 */
export function createReview(
    orderId: number,
    data: CreateReviewDto
) {
    return request.post<ReviewDto>(
        `/api/orders/${orderId}/reviews`,
        data
    )
}

/**
 * 查看某订单评价
 */
export function getOrderReview(orderId: number) {
    return request.get<ReviewDto>(
        `/api/orders/${orderId}/reviews`
    )
}

/**
 * 查看某商品相关评价
 */
export function getProductReviews(productId: number) {
    return request.get<ReviewDto[]>(
        `/api/products/${productId}/reviews`
    )
}

/**
 * 查看某用户收到的评价
 */
export function getUserReceivedReviews(userId: number) {
    return request.get<ReviewDto[]>(
        `/api/users/${userId}/reviews/received`
    )
}

/**
 * 查看某用户发出的评价
 */
export function getUserGivenReviews(userId: number) {
    return request.get<ReviewDto[]>(
        `/api/users/${userId}/reviews/given`
    )
}

/**
 * 被评价方回复评价
 */
export function replyReview(
    reviewId: number,
    data: ReplyReviewDto
) {
    return request.post<ReviewDto>(
        `/api/reviews/${reviewId}/reply`,
        data
    )
}

/**
 * 删除评价
 */
export function deleteReview(reviewId: number) {
    return request.delete<void>(
        `/api/reviews/${reviewId}`
    )
}

// ===== 信誉相关接口 =====

/**
 * 获取用户信誉概览
 */
export function getReputationSummary(userId: number) {
    return request.get<ReputationSummaryDto>(
        `/api/users/${userId}/reputation/summary`
    )
}

/**
 * 获取用户信誉明细
 */
export function getReputationDetail(userId: number) {
    return request.get<ReputationDetailDto>(
        `/api/users/${userId}/reputation/detail`
    )
}
