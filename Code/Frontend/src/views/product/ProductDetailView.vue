<script setup lang="ts">
import {computed,ref,watch} from 'vue'
import {
  useRoute,
  useRouter
} from 'vue-router'
import { ElMessage } from 'element-plus'
import { getProductDetail } from '../../api/modules/product'
import type {
  ProductDto,
  ProductStatus
} from '../../types/api/product'
import { resolveFileUrl } from '../../utils/image'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const product = ref<ProductDto | null>(null)
const selectedImageUrl = ref('')

const productId = computed<number | null>(() => {
  const value = route.params.productId

  if (typeof value !== 'string') {
    return null
  }

  const id = Number(value)

  if (!Number.isInteger(id) || id <= 0) {
    return null
  }

  return id
})

const sortedImages = computed(() => {
  if (!product.value) {
    return []
  }

  return [...(product.value.images ?? [])].sort(
    (a, b) => a.imgIndex - b.imgIndex
  )
})

async function loadProduct(): Promise<void> {
  if (productId.value === null) {
    errorMessage.value = '商品编号不正确'
    product.value = null
    return
  }

  loading.value = true
  errorMessage.value = ''
  product.value = null
  selectedImageUrl.value = ''

  try {
    const response = await getProductDetail(productId.value)

    product.value = response.data

    const firstImage = [...response.data.images]
      .sort((a, b) => a.imgIndex - b.imgIndex)[0]

    if(firstImage){
        selectedImageUrl.value=resolveFileUrl(firstImage.imgFileId)
    }
    else{
        selectedImageUrl.value=resolveFileUrl(null)
    }

  } 
  catch (error) {
    errorMessage.value = '商品详情加载失败，请稍后重试'
    console.error('商品详情加载失败：', error)
  } 
  finally {
    loading.value = false
  }
}

watch(
  productId,
  () => {
    loadProduct()
  },
  {
    immediate: true
  }
)

function getStatusText(status: ProductStatus): string {
  if (status === 0) {
    return '在售'
  }

  if (status === 1) {
    return '已售'
  }

  return '已下架'
}

function getStatusClass(status: ProductStatus): string {
  if (status === 0) {
    return 'status-available'
  }

  if (status === 1) {
    return 'status-sold'
  }

  return 'status-removed'
}

function selectImage(fileId: number): void {
  selectedImageUrl.value = resolveFileUrl(fileId)
}

function handleBuy(): void {
  if (product.value?.status !== 0) {
    ElMessage.warning('当前商品不可购买')
    return
  }

  ElMessage.info('购买功能将在订单模块中开放')
}

function handleContactSeller(): void {
  ElMessage.info('联系卖家功能将在消息模块中开放')
}

function handleFavorite(): void {
  ElMessage.info('收藏功能将在收藏模块中开放')
}
</script>

<template>
  <div class="product-detail-page">
    <!-- 加载状态 -->
    <div v-if="loading" class="detail-state">
      <div class="state-loading"></div>
      <h2>商品详情加载中</h2>
      <p>正在获取商品信息，请稍候...</p>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="errorMessage" class="detail-state error-state">
      <div class="state-symbol">!</div>

      <h2>商品加载失败</h2>

      <p>{{ errorMessage }}</p>

      <div class="state-actions">
        <el-button type="primary" @click="loadProduct">
          重新加载
        </el-button>

        <el-button @click="router.push('/products')">
          返回商品列表
        </el-button>
      </div>
    </div>

    <!-- 商品详情 -->
    <div v-else-if="product" class="detail-content">
      <!-- 返回按钮 -->
      <div class="back-row">
        <el-button text @click="router.push('/products')">
          ← 返回商品列表
        </el-button>
      </div>

      <!-- 商品主体 -->
      <div class="product-main">
        <!-- 左侧图片区域 -->
        <section class="product-gallery">
          <div class="main-image">
            <el-image
              v-if="selectedImageUrl"
              :src="selectedImageUrl"
              :alt="product.name"
              fit="contain"
              preview-teleported
              :preview-src-list="
                sortedImages.map((image) =>
                  resolveFileUrl(image.imgFileId)
                )
              "
            >
              <template #error>
                <div class="image-placeholder">
                  <span>图片加载失败</span>
                </div>
              </template>
            </el-image>

            <div v-else class="image-placeholder">
              <span class="placeholder-icon">图</span>
              <span>暂无商品图片</span>
            </div>
          </div>

          <!-- 缩略图 -->
          <div
            v-if="sortedImages.length > 1"
            class="thumbnail-list"
          >
            <button
              v-for="image in sortedImages"
              :key="image.imgFileId"
              class="thumbnail-button"
              :class="{
                active:
                  selectedImageUrl ===
                  resolveFileUrl(image.imgFileId)
              }"
              type="button"
              @click="selectImage(image.imgFileId)"
            >
              <el-image
                :src="resolveFileUrl(image.imgFileId)"
                :alt="`${product.name}商品图片`"
                fit="cover"
              >
                <template #error>
                  <div class="thumbnail-placeholder">
                    暂无
                  </div>
                </template>
              </el-image>
            </button>
          </div>

          <p
            v-if="sortedImages.length > 1"
            class="gallery-tip"
          >
            点击缩略图切换商品图片
          </p>
        </section>

        <!-- 右侧商品信息 -->
        <section class="product-summary">
          <div class="summary-top">
            <span
              class="product-status"
              :class="getStatusClass(product.status)"
            >
              {{ getStatusText(product.status) }}
            </span>

            <span class="product-id">
              商品编号：{{ product.productId }}
            </span>
          </div>

          <h1 class="product-name">
            {{ product.name }}
          </h1>

          <div class="price-box">
            <span class="price-label">商品价格</span>

            <p class="product-price">
              <span class="currency">¥</span>
              {{ product.price.toFixed(2) }}
            </p>
          </div>

          <!-- 商品基础信息 -->
          <div class="product-meta">
            <div class="meta-item">
              <span class="meta-label">商品分类</span>

              <span class="meta-value">
                {{ product.categoryName ?? '未分类' }}
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">浏览次数</span>

              <span class="meta-value">
                {{ product.viewCount }} 次
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">卖家编号</span>

              <span class="meta-value">
                {{ product.userId }}
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">交易状态</span>

              <span class="meta-value">
                {{ getStatusText(product.status) }}
              </span>
            </div>
          </div>

          <!-- 操作按钮 -->
          <div class="product-actions">
            <el-button
              type="primary"
              size="large"
              class="buy-button"
              :disabled="product.status !== 0"
              @click="handleBuy"
            >
              {{
                product.status === 0
                  ? '立即购买'
                  : '当前不可购买'
              }}
            </el-button>

            <el-button
              size="large"
              class="contact-button"
              @click="handleContactSeller"
            >
              联系卖家
            </el-button>

            <el-button
              size="large"
              class="favorite-button"
              @click="handleFavorite"
            >
              收藏商品
            </el-button>
          </div>

          <!-- 交易提示 -->
          <div class="trade-notice">
            <h3>交易提醒</h3>

            <ul>
              <li>建议优先选择校内公共场所当面交易。</li>
              <li>交易前请仔细确认商品实际情况。</li>
              <li>请勿脱离平台进行可疑转账或付款。</li>
            </ul>
          </div>
        </section>
      </div>

      <!-- 商品描述 -->
      <section class="detail-section product-description">
        <div class="section-title">
          <h2>商品描述</h2>
          <span>卖家提供的商品详细信息</span>
        </div>

        <p v-if="product.info" class="description-content">
          {{ product.info }}
        </p>

        <div v-else class="empty-description">
          卖家暂未填写商品描述
        </div>
      </section>

      <!-- 卖家信息占位 -->
      <section class="detail-section seller-section">
        <div class="section-title">
          <h2>卖家信息</h2>
          <span>更多卖家资料将在用户模块中展示</span>
        </div>

        <div class="seller-card">
          <div class="seller-avatar">
            {{ String(product.userId).slice(0, 1) }}
          </div>

          <div class="seller-info">
            <strong>卖家编号 {{ product.userId }}</strong>

            <span>
              当前商品由该用户发布
            </span>
          </div>

          <el-button @click="handleContactSeller">
            联系卖家
          </el-button>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.product-detail-page {
  min-height: calc(100vh - 72px);
  padding: 32px 20px 72px;
  background: #f5f7f6;
}

.detail-content {
  width: min(1280px, 100%);
  margin: 0 auto;
}

/* 加载和错误状态 */

.detail-state {
  display: flex;
  min-height: 520px;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #68766f;
  text-align: center;
}

.detail-state h2 {
  margin: 18px 0 8px;
  color: #26352f;
  font-size: 24px;
}

.detail-state p {
  max-width: 460px;
  margin: 0;
  line-height: 1.7;
}

.state-loading {
  width: 42px;
  height: 42px;
  border: 4px solid #dce9e3;
  border-top-color: #3e9b79;
  border-radius: 50%;
  animation: loading-rotate 0.9s linear infinite;
}

.state-symbol {
  display: flex;
  width: 52px;
  height: 52px;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  background: #d96363;
  border-radius: 50%;
  font-size: 30px;
  font-weight: 700;
}

.state-actions {
  display: flex;
  gap: 12px;
  margin-top: 24px;
}

@keyframes loading-rotate {
  from {
    transform: rotate(0deg);
  }

  to {
    transform: rotate(360deg);
  }
}

/* 返回按钮 */

.back-row {
  margin-bottom: 18px;
}

.back-row :deep(.el-button) {
  padding-left: 0;
  color: #537168;
}

/* 商品主体 */

.product-main {
  display: grid;
  grid-template-columns:
    minmax(0, 1.08fr)
    minmax(380px, 0.92fr);
  gap: 42px;
  padding: 30px;
  background: #ffffff;
  border: 1px solid #e2e9e6;
  border-radius: 20px;
  box-shadow: 0 12px 40px rgb(37 63 52 / 6%);
}

/* 商品图片 */

.product-gallery {
  min-width: 0;
}

.main-image {
  display: flex;
  width: 100%;
  aspect-ratio: 1 / 1;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: #f7f9f8;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.main-image :deep(.el-image) {
  width: 100%;
  height: 100%;
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 12px;
  color: #8a9791;
  background:
    linear-gradient(
      135deg,
      #f6f8f7 0%,
      #edf2ef 100%
    );
}

.placeholder-icon {
  display: flex;
  width: 58px;
  height: 58px;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  background: #b8c9c1;
  border-radius: 16px;
  font-size: 22px;
  font-weight: 700;
}

.thumbnail-list {
  display: flex;
  gap: 12px;
  margin-top: 16px;
  padding-bottom: 4px;
  overflow-x: auto;
}

.thumbnail-button {
  flex: 0 0 76px;
  width: 76px;
  height: 76px;
  padding: 3px;
  overflow: hidden;
  background: #ffffff;
  border: 2px solid transparent;
  border-radius: 12px;
  cursor: pointer;
  transition:
    border-color 0.2s ease,
    transform 0.2s ease;
}

.thumbnail-button:hover {
  transform: translateY(-2px);
  border-color: #9bc8b7;
}

.thumbnail-button.active {
  border-color: #3e9b79;
}

.thumbnail-button :deep(.el-image) {
  width: 100%;
  height: 100%;
  overflow: hidden;
  border-radius: 8px;
}

.thumbnail-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #8a9791;
  background: #edf2ef;
  font-size: 12px;
}

.gallery-tip {
  margin: 10px 0 0;
  color: #8a9791;
  font-size: 13px;
}

/* 商品概要 */

.product-summary {
  display: flex;
  min-width: 0;
  flex-direction: column;
}

.summary-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.product-status {
  display: inline-flex;
  padding: 6px 12px;
  align-items: center;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 600;
}

.status-available {
  color: #24735b;
  background: #e8f6ef;
}

.status-sold {
  color: #9b681f;
  background: #fff2d9;
}

.status-removed {
  color: #69746f;
  background: #edf0ef;
}

.product-id {
  color: #919d98;
  font-size: 13px;
}

.product-name {
  margin: 22px 0 18px;
  color: #1f2d27;
  font-size: clamp(26px, 3vw, 38px);
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.price-box {
  padding: 20px 22px;
  background:
    linear-gradient(
      135deg,
      #f0f8f4 0%,
      #e7f4ee 100%
    );
  border-radius: 16px;
}

.price-label {
  color: #668076;
  font-size: 13px;
}

.product-price {
  margin: 8px 0 0;
  color: #e16b3f;
  font-size: 38px;
  font-weight: 700;
  line-height: 1;
}

.currency {
  margin-right: 3px;
  font-size: 22px;
}

.product-meta {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  margin-top: 24px;
  overflow: hidden;
  border: 1px solid #e7ecea;
  border-radius: 14px;
}

.meta-item {
  display: flex;
  min-width: 0;
  padding: 16px;
  flex-direction: column;
  gap: 6px;
  border-right: 1px solid #e7ecea;
  border-bottom: 1px solid #e7ecea;
}

.meta-item:nth-child(2n) {
  border-right: 0;
}

.meta-item:nth-last-child(-n + 2) {
  border-bottom: 0;
}

.meta-label {
  color: #8a9691;
  font-size: 13px;
}

.meta-value {
  overflow: hidden;
  color: #34443d;
  font-size: 15px;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 操作区域 */

.product-actions {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
  margin-top: 28px;
}

.buy-button {
  grid-column: 1 / -1;
  min-height: 48px;
  font-size: 16px;
}

.contact-button,
.favorite-button {
  min-height: 44px;
}

.trade-notice {
  margin-top: 26px;
  padding: 18px 20px;
  color: #69766f;
  background: #fafbfa;
  border: 1px solid #e8edeb;
  border-radius: 14px;
}

.trade-notice h3 {
  margin: 0 0 10px;
  color: #3c4d45;
  font-size: 15px;
}

.trade-notice ul {
  margin: 0;
  padding-left: 20px;
}

.trade-notice li {
  margin: 6px 0;
  line-height: 1.6;
}

/* 下方详情模块 */

.detail-section {
  margin-top: 24px;
  padding: 28px 30px;
  background: #ffffff;
  border: 1px solid #e2e9e6;
  border-radius: 18px;
}

.section-title {
  display: flex;
  align-items: baseline;
  gap: 12px;
  padding-bottom: 16px;
  border-bottom: 1px solid #edf1ef;
}

.section-title h2 {
  margin: 0;
  color: #26352f;
  font-size: 21px;
}

.section-title span {
  color: #909b96;
  font-size: 13px;
}

.description-content {
  margin: 22px 0 0;
  color: #485850;
  font-size: 15px;
  line-height: 1.9;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.empty-description {
  margin-top: 22px;
  padding: 36px 20px;
  color: #8b9792;
  background: #f7f9f8;
  border-radius: 12px;
  text-align: center;
}

/* 卖家信息 */

.seller-card {
  display: flex;
  margin-top: 22px;
  align-items: center;
  gap: 16px;
}

.seller-avatar {
  display: flex;
  width: 54px;
  height: 54px;
  flex: 0 0 54px;
  align-items: center;
  justify-content: center;
  color: #ffffff;
  background: #3e9b79;
  border-radius: 50%;
  font-size: 20px;
  font-weight: 700;
}

.seller-info {
  display: flex;
  min-width: 0;
  flex: 1;
  flex-direction: column;
  gap: 6px;
}

.seller-info strong {
  overflow: hidden;
  color: #34443d;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.seller-info span {
  color: #84908b;
  font-size: 13px;
}
</style>