export interface ProductCommentDto {
    commentId: number
    content: string
    userId: number
    userName: string
    createTime: string
    responseToId: number | null
    replies: ProductCommentDto[]
}

export interface CreateProductCommentRequest {
    content: string
    responseToId?: number | null
}
