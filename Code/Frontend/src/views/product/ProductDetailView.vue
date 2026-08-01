<script setup lang="ts">
import {
  computed,
  ref,
  watch,
  onBeforeUnmount
} from 'vue'
import {
  useRoute,
  useRouter
} from 'vue-router'
import { 
  ElMessage,
  ElMessageBox 
} from 'element-plus'
import { getProductDetail } from '../../api/modules/product'
import type {
  ProductDto,
  ProductStatus
} from '../../types/api/product'
import { resolveFileUrl } from '../../utils/image'
import { getPublicUser } from '../../api/modules/user'
import type { PublicUserDto } from '../../types/api/user'
import {
  getCollectionStatus,
  toggleCollection
} from '../../api/modules/collection'
import { useAuthStore } from '../../stores/auth'
import { 
  getProductComments,
  createProductComment ,
  deleteProductComment
} from '../../api/modules/comment'
import type {
  ProductCommentDto,
  CreateProductCommentRequest
} from '../../types/api/comment'
import { formatDate } from '../../utils/format'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const product = ref<ProductDto | null>(null)
const selectedImageUrl = ref('')

const authStore=useAuthStore()

const isCollected = ref(false)
const collectionLoading = ref(false)

const comments = ref<ProductCommentDto[]>([])
const commentsLoading = ref(false)
const commentsErrorMessage = ref('')
const commentContent = ref('')
const commentSubmitting = ref(false)

const replyingToComment = ref<ProductCommentDto | null>(null)
const replyContent = ref('')
const replySubmitting = ref(false)

const deletingCommentId = ref<number | null>(null)

let detailLoadVersion = 0 //详情加载版本号

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

const seller = ref<PublicUserDto | null>(null)
const sellerLoading = ref(false)
const sellerErrorMessage = ref('')

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

  return '草稿'
}

function getStatusClass(status: ProductStatus): string {
  if (status === 0) {
    return 'status-available'
  }

  if (status === 1) {
    return 'status-sold'
  }

  if (status === 2) {
    return 'status-removed'
  }

  return 'status-draft'
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

async function handleFavorite(): Promise<void> {
  if(!product.value){
    return
  }

  if(!authStore.isLoggedIn){
    ElMessage.warning('请先登录之后再收藏商品')

    await router.push({
      name:'login',
      query:{
        redirect:route.fullPath
      }
    })

    return
  }

  if(authStore.currentUser?.userId===product.value.userId){
    ElMessage.warning('不能收藏自己发布的商品')
    return
  }

  collectionLoading.value=true

  try{
    const response=await toggleCollection(product.value.productId)
    isCollected.value=response.data.isCollected

    ElMessage.success(
      isCollected.value ? '收藏成功' : '已取消收藏'
    )
  }
  catch(error){
    console.error('收藏操作失败：', error)
  }
  finally{
    collectionLoading.value=false
  }
}

async function loadComments(
  requestedProductId: number,
  version = detailLoadVersion
): Promise<void> {
  commentsLoading.value = true
  commentsErrorMessage.value = ''

  try {
    const response = await getProductComments(
      requestedProductId
    )

    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    comments.value = response.data ?? []
  } catch (error) {
    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    comments.value = []
    commentsErrorMessage.value =
      '留言加载失败，请稍后重试'

    console.error('留言加载失败：', error)
  } finally {
    if (
      isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      commentsLoading.value = false
    }
  }
}


async function handleSubmitComment():Promise<void> {
  if(!product.value){
    return
  }

  if(!authStore.isLoggedIn){
    ElMessage.warning('请先登陆后再发表留言')

    await router.push({
      name: 'login',
      query: {
        redirect: route.fullPath
      }
    })

    return
  }

  const content=commentContent.value

  if(!content){
    ElMessage.warning('请输入留言信息')
    return
  }

  const requestData: CreateProductCommentRequest={
    content
  }

  commentSubmitting.value=true

  try{
    await createProductComment(
      product.value.productId,
      requestData
    )
    commentContent.value=''
    ElMessage.success('留言发表成功')

    await loadComments(product.value.productId)
  }
  catch(error){
    console.error('留言发表失败',error)
  }
  finally{
    commentSubmitting.value=false
  }
}

async function handleStartReply(
  comment: ProductCommentDto
): Promise<void> {
  if (!authStore.isLoggedIn) {
    ElMessage.warning('请先登录后再回复留言')

    await router.push({
      name: 'login',
      query: {
        redirect: route.fullPath
      }
    })

    return
  }

  replyingToComment.value = comment
  replyContent.value = ''
}

function handleCancelReply(): void {
  replyingToComment.value = null
  replyContent.value = ''
}

async function handleSubmitReply(): Promise<void> {
  if (!product.value || !replyingToComment.value) {
    return
  }

  if (!authStore.isLoggedIn) {
    ElMessage.warning('请先登录后再回复留言')
    return
  }

  const content = replyContent.value

  if (!content) {
    ElMessage.warning('请输入回复内容')
    return
  }

  const requestData: CreateProductCommentRequest = {
    content,
    responseToId: replyingToComment.value.commentId
  }

  replySubmitting.value = true

  try {
    await createProductComment(
      product.value.productId,
      requestData
    )

    ElMessage.success('回复发表成功')

    replyingToComment.value = null
    replyContent.value = ''

    await loadComments(product.value.productId)
  } catch (error) {
    console.error('回复留言失败：', error)
  } finally {
    replySubmitting.value = false
  }
}

async function handleDeleteComment(
  comment: ProductCommentDto
): Promise<void> {
  if (!product.value) {
    return
  }

  if (!authStore.isLoggedIn) {
    ElMessage.warning('请先登录')
    return
  }

  if (comment.canDelete !== true) {
    ElMessage.warning('你无权删除这条留言')
    return
  }

  try {
    await ElMessageBox.confirm(
      '删除后无法恢复，确定要删除这条留言吗？',
      '删除留言',
      {
        confirmButtonText: '确认删除',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
  } 
  catch {
    return
  }

  deletingCommentId.value = comment.commentId

  try {
    await deleteProductComment(
      product.value.productId,
      comment.commentId
    )

    ElMessage.success('留言已删除')

    if (
      replyingToComment.value?.commentId ===
      comment.commentId
    ) {
      handleCancelReply()
    }

    await loadComments(product.value.productId)
  } 
  catch (error) {
    console.error('删除留言失败：', error)
  } 
  finally {
    deletingCommentId.value = null
  }
}

function isCurrentDetailLoad(
  version: number,
  requestedProductId: number
): boolean {
  return (
    version === detailLoadVersion &&
    productId.value === requestedProductId
  )
}

async function loadSeller(
  userId: number,
  requestedProductId = productId.value,
  version = detailLoadVersion
): Promise<void> {
  if (requestedProductId === null) {
    return
  }

  sellerLoading.value = true
  sellerErrorMessage.value = ''

  try {
    const publicUser = await getPublicUser(userId)

    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    seller.value = publicUser
  } catch (error) {
    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    sellerErrorMessage.value =
      '卖家信息加载失败'

    console.error('卖家信息加载失败：', error)
  } finally {
    if (
      isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      sellerLoading.value = false
    }
  }
}

async function loadCollectionStatus(
  requestedProductId: number,
  version = detailLoadVersion
): Promise<void> {
  if (!authStore.isLoggedIn) {
    if (
      isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      isCollected.value = false
    }

    return
  }

  try {
    const response = await getCollectionStatus(
      requestedProductId
    )

    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    isCollected.value =
      response.data.isCollected
  } catch (error) {
    if (
      !isCurrentDetailLoad(
        version,
        requestedProductId
      )
    ) {
      return
    }

    isCollected.value = false

    console.error('收藏状态加载失败：', error)
  }
}

async function loadProduct(): Promise<void> {
  const requestedProductId = productId.value
  const currentVersion = ++detailLoadVersion

  loading.value = true
  errorMessage.value = ''

  product.value = null
  seller.value = null
  comments.value = []
  selectedImageUrl.value = ''
  isCollected.value = false

  replyingToComment.value = null
  replyContent.value = ''
  commentContent.value = ''

  if (requestedProductId === null) {
    errorMessage.value = '商品编号不正确'
    loading.value = false
    return
  }

  try {
    const response = await getProductDetail(
      requestedProductId
    )

    if (
      !isCurrentDetailLoad(
        currentVersion,
        requestedProductId
      )
    ) {
      return
    }

    product.value = response.data

    const images = [
      ...(response.data.images ?? [])
    ].sort(
      (a, b) => a.imgIndex - b.imgIndex
    )

    const firstImage = images[0]

    selectedImageUrl.value = firstImage
      ? resolveFileUrl(firstImage.imgFileId)
      : ''

    void loadSeller(
      response.data.userId,
      requestedProductId,
      currentVersion
    )

    void loadCollectionStatus(
      requestedProductId,
      currentVersion
    )

    void loadComments(
      requestedProductId,
      currentVersion
    )
  } catch (error) {
    if (
      !isCurrentDetailLoad(
        currentVersion,
        requestedProductId
      )
    ) {
      return
    }

    errorMessage.value =
      '商品详情加载失败，请稍后重试'

    console.error('商品详情加载失败：', error)
  } finally {
    if (
      isCurrentDetailLoad(
        currentVersion,
        requestedProductId
      )
    ) {
      loading.value = false
    }
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

onBeforeUnmount(() => {
  detailLoadVersion += 1
})

</script>

<template>
  <div class="product-detail-page">
    <!-- 页面加载状态 -->
    <div
      v-if="loading"
      class="detail-state"
    >
      <div class="state-loading"></div>

      <h2>商品详情加载中</h2>

      <p>正在获取商品信息，请稍候...</p>
    </div>

    <!-- 页面错误状态 -->
    <div
      v-else-if="errorMessage"
      class="detail-state error-state"
    >
      <div class="state-symbol">
        !
      </div>

      <h2>商品加载失败</h2>

      <p>{{ errorMessage }}</p>

      <div class="state-actions">
        <el-button
          type="primary"
          @click="loadProduct"
        >
          重新加载
        </el-button>

        <el-button @click="router.push('/products')">
          返回商品列表
        </el-button>
      </div>
    </div>

    <!-- 商品详情主体 -->
    <div
      v-else-if="product"
      class="detail-content"
    >
      <!-- 返回商品列表 -->
      <div class="back-row">
        <el-button
          text
          @click="router.push('/products')"
        >
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
              :preview-src-list="
                sortedImages.map((image) =>
                  resolveFileUrl(image.imgFileId)
                )
              "
            >
              <template #error>
                <div class="image-placeholder">
                  <span class="placeholder-icon">
                    图
                  </span>

                  <span>图片加载失败</span>
                </div>
              </template>
            </el-image>

            <div
              v-else
              class="image-placeholder"
            >
              <span class="placeholder-icon">
                图
              </span>

              <span>暂无商品图片</span>
            </div>
          </div>

          <!-- 商品缩略图 -->
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

        <!-- 右侧商品概要 -->
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

          <!-- 商品价格 -->
          <div class="price-box">
            <span class="price-label">
              商品价格
            </span>

            <p class="product-price">
              <span class="currency">¥</span>
              {{ product.price.toFixed(2) }}
            </p>
          </div>

          <!-- 商品基础信息 -->
          <div class="product-meta">
            <div class="meta-item">
              <span class="meta-label">
                商品分类
              </span>

              <span class="meta-value">
                {{ product.categoryName ?? '未分类' }}
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">
                浏览次数
              </span>

              <span class="meta-value">
                {{ product.viewCount }} 次
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">
                卖家编号
              </span>

              <span class="meta-value">
                {{ product.userId }}
              </span>
            </div>

            <div class="meta-item">
              <span class="meta-label">
                交易状态
              </span>

              <span class="meta-value">
                {{ getStatusText(product.status) }}
              </span>
            </div>
          </div>

          <!-- 商品操作 -->
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
              :loading="collectionLoading"
              :disabled="
                collectionLoading ||
                authStore.currentUser?.userId === product.userId
              "
              @click="handleFavorite"
            >
              <template
                v-if="
                  authStore.currentUser?.userId === product.userId
                "
              >
                自己发布的商品
              </template>

              <template v-else>
                {{ isCollected ? '取消收藏' : '收藏商品' }}
              </template>
            </el-button>
          </div>

          <!-- 交易安全提示 -->
          <div class="trade-notice">
            <h3>交易提醒</h3>

            <ul>
              <li>
                建议优先选择校内公共场所当面交易。
              </li>

              <li>
                交易前请仔细确认商品实际情况。
              </li>

              <li>
                请勿脱离平台进行可疑转账或付款。
              </li>
            </ul>
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

        <p
          v-if="product.info"
          class="description-content"
        >
          {{ product.info }}
        </p>

        <div
          v-else
          class="empty-description"
        >
          卖家暂未填写商品描述
        </div>
      </section>

      <!-- 卖家信息 -->
      <section class="detail-section seller-section">
        <div class="section-title">
          <div>
            <h2>卖家信息</h2>

            <span>查看商品发布者的公开信息</span>
          </div>
        </div>

        <!-- 卖家加载中 -->
        <el-skeleton
          v-if="sellerLoading"
          class="seller-skeleton"
          :rows="2"
          animated
        />

        <!-- 卖家加载失败 -->
        <div
          v-else-if="sellerErrorMessage"
          class="seller-error"
        >
          <div>
            <strong>卖家信息加载失败</strong>

            <p>{{ sellerErrorMessage }}</p>
          </div>

          <el-button
            type="primary"
            plain
            @click="loadSeller(product.userId)"
          >
            重新加载
          </el-button>
        </div>

        <!-- 卖家信息 -->
        <div
          v-else-if="seller"
          class="seller-card"
        >
          <el-avatar
            :size="56"
            class="seller-avatar"
          >
            {{ seller.userName?.slice(0, 1) || '卖' }}
          </el-avatar>

          <div class="seller-info">
            <strong>
              {{ seller.userName || `卖家 ${seller.userId}` }}
            </strong>

            <span>
              用户编号：{{ seller.userId }}
            </span>
          </div>

          <el-button @click="handleContactSeller">
            联系卖家
          </el-button>
        </div>

        <!-- 卖家信息为空 -->
        <div
          v-else
          class="seller-empty"
        >
          暂无卖家公开信息
        </div>
      </section>

      <!-- 买家留言 -->
      <section class="detail-section comment-section">
        <div class="section-title comment-title">
          <div>
            <h2>买家留言</h2>

            <span>
              查看其他用户对该商品的留言和回复
            </span>
          </div>

          <span class="comment-count">
            共 {{ comments.length }} 条
          </span>
        </div>

        <!-- 发表留言 -->
        <div class="comment-composer">
          <template v-if="authStore.isLoggedIn">
            <div class="composer-user">
              <el-avatar
                :size="40"
                class="composer-avatar"
              >
                {{
                  authStore.currentUser?.userName
                    ?.slice(0, 1) || '我'
                }}
              </el-avatar>

              <div class="composer-input">
                <el-input
                  v-model="commentContent"
                  type="textarea"
                  :rows="3"
                  resize="none"
                  placeholder="向卖家咨询商品成色、交易地点等信息"
                  :disabled="commentSubmitting"
                  @keydown.ctrl.enter.prevent="handleSubmitComment"
                />

                <div class="composer-footer">
                  <el-button
                    type="primary"
                    :loading="commentSubmitting"
                    :disabled="
                      commentSubmitting ||
                      !commentContent.trim()
                    "
                    @click="handleSubmitComment"
                  >
                    发表留言
                  </el-button>
                </div>
              </div>
            </div>
          </template>

          <div
            v-else
            class="comment-login-tip"
          >
            <div>
              <strong>登录后参与留言</strong>
            </div>

            <el-button
              type="primary"
              plain
              @click="
                router.push({
                  name: 'login',
                  query: {
                    redirect: route.fullPath
                  }
                })
              "
            >
              前往登录
            </el-button>
          </div>
        </div>

        <!-- 留言加载中 -->
        <div
          v-if="commentsLoading"
          class="comments-loading"
        >
          <el-skeleton
            :rows="4"
            animated
          />
        </div>

        <!-- 留言加载失败 -->
        <div
          v-else-if="commentsErrorMessage"
          class="comments-error"
        >
          <div class="comments-error-symbol">
            !
          </div>

          <div class="comments-error-content">
            <strong>留言加载失败</strong>

            <p>{{ commentsErrorMessage }}</p>
          </div>

          <el-button
            type="primary"
            plain
            @click="loadComments(product.productId)"
          >
            重新加载
          </el-button>
        </div>

        <!-- 暂无留言 -->
        <el-empty
          v-else-if="comments.length === 0"
          class="comments-empty"
          description="暂时还没有留言"
        >
          <template #image>
            <div class="empty-comment-icon">
              留
            </div>
          </template>

          <p class="empty-comment-tip">
            登录后可以向卖家咨询商品情况
          </p>
        </el-empty>

        <!-- 留言列表 -->
        <div
          v-else
          class="comment-list"
        >
          <article
            v-for="comment in comments"
            :key="comment.commentId"
            class="comment-item"
          >
            <!-- 留言用户头像 -->
            <el-avatar
              :size="44"
              class="comment-avatar"
            >
              {{ comment.userName?.slice(0, 1) || '用' }}
            </el-avatar>

            <!-- 留言主体 -->
            <div class="comment-body">
              <div class="comment-header">
                <div class="comment-user">
                  <strong>
                    {{ comment.userName || '匿名用户' }}
                  </strong>

                  <span
                    v-if="comment.responseToId === null"
                    class="comment-type"
                  >
                    留言
                  </span>
                </div>

                <time class="comment-time">
                  {{ formatDate(comment.createTime) }}
                </time>
              </div>

              <p class="comment-text">
                {{ comment.content }}
              </p>

              <!-- 留言操作 -->
              <div class="comment-actions">
                <el-button
                  text
                  type="primary"
                  @click="handleStartReply(comment)"
                >
                  回复
                </el-button>

                 <el-button
                  v-if="comment.canDelete === true"
                  text
                  type="danger"
                  :loading="
                  deletingCommentId === comment.commentId
                  "
                  :disabled="deletingCommentId !== null"
                  @click="handleDeleteComment(comment)"
                  >
                    删除
                  </el-button>
              </div>

              <!-- 行内回复输入框 -->
              <div
                v-if="
                  replyingToComment?.commentId ===
                  comment.commentId
                "
                class="reply-composer"
              >
                <div class="reply-target">
                  正在回复：
                  <strong>
                    {{ comment.userName || '匿名用户' }}
                  </strong>
                </div>

                <el-input
                  v-model="replyContent"
                  type="textarea"
                  :rows="2"
                  resize="none"
                  maxlength="300"
                  show-word-limit
                  placeholder="输入回复内容"
                  :disabled="replySubmitting"
                  @keydown.ctrl.enter.prevent="handleSubmitReply"
                />

                <div class="reply-composer-actions">
                  <span>
                    按 Ctrl + Enter 快速回复
                  </span>

                  <div>
                    <el-button
                      :disabled="replySubmitting"
                      @click="handleCancelReply"
                    >
                      取消
                    </el-button>

                    <el-button
                      type="primary"
                      :loading="replySubmitting"
                      :disabled="
                        replySubmitting ||
                        !replyContent.trim()
                      "
                      @click="handleSubmitReply"
                    >
                      发表回复
                    </el-button>
                  </div>
                </div>
              </div>

              <!-- 回复列表 -->
              <div
                v-if="(comment.replies?.length ?? 0) > 0"
                class="reply-list"
              >
                <article
                  v-for="reply in comment.replies ?? []"
                  :key="reply.commentId"
                  class="reply-item"
                >
                  <el-avatar
                    :size="34"
                    class="reply-avatar"
                  >
                    {{ reply.userName?.slice(0, 1) || '用' }}
                  </el-avatar>

                  <div class="reply-body">
                    <div class="reply-header">
                      <div class="reply-user">
                        <strong>
                          {{ reply.userName || '匿名用户' }}
                        </strong>

                        <span>回复</span>
                      </div>

                      <time class="reply-time">
                        {{ formatDate(reply.createTime) }}
                      </time>
                    </div>

                    <p class="reply-text">
                      {{ reply.content }}
                    </p>

                    <div
                      v-if="reply.canDelete === true"
                      class="reply-actions"
                    >
                      <el-button
                        text
                        type="danger"
                        :loading="
                          deletingCommentId === reply.commentId
                        "
                        :disabled="deletingCommentId !== null"
                        @click="handleDeleteComment(reply)"
                      >
                        删除回复
                      </el-button>
                    </div>
                  </div>
                </article>
              </div>
            </div>
          </article>
        </div>
      </section>

      <!-- 交易须知 -->
      <section class="detail-section transaction-section">
        <div class="section-title">
          <div>
            <h2>交易须知</h2>

            <span>校园二手交易安全提示</span>
          </div>
        </div>

        <div class="transaction-grid">
          <div class="transaction-item">
            <span class="transaction-number">
              01
            </span>

            <div>
              <strong>当面验货</strong>

              <p>
                建议在校内公共场所见面，并在付款前仔细检查商品。
              </p>
            </div>
          </div>

          <div class="transaction-item">
            <span class="transaction-number">
              02
            </span>

            <div>
              <strong>谨慎付款</strong>

              <p>
                不要点击不明链接，不要向陌生账户提前支付大额款项。
              </p>
            </div>
          </div>

          <div class="transaction-item">
            <span class="transaction-number">
              03
            </span>

            <div>
              <strong>保留记录</strong>

              <p>
                重要约定应尽量通过平台消息完成，以便发生争议时核查。
              </p>
            </div>
          </div>
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

.comment-section {
  min-height: 180px;
}

.comment-error {
  padding: 32px;
  text-align: center;
}

.comment-error p {
  margin: 0 0 16px;
  color: #6c7a74;
}

.comment-list {
  display: grid;
}

.comment-item {
  display: flex;
  gap: 14px;
  padding: 20px 0;
  border-bottom: 1px solid #e3e9e6;
}

.comment-item:last-child {
  border-bottom: 0;
}

.comment-content {
  flex: 1;
  min-width: 0;
}

.comment-header,
.reply-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.comment-header strong,
.reply-header strong {
  color: #1e2a26;
}

.comment-header span,
.reply-header span {
  color: #6c7a74;
  font-size: 13px;
}

.comment-content > p,
.reply-item p {
  margin: 10px 0 0;
  color: #1e2a26;
  line-height: 1.7;
  white-space: pre-wrap;
  word-break: break-word;
}

.reply-list {
  display: grid;
  gap: 12px;
  margin-top: 14px;
}

.reply-item {
  padding: 14px 16px;
  border-radius: 12px;
  background: #f5f7f6;
}

.comment-composer {
  margin-top: 22px;
  padding: 18px;
  background: #f7f9f8;
  border: 1px solid #e5ebe8;
  border-radius: 14px;
}

.composer-user {
  display: flex;
  align-items: flex-start;
  gap: 14px;
}

.composer-avatar {
  flex: 0 0 40px;
  color: #ffffff;
  background: #3e9b79;
  font-weight: 600;
}

.composer-input {
  min-width: 0;
  flex: 1;
}

.composer-input :deep(.el-textarea__inner) {
  padding: 12px 14px;
  color: #34443d;
  background: #ffffff;
  border-radius: 10px;
  line-height: 1.7;
  box-shadow: 0 0 0 1px #dce5e1 inset;
}

.composer-input :deep(.el-textarea__inner:focus) {
  box-shadow: 0 0 0 1px #3e9b79 inset;
}

.composer-footer {
  display: flex;
  margin-top: 12px;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.composer-footer span {
  color: #909b96;
  font-size: 12px;
}

.comment-login-tip {
  display: flex;
  margin-top: 22px;
  padding: 18px 20px;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  background: #f2f8f5;
  border: 1px solid #dceae4;
  border-radius: 14px;
}

.comment-login-tip strong {
  color: #304039;
}

.comment-login-tip p {
  margin: 6px 0 0;
  color: #78857f;
  font-size: 13px;
}

.comment-actions {
  display: flex;
  margin-top: 8px;
  align-items: center;
}

.comment-actions :deep(.el-button) {
  height: auto;
  padding: 4px 0;
}

.reply-composer {
  margin-top: 14px;
  padding: 14px;
  background: #f6f9f7;
  border: 1px solid #dfe9e4;
  border-radius: 12px;
}

.reply-target {
  margin-bottom: 10px;
  color: #74817b;
  font-size: 13px;
}

.reply-target strong {
  color: #24735b;
}

.reply-composer :deep(.el-textarea__inner) {
  padding: 11px 13px;
  line-height: 1.7;
  border-radius: 10px;
  box-shadow: 0 0 0 1px #dce5e1 inset;
}

.reply-composer :deep(.el-textarea__inner:focus) {
  box-shadow: 0 0 0 1px #3e9b79 inset;
}

.reply-composer-actions {
  display: flex;
  margin-top: 12px;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.reply-composer-actions > span {
  color: #929d98;
  font-size: 12px;
}

.reply-composer-actions > div {
  display: flex;
  gap: 10px;
}

.comment-actions {
  display: flex;
  margin-top: 8px;
  align-items: center;
  gap: 12px;
}

.comment-actions :deep(.el-button) {
  height: auto;
  margin-left: 0;
  padding: 4px 0;
}

.reply-actions {
  display: flex;
  margin-top: 6px;
  justify-content: flex-end;
}

.reply-actions :deep(.el-button) {
  height: auto;
  margin-left: 0;
  padding: 3px 0;
  font-size: 12px;
}
</style>
