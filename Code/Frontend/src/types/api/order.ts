/**
 * 订单状态（后端使用字符串：pending/paid/shipping/success/cancel/refund）
 */
export type OrderStatus =
    | 'pending'
    | 'paid'
    | 'shipping'
    | 'success'
    | 'cancel'
    | 'refund'

/**
 * 发货方式（后端枚举：0=包邮 1=按距离 2=固定邮费 3=无需邮寄）
 */
export type ShippingType = 0 | 1 | 2 | 3

/**
 * 创建订单请求
 */
export interface CreateOrderDto {
    productId: number
    addressId: number
    shippingMethod?: string | null
    note?: string | null
}

/**
 * 订单详情响应
 */
export interface OrderDto {
    purchaseId: number
    status: string
    createTime: string
    cancelTime: string | null
    payTime: string | null
    shippingTime: string | null
    deliveryTime: string | null
    completeTime: string | null
    shippingFees: number
    responsibleForShip: number
    shippingMethod: string | null
    shippingAddress: string | null
    receivingAddress: string | null
    trackingNumber: string | null

    buyerId: number
    buyerName: string | null
    productId: number
    productName: string | null
    productPrice: number
    productCoverImageId: number | null
    addressId: number
    addressDetail: string | null

    reviewId: number | null
    rating: number | null
}

/**
 * 订单列表项（简化响应，用于列表展示）
 */
export interface OrderListItemDto {
    purchaseId: number
    status: string
    createTime: string
    payTime: string | null
    completeTime: string | null
    shippingFees: number

    productId: number
    productName: string | null
    productPrice: number
    productCoverImageId: number | null

    buyerId: number
    buyerName: string | null
    sellerId: number
    sellerName: string | null

    hasReview: boolean
}

/**
 * 购买可行性检查响应
 */
export interface PurchaseCheckDto {
    canPurchase: boolean
    reason: string | null
    productStatus: string
    isOwnProduct: boolean
    shippingType: number
}

/**
 * 修改发货信息请求
 */
export interface UpdateShippingDto {
    shippingMethod?: string | null
    shippingAddress?: string | null
    receivingAddress?: string | null
    shippingFees?: number | null
}

/**
 * 确认发货请求
 */
export interface ShipOrderDto {
    trackingNumber?: string | null
}

/**
 * 订单状态流转记录
 */
export interface OrderTimelineDto {
    timelineId: number
    oldStatus: string | null
    newStatus: string
    changeTime: string
    operatorId: number
    note: string | null
}
