<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getApiErrorMessage } from '../../utils/error'
import { getSellingOrders } from '../../api/modules/order'
import { getProductImages } from '../../api/modules/product'
import type { OrderListItemDto } from '../../types/api/order'

const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const orderList = ref<OrderListItemDto[]>([])
const imageUrls = ref<Record<number, string>>({})

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

async function loadOrderImages(orders: OrderListItemDto[]): Promise<void> {
    const fileIds = orders
        .map(o => o.productCoverImageId)
        .filter((id): id is number => id !== null && id > 0)

    if (fileIds.length === 0) return

    try {
        const response = await getProductImages(fileIds)
        const nextUrls: Record<number, string> = {}

        for (const image of response.data ?? []) {
            if (image.fileId > 0 && image.content) {
                const binary = window.atob(image.content)
                const bytes = new Uint8Array(binary.length)
                for (let i = 0; i < binary.length; i++) {
                    bytes[i] = binary.charCodeAt(i)
                }
                const blob = new Blob([bytes], { type: image.mimeType || 'application/octet-stream' })
                nextUrls[image.fileId] = URL.createObjectURL(blob)
            }
        }

        imageUrls.value = nextUrls
    } catch (error) {
        console.error('订单商品图片加载失败：', error)
    }
}

async function loadOrders(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getSellingOrders()
        orderList.value = response.data ?? []
        await loadOrderImages(orderList.value)
    } catch (error) {
        orderList.value = []
        errorMessage.value = getApiErrorMessage(error, '卖出订单加载失败，请稍后重试')
        console.error('卖出订单加载失败：', error)
    } finally {
        loading.value = false
    }
}

function goToDetail(orderId: number): void {
    router.push({
        name: 'order-detail',
        params: { orderId }
    })
}

onMounted(() => {
    void loadOrders()
})
</script>

<template>
    <main class="order-list-page">
        <div class="order-list-container">
            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">MY SALES</p>
                <h1>我卖出的订单</h1>
                <p class="page-description">
                    管理你的卖出订单，确认、发货和处理交易。
                </p>
            </header>

            <!-- 加载状态 -->
            <section v-if="loading" class="order-panel">
                <div v-for="i in 4" :key="i" class="skeleton-card">
                    <el-skeleton :rows="3" animated />
                </div>
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="订单加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadOrders">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 空数据 -->
            <el-empty
                v-else-if="orderList.length === 0"
                description="暂无卖出订单"
            />

            <!-- 订单列表 -->
            <div v-else class="order-list">
                <div
                    v-for="order in orderList"
                    :key="order.purchaseId"
                    class="order-card"
                    @click="goToDetail(order.purchaseId)"
                >
                    <div class="order-card-left">
                        <el-image
                            class="order-product-image"
                            :src="imageUrls[order.productCoverImageId ?? 0] ?? ''"
                            fit="cover"
                        >
                            <template #error>
                                <div class="image-placeholder">商品图</div>
                            </template>
                        </el-image>
                    </div>

                    <div class="order-card-middle">
                        <h3 class="order-product-name">
                            {{ order.productName ?? '未知商品' }}
                        </h3>
                        <p class="order-meta">
                            订单号：{{ order.purchaseId }}
                        </p>
                        <p class="order-meta">
                            买家：{{ order.buyerName ?? '未知' }}
                        </p>
                        <p class="order-meta">
                            下单时间：{{ formatDateTime(order.createTime) }}
                        </p>
                    </div>

                    <div class="order-card-right">
                        <p class="order-price">
                            ¥{{ order.productPrice.toFixed(2) }}
                        </p>
                        <el-tag
                            :type="statusTagType[order.status] ?? 'info'"
                            effect="plain"
                        >
                            {{ statusTextMap[order.status] ?? order.status }}
                        </el-tag>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.order-list-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.order-list-container {
    width: 100%;
    max-width: 960px;
    margin: 0 auto;
}

.page-header {
    margin-bottom: 24px;
}

.page-eyebrow {
    margin: 0 0 8px;
    color: #3e9b79;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 1.6px;
}

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 30px;
    line-height: 1.25;
}

.page-description {
    margin: 10px 0 0;
    color: #6c7a74;
    font-size: 14px;
    line-height: 1.7;
}

.order-panel {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.skeleton-card {
    padding: 20px 24px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.order-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.order-card {
    display: flex;
    padding: 20px 24px;
    align-items: center;
    gap: 20px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
    cursor: pointer;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.order-card:hover {
    border-color: #3e9b79;
    box-shadow: 0 8px 20px rgb(36 115 91 / 10%);
}

.order-card-left {
    flex-shrink: 0;
}

.order-product-image {
    width: 80px;
    height: 80px;
    border-radius: 12px;
    overflow: hidden;
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

.order-card-middle {
    flex: 1;
    min-width: 0;
}

.order-product-name {
    margin: 0 0 8px;
    color: #1e2a26;
    font-size: 16px;
    font-weight: 600;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.order-meta {
    margin: 0 0 4px;
    color: #6c7a74;
    font-size: 13px;
}

.order-card-right {
    display: flex;
    flex-shrink: 0;
    flex-direction: column;
    align-items: flex-end;
    gap: 10px;
}

.order-price {
    margin: 0;
    color: #24735b;
    font-size: 18px;
    font-weight: 700;
}

@media (max-width: 760px) {
    .order-card {
        flex-direction: column;
        align-items: flex-start;
    }

    .order-card-right {
        align-items: flex-start;
    }
}
</style>
