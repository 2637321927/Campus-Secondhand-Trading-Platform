<script setup lang="ts">
import {
  computed,
  onBeforeUnmount,
  reactive,
  ref,
  watch
} from 'vue'
import {
  useRoute,
  useRouter
} from 'vue-router'
import axios from 'axios'
import { ElMessage } from 'element-plus'
import {
  getProductDetail
} from '../../api/modules/product'
import {
  createProductComment,
  getProductComments
} from '../../api/modules/comment'
import type {
  ProductDto,
  ProductStatus
} from '../../types/api/product'
import type { ProductCommentDto } from '../../types/api/comment'
import SellerProductActions from '../../components/product/SellerProductActions.vue'
import { useProductImages } from '../../composables/useProductImages'

const route = useRoute()
const router = useRouter()

const product = ref<ProductDto | null>(null)
const productLoading = ref(false)
const productErrorMessage = ref('')
const {
  loadProductImages,
  getProductImageUrl
} = useProductImages()

const comments = ref<ProductCommentDto[]>([])
const commentsLoading = ref(false)
const commentsErrorMessage = ref('')

const replyContents = reactive<Record<number, string>>({})
const replyingCommentId = ref<number | null>(null)

let loadVersion = 0

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

function countComments(items: ProductCommentDto[]): number {
  return items.reduce(
    (count, comment) =>
      count + 1 + countComments(comment.replies ?? []),
    0
  )
}

const commentCount = computed(() =>
  countComments(comments.value)
)

function getStatusText(status: ProductStatus): string {
  if (status === 0) {
    return '在售'
  }

  if (status === 1) {
    return '已售'
  }

  if (status === 2) {
    return '已下架'
  }

  return '未知状态'
}

function getStatusType(
  status: ProductStatus
): 'success' | 'warning' | 'info' | 'danger' {
  if (status === 0) {
    return 'success'
  }

  if (status === 1) {
    return 'info'
  }

  if (status === 2) {
    return 'danger'
  }

  return 'warning'
}

function getShippingTypeLabel(
  shippingType: number
): string {
  if (shippingType === 0) {
    return '包邮'
  }

  if (shippingType === 1) {
    return '按距离计费'
  }

  if (shippingType === 2) {
    return '固定邮费'
  }

  if (shippingType === 3) {
    return '无需邮寄'
  }

  return '未知'
}

function formatCommentTime(value: string): string {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString('zh-CN')
}

function isCurrentLoad(
  version: number,
  requestedProductId: number
): boolean {
  return (
    version === loadVersion &&
    productId.value === requestedProductId
  )
}

function getProductErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return '商品管理详情加载失败，请稍后重试'
  }

  if (!error.response) {
    return '无法连接后端服务，请稍后重试'
  }

  if (error.response.status === 403) {
    return '你无权管理这件商品'
  }

  if (error.response.status === 404) {
    return '商品不存在，或卖家详情接口尚未实现'
  }

  return '商品管理详情加载失败，请稍后重试'
}

async function loadProduct(
  requestedProductId: number,
  version = loadVersion
): Promise<void> {
  if (!isCurrentLoad(version, requestedProductId)) {
    return
  }

  productLoading.value = true
  productErrorMessage.value = ''

  try {
    const response = await getProductDetail(
      requestedProductId
    )

    if (!isCurrentLoad(version, requestedProductId)) {
      return
    }

    product.value = response.data

    await loadProductImages(
      (response.data.images ?? []).map(
        (image) => image.imgFileId
      )
    ).catch((error) => {
      console.error('卖家商品图片加载失败：', error)
    })
  } catch (error) {
    if (!isCurrentLoad(version, requestedProductId)) {
      return
    }

    productErrorMessage.value =
      getProductErrorMessage(error)

    console.error('卖家商品详情加载失败：', error)
  } finally {
    if (isCurrentLoad(version, requestedProductId)) {
      productLoading.value = false
    }
  }
}

async function loadComments(
  requestedProductId: number,
  version = loadVersion
): Promise<void> {
  if (!isCurrentLoad(version, requestedProductId)) {
    return
  }

  commentsLoading.value = true
  commentsErrorMessage.value = ''

  try {
    const response = await getProductComments(
      requestedProductId
    )

    if (!isCurrentLoad(version, requestedProductId)) {
      return
    }

    comments.value = response.data ?? []

    for (const comment of comments.value) {
      if (typeof replyContents[comment.commentId] !== 'string') {
        replyContents[comment.commentId] = ''
      }
    }
  } catch (error) {
    if (!isCurrentLoad(version, requestedProductId)) {
      return
    }

    commentsErrorMessage.value =
      '卖家留言加载失败，商品和统计区域不受影响'

    console.error('卖家留言加载失败：', error)
  } finally {
    if (isCurrentLoad(version, requestedProductId)) {
      commentsLoading.value = false
    }
  }
}

function loadPage(): void {
  const requestedProductId = productId.value
  const currentVersion = ++loadVersion

  product.value = null
  comments.value = []
  productLoading.value = false
  commentsLoading.value = false

  productErrorMessage.value = ''
  commentsErrorMessage.value = ''

  if (requestedProductId === null) {
    productErrorMessage.value = '商品编号不正确'
    return
  }

  void loadProduct(
    requestedProductId,
    currentVersion
  )

  void loadComments(
    requestedProductId,
    currentVersion
  )
}

async function handleReply(
  comment: ProductCommentDto
): Promise<void> {
  const requestedProductId = productId.value
  const currentVersion = loadVersion

  if (requestedProductId === null) {
    return
  }

  if (replyingCommentId.value !== null) {
    return
  }

  const originalContent =
    replyContents[comment.commentId] ?? ''
  const content = originalContent.trim()

  if (!content) {
    ElMessage.warning('回复内容不能为空')
    return
  }

  replyingCommentId.value = comment.commentId

  try {
    await createProductComment(
      requestedProductId,
      {
        content,
        responseToId: comment.commentId
      }
    )

    if (
      !isCurrentLoad(
        currentVersion,
        requestedProductId
      )
    ) {
      return
    }

    replyContents[comment.commentId] = ''
    ElMessage.success('回复成功')

    await loadComments(
      requestedProductId,
      currentVersion
    )
  } catch (error) {
    replyContents[comment.commentId] =
      originalContent

    console.error('回复留言失败：', error)
    ElMessage.error('回复失败，请稍后重试')
  } finally {
    replyingCommentId.value = null
  }
}

async function goBack(): Promise<void> {
  await router.push({
    name: 'my-products'
  })
}

async function goToEdit(): Promise<void> {
  if (!product.value) {
    return
  }

  await router.push({
    name: 'product-edit',
    params: {
      productId: product.value.productId
    }
  })
}

function handleProductChanged(): void {
  const requestedProductId = productId.value

  if (requestedProductId === null) {
    return
  }

  void loadProduct(requestedProductId)
}

function handleProductDeleted(): void {
  void router.replace({
    name: 'my-products'
  })
}

watch(
  productId,
  () => {
    loadPage()
  },
  {
    immediate: true
  }
)

onBeforeUnmount(() => {
  loadVersion += 1
})
</script>

<template>
  <main class="seller-detail-page">
    <section class="seller-detail-container">
      <header class="page-header">
        <el-button @click="goBack">
          返回我的商品
        </el-button>

        <div class="header-actions">
          <el-button
            type="primary"
            :disabled="!product"
            @click="goToEdit"
          >
            编辑商品
          </el-button>
        </div>
      </header>

      <div
        v-if="productLoading && !product"
        class="page-state"
      >
        <el-skeleton :rows="8" animated />
      </div>

      <el-result
        v-else-if="productErrorMessage && !product"
        icon="error"
        title="无法加载商品管理详情"
        :sub-title="productErrorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            :loading="productLoading"
            @click="loadPage"
          >
            重新加载
          </el-button>

          <el-button @click="goBack">
            返回列表
          </el-button>
        </template>
      </el-result>

      <template v-else-if="product">
        <el-alert
          v-if="productErrorMessage"
          :title="productErrorMessage"
          type="error"
          :closable="false"
          show-icon
          class="section-alert"
        />

        <el-card
          class="detail-card"
          shadow="never"
        >
          <div class="product-title-row">
            <div>
              <h1>{{ product.name }}</h1>
              <p class="product-price">
                ¥{{ product.price.toFixed(2) }}
              </p>
            </div>

            <el-tag
              :type="getStatusType(product.status)"
              effect="light"
              size="large"
            >
              {{ getStatusText(product.status) }}
            </el-tag>
          </div>

          <div class="product-actions">
            <SellerProductActions
              :product-id="product.productId"
              :status="product.status"
              :disabled="productLoading"
              @changed="handleProductChanged"
              @deleted="handleProductDeleted"
            />
          </div>

          <div
            v-if="sortedImages.length > 0"
            class="image-grid"
          >
            <el-image
              v-for="image in sortedImages"
              :key="image.imgFileId"
              :src="getProductImageUrl(image.imgFileId)"
              :alt="product.name"
              fit="cover"
              class="product-image"
            >
              <template #error>
                <div class="image-placeholder">
                  图片加载失败
                </div>
              </template>
            </el-image>
          </div>

          <el-empty
            v-else
            description="该商品暂无图片"
            :image-size="90"
          />

          <dl class="detail-list">
            <div>
              <dt>分类</dt>
              <dd>{{ product.categoryName || '暂无' }}</dd>
            </div>

            <div>
              <dt>配送方式</dt>
              <dd>
                {{
                  getShippingTypeLabel(
                    product.shippingType
                  )
                }}
              </dd>
            </div>

            <div>
              <dt>固定邮费</dt>
              <dd>
                {{
                  product.shippingType === 2
                    ? `¥${Number(product.shippingFee ?? 0).toFixed(2)}`
                    : '不适用'
                }}
              </dd>
            </div>

            <div>
              <dt>校内自提</dt>
              <dd>
                {{ product.allowPickup === 1 ? '支持' : '不支持' }}
              </dd>
            </div>

            <div>
              <dt>商品编号</dt>
              <dd>{{ product.productId }}</dd>
            </div>
          </dl>

          <div class="description-block">
            <h2>商品描述</h2>
            <p>{{ product.info || '卖家暂未填写商品描述。' }}</p>
          </div>
        </el-card>

        <el-card
          class="detail-card"
          shadow="never"
        >
          <template #header>
            <div class="section-header">
              <h2>商品统计</h2>
            </div>
          </template>

          <div
            v-if="product"
            class="stats-grid"
          >
            <div class="stat-item">
              <strong>{{ product.viewCount }}</strong>
              <span>浏览量</span>
            </div>

            <div class="stat-item">
              <strong>{{ commentCount }}</strong>
              <span>留言量</span>
            </div>
          </div>
        </el-card>

        <el-card
          class="detail-card"
          shadow="never"
        >
          <template #header>
            <div class="section-header">
              <h2>留言管理</h2>

              <el-button
                text
                :loading="commentsLoading"
                @click="
                  productId !== null &&
                  loadComments(productId)
                "
              >
                刷新
              </el-button>
            </div>
          </template>

          <div
            v-if="commentsLoading && comments.length === 0"
            class="section-loading"
          >
            <el-skeleton :rows="4" animated />
          </div>

          <el-alert
            v-else-if="
              commentsErrorMessage &&
              comments.length === 0
            "
            :title="commentsErrorMessage"
            type="error"
            :closable="false"
            show-icon
          >
            <template #default>
              <el-button
                link
                type="primary"
                @click="
                  productId !== null &&
                  loadComments(productId)
                "
              >
                重新加载留言
              </el-button>
            </template>
          </el-alert>

          <el-empty
            v-else-if="comments.length === 0"
            description="当前商品还没有留言"
            :image-size="90"
          />

          <div
            v-else
            class="comment-list"
          >
            <el-alert
              v-if="commentsErrorMessage"
              :title="commentsErrorMessage"
              type="error"
              :closable="false"
              show-icon
            />

            <article
              v-for="comment in comments"
              :key="comment.commentId"
              class="comment-item"
            >
              <div class="comment-heading">
                <strong>
                  {{ comment.userName || `用户 ${comment.userId}` }}
                </strong>

                <span>
                  {{ formatCommentTime(comment.createTime) }}
                </span>
              </div>

              <p class="comment-content">
                {{ comment.content }}
              </p>

              <div
                v-if="comment.replies?.length"
                class="reply-list"
              >
                <div
                  v-for="reply in comment.replies"
                  :key="reply.commentId"
                  class="reply-item"
                >
                  <strong>
                    {{ reply.userName || `用户 ${reply.userId}` }}：
                  </strong>
                  <span>{{ reply.content }}</span>
                </div>
              </div>

              <div class="reply-editor">
                <el-input
                  v-model="replyContents[comment.commentId]"
                  type="textarea"
                  :rows="2"
                  maxlength="300"
                  show-word-limit
                  placeholder="回复这条留言"
                  :disabled="replyingCommentId !== null"
                />

                <el-button
                  type="primary"
                  :loading="
                    replyingCommentId === comment.commentId
                  "
                  :disabled="
                    replyingCommentId !== null ||
                    !replyContents[comment.commentId]?.trim()
                  "
                  @click="handleReply(comment)"
                >
                  回复
                </el-button>
              </div>
            </article>
          </div>
        </el-card>
      </template>
    </section>
  </main>
</template>

<style scoped>
.seller-detail-page {
  min-height: calc(100vh - 72px);
  padding: 30px 20px 56px;
  background: #f5f7f6;
  color: #1e2a26;
}

.seller-detail-container {
  width: min(1120px, 100%);
  margin: 0 auto;
}

.page-header,
.section-header,
.product-title-row,
.product-actions,
.comment-heading,
.reply-editor {
  display: flex;
  align-items: center;
}

.page-header {
  justify-content: space-between;
  margin-bottom: 18px;
}

.header-actions {
  display: flex;
  gap: 8px;
}

.page-state {
  padding: 34px;
  background: #fff;
  border-radius: 14px;
}

.section-alert {
  margin-bottom: 16px;
}

.detail-card {
  margin-bottom: 18px;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
  text-align: left;
}

.product-title-row {
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.product-title-row h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
  overflow-wrap: anywhere;
}

.product-price {
  margin-top: 10px;
  color: #d9544d;
  font-size: 26px;
  font-weight: 700;
}

.product-actions {
  margin-top: 20px;
}

.image-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
  margin-top: 24px;
}

.product-image {
  width: 100%;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  border-radius: 10px;
  background: #eef2f0;
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #6c7a74;
  background: #eef2f0;
}

.detail-list {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  margin: 26px 0 0;
}

.detail-list div {
  padding: 14px;
  background: #f7f9f8;
  border-radius: 10px;
}

.detail-list dt {
  color: #6c7a74;
  font-size: 13px;
}

.detail-list dd {
  margin: 7px 0 0;
  font-weight: 600;
}

.description-block {
  margin-top: 24px;
}

.description-block h2,
.section-header h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 19px;
}

.description-block p {
  margin-top: 10px;
  color: #46534d;
  line-height: 1.8;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.section-header {
  justify-content: space-between;
}

.section-loading {
  padding: 12px 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 14px;
}

.stat-item {
  display: flex;
  padding: 22px 14px;
  align-items: center;
  flex-direction: column;
  background: #eef7f3;
  border-radius: 12px;
}

.stat-item strong {
  color: #24735b;
  font-size: 26px;
}

.stat-item span {
  margin-top: 8px;
  color: #6c7a74;
}

.comment-list {
  display: grid;
  gap: 14px;
}

.comment-item {
  padding: 18px;
  border: 1px solid #e3e9e6;
  border-radius: 12px;
}

.comment-heading {
  justify-content: space-between;
  gap: 14px;
}

.comment-heading span {
  color: #7d8984;
  font-size: 12px;
}

.comment-content {
  margin-top: 12px;
  line-height: 1.7;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.reply-list {
  display: grid;
  gap: 8px;
  margin-top: 14px;
  padding: 12px;
  background: #f7f9f8;
  border-radius: 9px;
}

.reply-item {
  color: #52605a;
  font-size: 14px;
}

.reply-editor {
  align-items: flex-end;
  gap: 10px;
  margin-top: 16px;
}

.reply-editor :deep(.el-textarea) {
  flex: 1;
}

</style>
