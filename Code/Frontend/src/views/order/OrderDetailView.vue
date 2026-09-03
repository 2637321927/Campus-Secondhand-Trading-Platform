<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
    cancelOrder,
    completeOrder,
    getOrder,
    getOrderTimeline,
    receiveOrder,
    sellerConfirmOrder,
    sellerRejectOrder,
    shipOrder,
    updateShipping
} from '../../api/modules/order'
import { getProductImages } from '../../api/modules/product'
import { getOrderReview } from '../../api/modules/review'
import type {
    OrderDto,
    OrderTimelineDto
} from '../../types/api/order'
import type { ReviewDto } from '../../types/api/review'
import { getApiErrorMessage } from '../../utils/error'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const loading = ref(false)
const errorMessage = ref('')
const order = ref<OrderDto | null>(null)
const timeline = ref<OrderTimelineDto[]>([])
const review = ref<ReviewDto | null>(null)
const productImageUrl = ref('')
const operating = ref(false)

const orderId = computed(() => Number(route.params.orderId))

const isBuyer = computed(() => {
    if (!order.value || !authStore.currentUser) return false
    return order.value.buyerId === authStore.currentUser.userId
})

const isSeller = computed(() => {
    if (!order.value || !authStore.currentUser) return false
    return order.value.productCoverImageId !== null && !isBuyer.value
})

const statusTextMap: Record<string, string> = {
    pending: '待付款',
    paid: '已付款',
    shipping: '已发货',
    success: '已完成',
    cancel: '已取消',
    refund: '退款中'
}

const statusTagType: Record<string, string> = {
    pending: 'warning',
    paid: 'primary',
    shipping: 'primary',
    success: 'success',
    cancel: 'info',
    refund: 'danger'
}

const timelineStatusText = (status: string | null): string => {
    if (!status) return '初始状态'
    return statusTextMap[status] ?? status
}

function formatDateTime(value: string | null): string {
    if (!value) return ''
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) return ''
    return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    })
}

async function loadProductImage(fileId: number | null): Promise<void> {
    if (!fileId || fileId <= 0) return

    try {
        const response = await getProductImages([fileId])
        const image = response.data?.[0]
        if (image?.content) {
            const binary = window.atob(image.content)
            const bytes = new Uint8Array(binary.length)
            for (let i = 0; i < binary.length; i++) {
                bytes[i] = binary.charCodeAt(i)
            }
            const blob = new Blob([bytes], { type: image.mimeType || 'application/octet-stream' })
            productImageUrl.value = URL.createObjectURL(blob)
        }
    } catch (error) {
        console.error('商品图片加载失败：', error)
    }
}

async function loadOrder(): Promise<void> {
    const id = orderId.value
    if (!id || Number.isNaN(id)) {
        errorMessage.value = '订单ID无效'
        return
    }

    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getOrder(id)
        order.value = response.data

        await loadProductImage(order.value?.productCoverImageId ?? null)

        // 加载时间线
        try {
            const timelineResponse = await getOrderTimeline(id)
            timeline.value = timelineResponse.data ?? []
        } catch {
            timeline.value = []
        }

        // 加载评价
        try {
            const reviewResponse = await getOrderReview(id)
            review.value = reviewResponse.data
        } catch {
            review.value = null
        }
    } catch (error) {
        order.value = null
        errorMessage.value = getApiErrorMessage(error, '订单详情加载失败，请稍后重试')
        console.error('订单详情加载失败：', error)
    } finally {
        loading.value = false
    }
}

async function handleCancelOrder(): Promise<void> {
    if (!order.value) return

    try {
        await ElMessageBox.confirm(
            '确定取消此订单吗？取消后无法恢复。',
            '取消订单',
            { type: 'warning', confirmButtonText: '确定取消', cancelButtonText: '再想想' }
        )
    } catch {
        return
    }

    operating.value = true
    try {
        await cancelOrder(order.value.purchaseId)
        ElMessage.success('订单已取消')
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '取消订单失败'))
        console.error('取消订单失败：', error)
    } finally {
        operating.value = false
    }
}

async function handleSellerConfirm(): Promise<void> {
    if (!order.value) return

    operating.value = true
    try {
        await sellerConfirmOrder(order.value.purchaseId)
        ElMessage.success('订单已确认')
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '确认订单失败'))
        console.error('确认订单失败：', error)
    } finally {
        operating.value = false
    }
}

async function handleSellerReject(): Promise<void> {
    if (!order.value) return

    try {
        await ElMessageBox.confirm(
            '确定拒绝此订单吗？',
            '拒绝订单',
            { type: 'warning', confirmButtonText: '确定拒绝', cancelButtonText: '再想想' }
        )
    } catch {
        return
    }

    operating.value = true
    try {
        await sellerRejectOrder(order.value.purchaseId)
        ElMessage.success('订单已拒绝')
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '拒绝订单失败'))
        console.error('拒绝订单失败：', error)
    } finally {
        operating.value = false
    }
}

// 卖家发货弹窗
const shipDialogVisible = ref(false)
const shipTrackingNumber = ref('')

function openShipDialog(): void {
    shipTrackingNumber.value = order.value?.trackingNumber ?? ''
    shipDialogVisible.value = true
}

async function handleShipOrder(): Promise<void> {
    if (!order.value) return

    operating.value = true
    try {
        await shipOrder(order.value.purchaseId, {
            trackingNumber: shipTrackingNumber.value || null
        })
        ElMessage.success('已确认发货')
        shipDialogVisible.value = false
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '发货失败'))
        console.error('发货失败：', error)
    } finally {
        operating.value = false
    }
}

async function handleReceiveOrder(): Promise<void> {
    if (!order.value) return

    try {
        await ElMessageBox.confirm(
            '确认已收到商品吗？',
            '确认收货',
            { type: 'warning', confirmButtonText: '确认收货', cancelButtonText: '再想想' }
        )
    } catch {
        return
    }

    operating.value = true
    try {
        await receiveOrder(order.value.purchaseId)
        ElMessage.success('已确认收货')
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '确认收货失败'))
        console.error('确认收货失败：', error)
    } finally {
        operating.value = false
    }
}

async function handleCompleteOrder(): Promise<void> {
    if (!order.value) return

    try {
        await ElMessageBox.confirm(
            '确定完成此订单吗？',
            '完成订单',
            { type: 'warning', confirmButtonText: '确定', cancelButtonText: '再想想' }
        )
    } catch {
        return
    }

    operating.value = true
    try {
        await completeOrder(order.value.purchaseId)
        ElMessage.success('订单已完成')
        await loadOrder()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '完成订单失败'))
        console.error('完成订单失败：', error)
    } finally {
        operating.value = false
    }
}

function goToProduct(): void {
    if (!order.value) return
    router.push({
        name: 'product-detail',
        params: { productId: order.value.productId }
    })
}

function goToReviewCreate(): void {
    if (!order.value) return
    router.push({
        name: 'review-create',
        params: { orderId: order.value.purchaseId }
    })
}

onMounted(() => {
    void loadOrder()
})
</script>

<template>
    <main class="order-detail-page">
        <div class="order-detail-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 加载状态 -->
            <section v-if="loading" class="detail-panel">
                <el-skeleton :rows="6" animated />
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="订单加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadOrder">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 正常内容 -->
            <template v-else-if="order">
                <!-- 页面头部 -->
                <header class="page-header">
                    <p class="page-eyebrow">ORDER DETAIL</p>
                    <div class="header-row">
                        <h1>订单详情</h1>
                        <el-tag
                            :type="statusTagType[order.status] ?? 'info'"
                            effect="plain"
                            size="large"
                        >
                            {{ statusTextMap[order.status] ?? order.status }}
                        </el-tag>
                    </div>
                </header>

                <!-- 商品信息 -->
                <section class="detail-panel">
                    <h2 class="panel-title">商品信息</h2>
                    <div class="product-row" @click="goToProduct">
                        <el-image
                            class="product-image"
                            :src="productImageUrl"
                            fit="cover"
                        >
                            <template #error>
                                <div class="image-placeholder">商品图</div>
                            </template>
                        </el-image>
                        <div class="product-info">
                            <h3>{{ order.productName ?? '未知商品' }}</h3>
                            <p class="product-price">¥{{ order.productPrice.toFixed(2) }}</p>
                        </div>
                    </div>
                </section>

                <!-- 订单信息 -->
                <section class="detail-panel">
                    <h2 class="panel-title">订单信息</h2>
                    <div class="info-grid">
                        <div class="info-item">
                            <span class="info-label">订单编号</span>
                            <span class="info-value">{{ order.purchaseId }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">创建时间</span>
                            <span class="info-value">{{ formatDateTime(order.createTime) }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">买家</span>
                            <span class="info-value">{{ order.buyerName ?? '未知' }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">商品价格</span>
                            <span class="info-value">¥{{ order.productPrice.toFixed(2) }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">运费</span>
                            <span class="info-value">¥{{ order.shippingFees.toFixed(2) }}</span>
                        </div>
                        <div class="info-item" v-if="order.shippingMethod">
                            <span class="info-label">发货方式</span>
                            <span class="info-value">{{ order.shippingMethod }}</span>
                        </div>
                        <div class="info-item" v-if="order.trackingNumber">
                            <span class="info-label">物流单号</span>
                            <span class="info-value">{{ order.trackingNumber }}</span>
                        </div>
                        <div class="info-item" v-if="order.addressDetail">
                            <span class="info-label">收货地址</span>
                            <span class="info-value">{{ order.addressDetail }}</span>
                        </div>
                        <div class="info-item" v-if="order.receivingAddress">
                            <span class="info-label">收件地址</span>
                            <span class="info-value">{{ order.receivingAddress }}</span>
                        </div>
                    </div>
                </section>

                <!-- 时间信息 -->
                <section class="detail-panel">
                    <h2 class="panel-title">时间信息</h2>
                    <div class="info-grid">
                        <div class="info-item" v-if="order.payTime">
                            <span class="info-label">付款时间</span>
                            <span class="info-value">{{ formatDateTime(order.payTime) }}</span>
                        </div>
                        <div class="info-item" v-if="order.shippingTime">
                            <span class="info-label">发货时间</span>
                            <span class="info-value">{{ formatDateTime(order.shippingTime) }}</span>
                        </div>
                        <div class="info-item" v-if="order.deliveryTime">
                            <span class="info-label">送达时间</span>
                            <span class="info-value">{{ formatDateTime(order.deliveryTime) }}</span>
                        </div>
                        <div class="info-item" v-if="order.completeTime">
                            <span class="info-label">完成时间</span>
                            <span class="info-value">{{ formatDateTime(order.completeTime) }}</span>
                        </div>
                        <div class="info-item" v-if="order.cancelTime">
                            <span class="info-label">取消时间</span>
                            <span class="info-value">{{ formatDateTime(order.cancelTime) }}</span>
                        </div>
                    </div>
                </section>

                <!-- 评价信息 -->
                <section class="detail-panel" v-if="review">
                    <h2 class="panel-title">订单评价</h2>
                    <div class="review-section">
                        <el-rate :model-value="review.rating" disabled />
                        <p class="review-info" v-if="review.info">{{ review.info }}</p>
                        <p class="review-time">评价时间：{{ formatDateTime(review.reviewTime) }}</p>
                        <div v-if="review.replyInfo" class="review-reply">
                            <span class="reply-label">回复：</span>
                            <span>{{ review.replyInfo }}</span>
                        </div>
                    </div>
                </section>

                <!-- 操作按钮 -->
                <section class="detail-panel" v-if="order.status !== 'cancel' && order.status !== 'success'">
                    <h2 class="panel-title">操作</h2>
                    <div class="action-buttons">
                        <!-- 买家操作 -->
                        <template v-if="isBuyer">
                            <el-button
                                v-if="order.status === 'pending'"
                                type="danger"
                                :loading="operating"
                                @click="handleCancelOrder"
                            >
                                取消订单
                            </el-button>
                            <el-button
                                v-if="order.status === 'shipping'"
                                type="primary"
                                :loading="operating"
                                @click="handleReceiveOrder"
                            >
                                确认收货
                            </el-button>
                        </template>

                        <!-- 卖家操作 -->
                        <template v-if="!isBuyer">
                            <el-button
                                v-if="order.status === 'paid'"
                                type="primary"
                                :loading="operating"
                                @click="handleSellerConfirm"
                            >
                                确认订单
                            </el-button>
                            <el-button
                                v-if="order.status === 'paid'"
                                type="danger"
                                :loading="operating"
                                @click="handleSellerReject"
                            >
                                拒绝订单
                            </el-button>
                            <el-button
                                v-if="order.status === 'paid' || order.status === 'shipping'"
                                type="primary"
                                :loading="operating"
                                @click="openShipDialog"
                            >
                                确认发货
                            </el-button>
                        </template>

                        <!-- 完成订单（买卖双方都可） -->
                        <el-button
                            v-if="order.status === 'shipping'"
                            type="success"
                            :loading="operating"
                            @click="handleCompleteOrder"
                        >
                            完成订单
                        </el-button>
                    </div>
                </section>

                <!-- 评价按钮 -->
                <section class="detail-panel" v-if="order.status === 'success' && !review && isBuyer">
                    <el-button
                        type="primary"
                        @click="goToReviewCreate"
                    >
                        去评价
                    </el-button>
                </section>

                <!-- 状态流转记录 -->
                <section class="detail-panel" v-if="timeline.length > 0">
                    <h2 class="panel-title">状态流转</h2>
                    <el-timeline>
                        <el-timeline-item
                            v-for="item in timeline"
                            :key="item.timelineId"
                            :timestamp="formatDateTime(item.changeTime)"
                        >
                            <p class="timeline-status">
                                {{ timelineStatusText(item.oldStatus) }} → {{ timelineStatusText(item.newStatus) }}
                            </p>
                            <p class="timeline-note" v-if="item.note">{{ item.note }}</p>
                        </el-timeline-item>
                    </el-timeline>
                </section>
            </template>
        </div>

        <!-- 发货弹窗 -->
        <el-dialog
            v-model="shipDialogVisible"
            title="确认发货"
            width="440px"
        >
            <el-form>
                <el-form-item label="物流单号">
                    <el-input
                        v-model="shipTrackingNumber"
                        placeholder="请输入物流单号（选填）"
                        clearable
                    />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="shipDialogVisible = false">取消</el-button>
                <el-button type="primary" :loading="operating" @click="handleShipOrder">
                    确认发货
                </el-button>
            </template>
        </el-dialog>
    </main>
</template>

<style scoped>
.order-detail-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.order-detail-container {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
}

.page-header {
    margin: 16px 0 24px;
}

.page-eyebrow {
    margin: 0 0 8px;
    color: #3e9b79;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 1.6px;
}

.header-row {
    display: flex;
    align-items: center;
    gap: 16px;
}

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 28px;
    line-height: 1.25;
}

.detail-panel {
    margin-bottom: 20px;
    padding: 24px 28px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.panel-title {
    margin: 0 0 16px;
    color: #1e2a26;
    font-size: 18px;
}

.product-row {
    display: flex;
    gap: 16px;
    align-items: center;
    cursor: pointer;
}

.product-image {
    width: 72px;
    height: 72px;
    border-radius: 12px;
    overflow: hidden;
    flex-shrink: 0;
    background: #f5f7f6;
}

.image-placeholder {
    display: flex;
    width: 100%;
    height: 100%;
    align-items: center;
    justify-content: center;
    color: #6c7a74;
    font-size: 12px;
    background: #f5f7f6;
}

.product-info h3 {
    margin: 0 0 6px;
    color: #1e2a26;
    font-size: 15px;
    font-weight: 600;
}

.product-price {
    margin: 0;
    color: #24735b;
    font-size: 16px;
    font-weight: 700;
}

.info-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
}

.info-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.info-label {
    color: #6c7a74;
    font-size: 13px;
}

.info-value {
    color: #1e2a26;
    font-size: 14px;
}

.review-section {
    display: flex;
    flex-direction: column;
    gap: 10px;
}

.review-info {
    margin: 0;
    color: #1e2a26;
    font-size: 14px;
    line-height: 1.7;
}

.review-time {
    margin: 0;
    color: #6c7a74;
    font-size: 13px;
}

.review-reply {
    padding: 12px 16px;
    background: #f5f7f6;
    border-radius: 10px;
    font-size: 14px;
    line-height: 1.7;
}

.reply-label {
    color: #3e9b79;
    font-weight: 600;
}

.action-buttons {
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
}

.timeline-status {
    margin: 0;
    color: #1e2a26;
    font-size: 14px;
    font-weight: 600;
}

.timeline-note {
    margin: 4px 0 0;
    color: #6c7a74;
    font-size: 13px;
}

@media (max-width: 760px) {
    .info-grid {
        grid-template-columns: 1fr;
    }
}
</style>
