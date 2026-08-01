import request from '../http'
import type { 
    ProductCommentDto,
    CreateProductCommentRequest 
} from '../../types/api/comment'

export function getProductComments(productId: number) {
    return request.get<ProductCommentDto[]>(
        `/api/products/${productId}/comments`
    )
}

export function createProductComment(
    productId: number,
    data: CreateProductCommentRequest
) {
    return request.post(
        `/api/products/${productId}/comments`,
        data
    )
}

export function deleteProductComment(
    productId: number,
    commentId: number
) {
    return request.delete<void>(
        `/api/products/${productId}/comments/${commentId}`
    )
}