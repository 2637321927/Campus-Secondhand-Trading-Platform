<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
    createOrder,
    getPaymentMethods,
    purchaseCheck
} from '../../api/modules/order'
import { getProductDetail, getProductImages } from '../../api/modules/product'
import { getMyAddresses } from '../../api/modules/address'
import type { ProductDto } from '../../types/api/product'
import type { PurchaseCheckDto } from '../../types/api/order'
import type { AddressDto } from '../../types/api/address'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const submitting = ref(false)
const errorMessage = ref('')

const product = ref<ProductDto | null>(null)
const productImageUrl = ref('')
const purchaseCheckResult = ref<PurchaseCheckDto | null>(null)
const addressList = ref<AddressDto[]>([])
const selectedAddressId = ref<number>(0)
const shippingMethod = ref('')
const note = ref('')
const paymentMethods = ref<Array<{ value: string; label: string }>>([])
const selectedPaymentMethod = ref('')

const productId = computed(() => Number(route.params.productId))

const totalPrice = computed(() => {
    if (!product.value) return 0
    const base = product.value.price
    const shipping = product.value.shippingFee ?? 0
    return base + shipping
})

async function loadProductImage(fileIds: number[]): Promise<void> {
    if (fileIds.length === 0) return

    try {
        const response = await getProductImages(fileIds)
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

async function loadData(): Promise<void> {
    const id = productId.value
    if (!id || Number.isNaN(id)) {
        errorMessage.value = '商品ID无效'
        return
    }

    loading.value = true
    errorMessage.value = ''

    try {
        const [productResponse, checkResponse, addressResponse, methodsResponse] =
            await Promise.allSettled([
                getProductDetail(id),
                purchaseCheck(id),
                getMyAddresses(),
                getPaymentMethods()
            ])

        if (productResponse.status === 'fulfilled') {
            product.value = productResponse.value.data
            const fileIds = (product.value?.images ?? [])
                .map(img => img.imgFileId)
                .filter(fid => fid > 0)
            await loadProductImage(fileIds)
        } else {
            throw productResponse.reason
        }

        if (checkResponse.status === 'fulfilled') {
            purchaseCheckResult.value = checkResponse.value.data
        }

        if (addressResponse.status === 'fulfilled') {
            addressList.value = addressResponse.value.data ?? []
            const defaultAddr = addressList.value.find(a => a.isDefault)
            selectedAddressId.value = defaultAddr?.addressId ?? addressList.value[0]?.addressId ?? 0
        }

        if (methodsResponse.status === 'fulfilled') {
            paymentMethods.value = methodsResponse.value.data ?? []
            if (paymentMethods.value.length > 0) {
                selectedPaymentMethod.value = paymentMethods.value[0].value
            }
        }
    } catch (error) {
        errorMessage.value = getApiErrorMessage(error, '商品信息加载失败，请稍后重试')
        console.error('购买确认页加载失败：', error)
    } finally {
        loading.value = false
    }
}

async function handleSubmitOrder(): Promise<void> {
    if (!product.value) return

    if (!selectedAddressId.value) {
        ElMessage.warning('请选择收货地址')
        return
    }

    submitting.value = true
    try {
        const response = await createOrder({
            productId: productId.value,
            addressId: selectedAddressId.value,
            shippingMethod: shippingMethod.value || null,
            note: note.value || null
        })

        ElMessage.success('订单创建成功')
        router.push({
            name: 'order-detail',
            params: { orderId: response.data.purchaseId }
        })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '创建订单失败'))
        console.error('创建订单失败：', error)
    } finally {
        submitting.value = false
    }
}

onMounted(() => {
    void loadData()
})
</script>

<template>
    <main class="purchase-page">
        <div class="purchase-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 加载状态 -->
            <section v-if="loading" class="purchase-panel">
                <el-skeleton :rows="5" animated />
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadData">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 正常内容 -->
            <template v-else-if="product">
                <!-- 页面头部 -->
                <header class="page-header">
                    <p class="page-eyebrow">PURCHASE CONFIRM</p>
                    <h1>购买确认</h1>
                </header>

                <!-- 不可购买提示 -->
                <el-alert
                    v-if="purchaseCheckResult && !purchaseCheckResult.canPurchase"
                    :title="purchaseCheckResult.reason ?? '当前商品不可购买'"
                    type="warning"
                    :closable="false"
                    show-icon
                    class="purchase-alert"
                />

                <!-- 商品信息 -->
                <section class="purchase-panel">
                    <h2 class="panel-title">商品信息</h2>
                    <div class="product-row">
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
                            <h3>{{ product.name }}</h3>
                            <p class="product-price">¥{{ product.price.toFixed(2) }}</p>
                            <p class="product-shipping" v-if="product.shippingFee">
                                运费：¥{{ product.shippingFee.toFixed(2) }}
                            </p>
                            <p class="product-shipping" v-else>
                                运费：包邮
                            </p>
                        </div>
                    </div>
                </section>

                <!-- 收货地址 -->
                <section class="purchase-panel">
                    <h2 class="panel-title">收货地址</h2>
                    <div v-if="addressList.length === 0" class="no-address">
                        <span>暂无收货地址</span>
                        <el-button
                            type="primary"
                            size="small"
                            @click="router.push({ name: 'user-addresses' })"
                        >
                            去添加
                        </el-button>
                    </div>
                    <el-radio-group
                        v-else
                        v-model="selectedAddressId"
                        class="address-list"
                    >
                        <el-radio
                            v-for="addr in addressList"
                            :key="addr.addressId"
                            :value="addr.addressId"
                            class="address-item"
                        >
                            <span class="addr-name">{{ addr.name }}</span>
                            <span class="addr-phone">{{ addr.phoneNumber }}</span>
                            <span class="addr-detail">{{ addr.detailAddress }}</span>
                            <el-tag v-if="addr.isDefault" type="success" effect="plain" size="small">
                                默认
                            </el-tag>
                        </el-radio>
                    </el-radio-group>
                </section>

                <!-- 发货方式与备注 -->
                <section class="purchase-panel">
                    <h2 class="panel-title">发货方式与备注</h2>
                    <el-form label-width="100px">
                        <el-form-item label="发货方式">
                            <el-input
                                v-model="shippingMethod"
                                placeholder="如：快递、自提、面交（选填）"
                                clearable
                            />
                        </el-form-item>
                        <el-form-item label="买家备注">
                            <el-input
                                v-model="note"
                                type="textarea"
                                :rows="3"
                                placeholder="给卖家的备注（选填）"
                                maxlength="200"
                                show-word-limit
                            />
                        </el-form-item>
                    </el-form>
                </section>

                <!-- 支付方式 -->
                <section class="purchase-panel" v-if="paymentMethods.length > 0">
                    <h2 class="panel-title">支付方式</h2>
                    <el-radio-group v-model="selectedPaymentMethod">
                        <el-radio
                            v-for="method in paymentMethods"
                            :key="method.value"
                            :value="method.value"
                        >
                            {{ method.label }}
                        </el-radio>
                    </el-radio-group>
                </section>

                <!-- 价格汇总 -->
                <section class="purchase-panel">
                    <h2 class="panel-title">价格汇总</h2>
                    <div class="price-summary">
                        <div class="price-row">
                            <span>商品价格</span>
                            <span>¥{{ product.price.toFixed(2) }}</span>
                        </div>
                        <div class="price-row">
                            <span>运费</span>
                            <span>¥{{ (product.shippingFee ?? 0).toFixed(2) }}</span>
                        </div>
                        <div class="price-row total-row">
                            <span>合计</span>
                            <span class="total-price">¥{{ totalPrice.toFixed(2) }}</span>
                        </div>
                    </div>
                </section>

                <!-- 提交按钮 -->
                <div class="submit-bar">
                    <el-button
                        type="primary"
                        size="large"
                        :loading="submitting"
                        :disabled="!selectedAddressId || (purchaseCheckResult !== null && !purchaseCheckResult.canPurchase)"
                        @click="handleSubmitOrder"
                    >
                        提交订单
                    </el-button>
                </div>
            </template>
        </div>
    </main>
</template>

<style scoped>
.purchase-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.purchase-container {
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

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 28px;
    line-height: 1.25;
}

.purchase-alert {
    margin-bottom: 20px;
}

.purchase-panel {
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
    margin: 0 0 4px;
    color: #24735b;
    font-size: 16px;
    font-weight: 700;
}

.product-shipping {
    margin: 0;
    color: #6c7a74;
    font-size: 13px;
}

.no-address {
    display: flex;
    align-items: center;
    gap: 12px;
    color: #6c7a74;
    font-size: 14px;
}

.address-list {
    display: flex;
    flex-direction: column;
    gap: 12px;
    width: 100%;
}

.address-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 16px;
    background: #f5f7f6;
    border: 1px solid #e3e9e6;
    border-radius: 10px;
}

.addr-name {
    color: #1e2a26;
    font-weight: 600;
    font-size: 14px;
}

.addr-phone {
    color: #6c7a74;
    font-size: 13px;
}

.addr-detail {
    color: #46534d;
    font-size: 14px;
    flex: 1;
}

.price-summary {
    display: flex;
    flex-direction: column;
    gap: 12px;
}

.price-row {
    display: flex;
    justify-content: space-between;
    color: #6c7a74;
    font-size: 14px;
}

.total-row {
    padding-top: 12px;
    border-top: 1px solid #e3e9e6;
    color: #1e2a26;
    font-weight: 600;
    font-size: 16px;
}

.total-price {
    color: #24735b;
    font-size: 20px;
    font-weight: 700;
}

.submit-bar {
    display: flex;
    justify-content: flex-end;
    margin-top: 8px;
}
</style>
