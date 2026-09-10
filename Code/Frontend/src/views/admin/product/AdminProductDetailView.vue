<template>
  <div class="admin-product-detail-page">
    <!-- 页面加载状态 -->
    <div v-if="loading" class="detail-state">
      <div class="state-loading"></div>
      <h2>商品详情加载中</h2>
      <p>正在获取商品信息，请稍候...</p>
    </div>

    <!-- 页面错误状态 -->
    <div v-else-if="errorMessage" class="detail-state error-state">
      <div class="state-symbol">!</div>
      <h2>商品加载失败</h2>
      <p>{{ errorMessage }}</p>
      <div class="state-actions">
        <el-button type="primary" @click="loadProduct">重新加载</el-button>
        <el-button @click="router.push('/admin/products')">返回商品列表</el-button>
      </div>
    </div>

    <!-- 商品详情主体 -->
    <div v-else-if="product" class="detail-content">
      <!-- 返回商品列表 -->
      <div class="back-row">
        <el-button text @click="router.push('/admin/products')">
          ← 返回商品列表
        </el-button>
      </div>

      <!-- 商品主要信息 -->
      <div class="product-main">
        <!-- 左侧商品图片 -->
        <section class="product-gallery">
          <div class="main-image">
            <el-image
              v-if="selectedImageUrl"
              :src="selectedImageUrl"
              :alt="product.name"
              fit="contain"
              preview-teleported
              :preview-src-list="previewImageUrls"
            >
              <template #error>
                <div class="image-placeholder">
                  <span class="placeholder-icon">图</span>
                  <span>图片加载失败</span>
                </div>
              </template>
            </el-image>
            <div v-else class="image-placeholder">
              <span class="placeholder-icon">图</span>
              <span>暂无商品图片</span>
            </div>
          </div>

          <!-- 商品缩略图 -->
          <div v-if="sortedImages.length > 1" class="thumbnail-list">
            <button
              v-for="image in sortedImages"
              :key="image.imgFileId || image.fileId || image.imgId"
              class="thumbnail-button"
              :class="{ active: selectedImageFileId === (image.imgFileId || image.fileId || image.imgId) }"
              type="button"
              @click="selectImage(image.imgFileId || image.fileId || image.imgId)"
            >
              <el-image
                :src="getProductImageUrl(image.imgFileId || image.fileId || image.imgId)"
                :alt="`${product.name}商品图片`"
                fit="cover"
              >
                <template #error>
                  <div class="thumbnail-placeholder">暂无</div>
                </template>
              </el-image>
            </button>
          </div>
          <p v-if="sortedImages.length > 1" class="gallery-tip">点击缩略图切换商品图片</p>
        </section>

        <!-- 商品概要 -->
        <section class="product-summary">
          <div class="summary-top">
            <span class="product-status" :class="getStatusClass(product.status)">
              {{ getStatusText(product.status) }}
            </span>
            <span class="product-id">商品编号：{{ product.productId }}</span>
          </div>

          <h1 class="product-name">{{ product.name }}</h1>

          <!-- 商品价格 -->
          <div class="price-box">
            <span class="price-label">商品价格</span>
            <p class="product-price">
              <span class="currency">¥</span>
              {{ Number(product.price).toFixed(2) }}
            </p>
          </div>

          <!-- 商品基础信息 -->
          <div class="product-meta">
            <div class="meta-item">
              <span class="meta-label">商品分类</span>
              <span class="meta-value">{{ product.categoryName ?? '未分类' }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">浏览次数</span>
              <span class="meta-value">{{ product.viewCount || 0 }} 次</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">收藏人数</span>
              <span class="meta-value">{{ product.favoriteCount || 0 }} 人</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">卖家编号</span>
              <span class="meta-value">{{ product.userId || product.sellerId }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">卖家名称</span>
              <span class="meta-value">{{ product.sellerName || '未知' }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">交易状态</span>
              <span class="meta-value">{{ getStatusText(product.status) }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">配送方式</span>
              <span class="meta-value">{{ getShippingTypeText(product.shippingType) }}</span>
            </div>
            <div v-if="product.shippingType === 2" class="meta-item">
              <span class="meta-label">固定邮费</span>
              <span class="meta-value">¥{{ Number(product.shippingFee ?? 0).toFixed(2) }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">校内自提</span>
              <span class="meta-value">{{ product.allowPickup === 1 ? '支持' : '不支持' }}</span>
            </div>
            <div v-if="product.rejectReason" class="meta-item" style="grid-column: 1 / -1;">
              <span class="meta-label" style="color: #f56c6c;">驳回原因</span>
              <span class="meta-value" style="color: #f56c6c;">{{ product.rejectReason }}</span>
            </div>
          </div>
        </section>
      </div>

      <!-- 商品描述 -->
      <section class="detail-section product-description">
        <div class="section-title">
          <div>
            <h2>商品描述</h2>
            <span>卖家提供的商品详细信息</span>
          </div>
        </div>
        <p v-if="product.info" class="description-content">{{ product.info }}</p>
        <div v-else class="empty-description">卖家暂未填写商品描述</div>
      </section>

      <!-- 卖家信息 -->
      <section class="detail-section seller-section">
        <div class="section-title">
          <div>
            <h2>卖家信息</h2>
            <span>查看商品发布者的公开信息</span>
          </div>
        </div>
        <div v-if="seller" class="seller-card">
          <UserAvatar
            :size="56"
            :name="seller.userName"
            :file-id="seller.avatarFileId"
            className="seller-avatar"
          />
          <div class="seller-info">
            <strong>{{ seller.userName || `卖家 ${seller.userId}` }}</strong>
            <span>用户编号：{{ seller.userId }}</span>
          </div>
        </div>
        <div v-else class="seller-empty">暂无卖家公开信息</div>
      </section>

      <!-- 审核日志（如果有） -->
      <section v-if="auditLogs.length > 0" class="detail-section log-section">
        <div class="section-title">
          <div>
            <h2>审核记录</h2>
            <span>商品审核历史</span>
          </div>
        </div>
        <el-timeline>
          <el-timeline-item
            v-for="log in auditLogs"
            :key="log.auditId || log.id"
            :timestamp="formatDate(log.createTime || log.createdAt)"
            :type="getAuditActionType(log.action)"
          >
            <div>
              <span>{{ log.adminName || '管理员' }} 执行了</span>
              <el-tag size="small" :type="getAuditActionType(log.action)">
                {{ getAuditActionText(log.action) }}
              </el-tag>
              <span v-if="log.reason">，原因：{{ log.reason }}</span>
            </div>
          </el-timeline-item>
        </el-timeline>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getAdminProductDetail } from '../../../api/modules/admin'
import { getPublicUser } from '../../../api/modules/user'
import { useProductImages } from '../../../composables/useProductImages'
import UserAvatar from '../../../components/common/UserAvatar.vue'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const product = ref<any>(null)
const seller = ref<any>(null)
const auditLogs = ref<any[]>([])
const selectedImageFileId = ref<number | null>(null)

const {
  getProductImageUrl,
  loadProductImages,
  clearProductImages
} = useProductImages()

const productId = computed(() => {
  const id = Number(route.params.productId)
  return isNaN(id) || id <= 0 ? null : id
})

// ========== 图片处理 ==========
const sortedImages = computed(() => {
  if (!product.value?.images) return []
  return [...product.value.images].sort((a, b) => (a.imgIndex || 0) - (b.imgIndex || 0))
})

const previewImageUrls = computed(() =>
  sortedImages.value
    .map((image) => getProductImageUrl(image.imgFileId || image.fileId || image.imgId))
    .filter(Boolean)
)

const selectedImageUrl = computed(() => {
  if (!selectedImageFileId.value) return null
  return getProductImageUrl(selectedImageFileId.value)
})

function selectImage(fileId: number): void {
  selectedImageFileId.value = fileId
}

// ========== 状态工具 ==========
const statusMap: Record<number, { text: string; class: string }> = {
  0: { text: '在售', class: 'status-available' },
  1: { text: '已售', class: 'status-sold' },
  2: { text: '已下架', class: 'status-removed' },
  3: { text: '待审核', class: 'status-draft' },
  4: { text: '已驳回', class: 'status-removed' },
  5: { text: '交易中', class: 'status-draft' },
  6: { text: '管理员下架', class: 'status-removed' }
}

function getStatusText(status: number): string {
  return statusMap[status]?.text || '未知状态'
}

function getStatusClass(status: number): string {
  return statusMap[status]?.class || 'status-removed'
}

function getShippingTypeText(shippingType: number): string {
  const labels = ['包邮', '按距离计费', '固定邮费', '无需邮寄']
  return labels[shippingType] ?? '未知'
}

// ========== 审核日志工具 ==========
const auditActionMap: Record<string, { text: string; type: string }> = {
  approve: { text: '审核通过', type: 'success' },
  reject: { text: '审核驳回', type: 'danger' },
  remove: { text: '强制下架', type: 'danger' },
  restore: { text: '恢复上架', type: 'warning' },
  delete: { text: '删除商品', type: 'danger' }
}

function getAuditActionText(action: string): string {
  return auditActionMap[action]?.text || action
}

function getAuditActionType(action: string): 'success' | 'danger' | 'warning' | 'primary' | 'info' {
  return (auditActionMap[action]?.type as any) || 'info'
}

// ========== 加载数据 ==========
async function loadProduct(): Promise<void> {
  const id = productId.value
  if (!id) {
    errorMessage.value = '商品编号不正确'
    return
  }

  loading.value = true
  errorMessage.value = ''
  product.value = null
  seller.value = null
  auditLogs.value = []
  selectedImageFileId.value = null
  clearProductImages()

  try {
    const res = await getAdminProductDetail(id)
    const data = res?.data || res || {}
    product.value = data

    await loadProductImages(
      (data.images ?? []).map((image: { fileId: number }) => image.fileId)
    ).catch((error) => {
      console.error('管理员商品图片加载失败:', error)
    })

    // 设置默认图片
    if (data.images?.length) {
      const firstImage = data.images[0]
      selectedImageFileId.value = firstImage.fileId
    }

    // 加载审核日志
    auditLogs.value = data.auditLogs || []

    // 加载卖家信息
    const sellerId = data.userId
    if (sellerId) {
      try {
        const userRes = await getPublicUser(sellerId)
        seller.value = userRes
      } catch {
        seller.value = { userId: sellerId, userName: `用户 ${sellerId}` }
      }
    }
  } catch (error: any) {
    console.error('加载商品详情失败:', error)
    errorMessage.value = error?.message || '商品详情加载失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

function formatDate(date: string): string {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

// ========== 生命周期 ==========
onMounted(() => {
  loadProduct()
})
</script>

<style scoped>
.admin-product-detail-page {
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

/* 商品主体 - 复用 ProductDetailView 样式 */
.product-main {
  display: grid;
  grid-template-columns: minmax(0, 1.08fr) minmax(380px, 0.92fr);
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
  background: linear-gradient(135deg, #f6f8f7 0%, #edf2ef 100%);
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
  transition: border-color 0.2s ease, transform 0.2s ease;
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

.status-draft {
  color: #9b681f;
  background: #fff2d9;
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
  background: linear-gradient(135deg, #f0f8f4 0%, #e7f4ee 100%);
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

/* 详情模块 */
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

.seller-empty {
  margin-top: 22px;
  padding: 36px 20px;
  color: #8b9792;
  background: #f7f9f8;
  border-radius: 12px;
  text-align: center;
}

/* 审核日志 */
.log-section :deep(.el-timeline-item__wrapper) {
  padding-left: 20px;
}

/* 响应式 */
@media (max-width: 1000px) {
  .product-main {
    grid-template-columns: 1fr;
    gap: 24px;
  }
}

@media (max-width: 600px) {
  .product-main {
    padding: 16px;
  }
  .detail-section {
    padding: 16px;
  }
  .product-meta {
    grid-template-columns: 1fr;
  }
  .meta-item {
    border-right: 0;
  }
  .meta-item:nth-child(2n) {
    border-right: 0;
  }
}
</style>
