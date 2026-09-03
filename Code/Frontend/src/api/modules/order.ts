import request from '../http'
import type {
    CreateOrderDto,
    OrderDto,
    OrderListItemDto,
    OrderTimelineDto,
    PurchaseCheckDto,
    ShipOrderDto,
    UpdateShippingDto
} from '../../types/api/order'

// ===== 模块9：购买、订单与支付 =====

/**
 * 检查商品是否可购买
 */
export function purchaseCheck(productId: number) {
    return request.get<PurchaseCheckDto>(
        `/api/products/${productId}/purchase-check`
    )
}

/**
 * 创建购买订单
 */
export function createOrder(data: CreateOrderDto) {
    return request.post<OrderDto>(
        '/api/orders',
        data
    )
}

/**
 * 获取订单详情
 */
export function getOrder(orderId: number) {
    return request.get<OrderDto>(
        `/api/orders/${orderId}`
    )
}

/**
 * 获取当前用户购买订单列表
 */
export function getBuyingOrders() {
    return request.get<OrderListItemDto[]>(
        '/api/orders/me/buying'
    )
}

/**
 * 获取当前用户卖出订单列表
 */
export function getSellingOrders() {
    return request.get<OrderListItemDto[]>(
        '/api/orders/me/selling'
    )
}

/**
 * 取消订单（买家在待付款状态下取消）
 */
export function cancelOrder(orderId: number) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/cancel`
    )
}

/**
 * 卖家确认订单
 */
export function sellerConfirmOrder(orderId: number) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/seller-confirm`
    )
}

/**
 * 卖家拒绝订单
 */
export function sellerRejectOrder(orderId: number) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/seller-reject`
    )
}

/**
 * 设置或修改订单发货信息
 */
export function updateShipping(
    orderId: number,
    data: UpdateShippingDto
) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/shipping`,
        data
    )
}

/**
 * 卖家确认发货
 */
export function shipOrder(
    orderId: number,
    data: ShipOrderDto
) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/ship`,
        data
    )
}

/**
 * 买家确认收货
 */
export function receiveOrder(orderId: number) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/receive`
    )
}

/**
 * 完成订单
 */
export function completeOrder(orderId: number) {
    return request.patch<OrderDto>(
        `/api/orders/${orderId}/complete`
    )
}

/**
 * 获取订单状态流转记录
 */
export function getOrderTimeline(orderId: number) {
    return request.get<OrderTimelineDto[]>(
        `/api/orders/${orderId}/timeline`
    )
}

// ===== 支付相关接口 =====

/**
 * 获取可用支付方式
 */
export function getPaymentMethods() {
    return request.get<
        Array<{ value: string; label: string }>
    >('/api/payment-methods')
}

/**
 * 发起订单支付
 */
export function createPayment(
    data: {
        purchaseId: number
        paymentMethod: string
    }
) {
    return request.post<{
        paymentId: number
        status: string
        paymentMethod: string
        amount: number
        transactionId: string | null
        createTime: string
        payTime: string | null
        cancelTime: string | null
        purchaseId: number
    }>('/api/payments', data)
}

/**
 * 查询支付状态
 */
export function getPaymentStatus(paymentId: number) {
    return request.get<{
        paymentId: number
        status: string
        statusText: string
        amount: number
        createTime: string
        payTime: string | null
    }>(`/api/payments/${paymentId}/status`)
}

/**
 * 取消支付
 */
export function cancelPayment(paymentId: number) {
    return request.post(
        `/api/payments/${paymentId}/cancel`
    )
}
