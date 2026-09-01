<script setup lang="ts">
import {
  computed,
  nextTick,
  onMounted,
  ref
} from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { UploadRequestOptions } from 'element-plus'
import { Paperclip } from '@element-plus/icons-vue'
import { useAuthStore } from '../../stores/auth'
import { useMessageStore } from '../../stores/message'
import {
  getConversation,
  getConversationMessages,
  sendMessage,
  sendAttachment,
  deleteMessage,
  markConversationRead
} from '../../api/modules/conversation'
import { getUserById } from '../../api/modules/user'
import { getProductDetail } from '../../api/modules/product'
import type {
  ConversationDto,
  MessageDto
} from '../../types/api/conversation'
import { useProductImages } from '../../composables/useProductImages'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const messageStore = useMessageStore()

const conversationId = Number(route.params.conversationId)

const conversation = ref<ConversationDto | null>(null)
const messages = ref<MessageDto[]>([])
/** 对方用户名（后端会话接口不含用户信息，按对方 ID 单独请求） */
const otherUserName = ref('')
const otherUserId = ref<number | null>(null)
/** 商品价格与封面（后端会话接口不含，按商品 ID 单独请求） */
const productPrice = ref<number | null>(null)
const productCoverFileId = ref<number | null>(null)

const loading = ref(false)
const errorMessage = ref('')
const sending = ref(false)
const uploading = ref(false)

const inputText = ref('')
const messagesContainer = ref<HTMLElement>()

const { getProductImageUrl, loadProductImages } =
  useProductImages()

const currentUserId = computed(
  () => authStore.currentUser?.userId
)

/** 对方 = 会话双方中不是当前用户的那一方 */
function resolveOtherUserId(c: ConversationDto): number | null {
  const uid = currentUserId.value

  if (uid === c.buyerId) return c.sellerId
  if (uid === c.sellerId) return c.buyerId

  // 当前用户不在会话双方中（异常情况），退化为卖家
  return c.sellerId
}

function isMine(message: MessageDto): boolean {
  return (
    currentUserId.value !== undefined &&
    message.senderId === currentUserId.value
  )
}

function formatMessageTime(value?: string): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return date.toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

async function scrollToBottom(): Promise<void> {
  await nextTick()

  if (messagesContainer.value) {
    messagesContainer.value.scrollTop =
      messagesContainer.value.scrollHeight
  }
}

/**
 * 加载会话详情、消息记录、对方用户名、商品摘要。
 * 后端各接口独立返回，这里并行请求拼装。
 */
async function loadConversation(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const detailResponse = await getConversation(conversationId)

    conversation.value = detailResponse.data
    messageStore.setCurrentConversation(detailResponse.data)

    if (conversation.value) {
      const otherId = resolveOtherUserId(conversation.value)
      otherUserId.value = otherId

      // 并行加载：消息记录、对方用户名、商品详情
      const [messagesResult, otherUserResult, productResult] =
        await Promise.allSettled([
          getConversationMessages(conversationId),
          otherId !== null ? getUserById(otherId) : Promise.reject(new Error('无对方 ID')),
          getProductDetail(conversation.value.productId)
        ])

      if (messagesResult.status === 'fulfilled') {
        messages.value = messagesResult.value.data ?? []
      } else {
        messages.value = []
        console.warn('消息记录加载失败：', messagesResult.reason)
      }

      if (otherUserResult.status === 'fulfilled') {
        otherUserName.value = otherUserResult.value.data.userName
      } else {
        console.warn('对方用户信息加载失败：', otherUserResult.reason)
      }

      if (productResult.status === 'fulfilled') {
        const product = productResult.value.data
        productPrice.value = product.price
        productCoverFileId.value = product.images?.[0]?.imgFileId ?? null

        if (productCoverFileId.value) {
          await loadProductImages([productCoverFileId.value])
        }
      } else {
        console.warn('商品信息加载失败：', productResult.reason)
      }
    }

    await scrollToBottom()

    void markConversationRead(conversationId).catch((error) => {
      console.warn('标记会话已读失败：', error)
    })

    void messageStore.loadUnreadCount()
  } catch (error) {
    errorMessage.value = '会话加载失败，请稍后重试'

    console.error('会话加载失败：', error)
  } finally {
    loading.value = false
  }
}

async function handleSend(): Promise<void> {
  const content = inputText.value.trim()

  if (!content || sending.value) {
    return
  }

  sending.value = true

  try {
    const response = await sendMessage(conversationId, {
      content
    })

    const sentMessage = response.data

    if (sentMessage) {
      messages.value = [...messages.value, sentMessage]
    }

    inputText.value = ''

    await scrollToBottom()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '消息发送失败，请稍后重试')
    )

    console.error('消息发送失败：', error)
  } finally {
    sending.value = false
  }
}

async function handleUploadAttachment(
  options: UploadRequestOptions
): Promise<void> {
  if (uploading.value) {
    return
  }

  const file = options.file

  uploading.value = true

  try {
    const response = await sendAttachment(conversationId, file)

    const sentMessage = response.data

    if (sentMessage) {
      messages.value = [...messages.value, sentMessage]
    }

    ElMessage.success('附件已发送')

    await scrollToBottom()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '附件发送失败，请稍后重试')
    )

    console.error('附件发送失败：', error)
  } finally {
    uploading.value = false
  }
}

async function handleDeleteMessage(
  message: MessageDto
): Promise<void> {
  try {
    await deleteMessage(conversationId, message.messageId)

    messages.value = messages.value.filter(
      (item) => item.messageId !== message.messageId
    )

    ElMessage.success('消息已删除')
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '删除消息失败，请稍后重试')
    )

    console.error('删除消息失败：', error)
  }
}

function goBack(): void {
  void router.push({ name: 'message-list' })
}

function goToProduct(): void {
  if (conversation.value?.productId) {
    void router.push({
      name: 'product-detail',
      params: {
        productId: String(conversation.value.productId)
      }
    })
  }
}

onMounted(() => {
  void loadConversation()
})
</script>

<template>
  <main class="chat-page">
    <div class="chat-container">
      <!-- 会话头部 -->
      <header class="chat-header">
        <button
          class="back-button"
          type="button"
          @click="goBack"
        >
          返回消息
        </button>

        <div
          v-if="conversation"
          class="chat-partner"
        >
          <el-avatar
            class="partner-avatar"
            :size="40"
          >
            {{ otherUserName?.charAt(0) ?? '对' }}
          </el-avatar>

          <div class="partner-info">
            <span class="partner-name">
              {{ otherUserName || `用户 #${otherUserId ?? '?'}` }}
            </span>

            <button
              class="partner-product"
              type="button"
              @click="goToProduct"
            >
              关于商品：{{ conversation.productName }}
            </button>
          </div>
        </div>
      </header>

      <!-- 商品信息卡片 -->
      <div
        v-if="conversation"
        class="product-card"
      >
        <div class="product-cover">
          <img
            v-if="
              getProductImageUrl(productCoverFileId)
            "
            :src="
              getProductImageUrl(productCoverFileId)
            "
            alt="商品图片"
          />

          <span
            v-else
            class="product-cover-placeholder"
          >
            图
          </span>
        </div>

        <div class="product-info">
          <span class="product-name">
            {{ conversation.productName }}
          </span>

          <span
            v-if="productPrice !== null"
            class="product-price"
          >
            ¥{{ productPrice }}
          </span>
        </div>
      </div>

      <!-- 加载 / 错误 -->
      <el-result
        v-if="errorMessage"
        icon="error"
        title="会话加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadConversation"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <div
        v-else
        class="chat-body"
        v-loading="loading"
      >
        <!-- 消息记录 -->
        <div
          ref="messagesContainer"
          class="messages-container"
        >
          <el-empty
            v-if="
              !loading &&
              messages.length === 0
            "
            description="暂无消息，发送一条消息开始交流吧"
            class="messages-empty"
          />

          <div
            v-for="message in messages"
            :key="message.messageId"
            class="message-row"
            :class="{ 'message-row--mine': isMine(message) }"
          >
            <div class="message-bubble">
              <p
                v-if="message.messageType === 0"
                class="message-content"
              >
                {{ message.content }}
              </p>

              <div
                v-else
                class="message-attachment"
              >
                <img
                  v-if="getProductImageUrl(message.fileId)"
                  class="message-image"
                  :src="getProductImageUrl(message.fileId)"
                  alt="图片消息"
                />

                <template v-else>
                  <el-icon><Paperclip /></el-icon>

                  <span>图片</span>
                </template>
              </div>

              <div class="message-meta">
                <span class="message-time">
                  {{ formatMessageTime(message.sendTime) }}
                </span>

                <button
                  v-if="isMine(message)"
                  class="message-delete"
                  type="button"
                  @click="handleDeleteMessage(message)"
                >
                  撤回
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- 输入区 -->
        <footer class="chat-input">
          <el-upload
            class="attachment-upload"
            :show-file-list="false"
            :http-request="handleUploadAttachment"
            :disabled="uploading"
          >
            <el-button
              text
              :loading="uploading"
              aria-label="发送附件"
            >
              <el-icon><Paperclip /></el-icon>
            </el-button>
          </el-upload>

          <el-input
            v-model="inputText"
            class="text-input"
            type="textarea"
            :rows="2"
            maxlength="1000"
            placeholder="输入消息，回车发送"
            @keydown.enter.exact.prevent="handleSend"
          />

          <el-button
            class="send-button"
            type="primary"
            :loading="sending"
            :disabled="!inputText.trim()"
            @click="handleSend"
          >
            发送
          </el-button>
        </footer>
      </div>
    </div>
  </main>
</template>

<style scoped>
.chat-page {
  min-height: calc(100vh - 72px);
  padding: 24px;
  background: #f5f7f6;
  color: #1e2a26;
}

.chat-container {
  display: flex;
  width: 100%;
  max-width: 860px;
  height: calc(100vh - 72px - 48px);
  margin: 0 auto;
  flex-direction: column;
  overflow: hidden;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.chat-header {
  display: flex;
  padding: 14px 20px;
  align-items: center;
  gap: 14px;
  border-bottom: 1px solid #edf1ef;
}

.back-button {
  padding: 8px 12px;
  color: #24735b;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.back-button:hover {
  background: #eef4f2;
}

.chat-partner {
  display: flex;
  align-items: center;
  gap: 12px;
}

.partner-avatar {
  background: #24735b;
  color: #ffffff;
  font-weight: 600;
}

.partner-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.partner-name {
  font-size: 15px;
  font-weight: 600;
}

.partner-product {
  padding: 0;
  color: #6b7f78;
  font-size: 12px;
  text-align: left;
  background: transparent;
  border: none;
  cursor: pointer;
}

.partner-product:hover {
  color: #24735b;
}

.product-card {
  display: flex;
  padding: 12px 20px;
  align-items: center;
  gap: 12px;
  background: #f7faf9;
  border-bottom: 1px solid #edf1ef;
}

.product-cover {
  display: flex;
  width: 52px;
  height: 52px;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background: #e8efec;
  border-radius: 10px;
}

.product-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-cover-placeholder {
  color: #9db4ad;
  font-size: 14px;
}

.product-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.product-name {
  font-size: 14px;
  font-weight: 500;
}

.product-price {
  color: #d64b3f;
  font-size: 14px;
  font-weight: 600;
}

.chat-body {
  display: flex;
  min-height: 0;
  flex: 1;
  flex-direction: column;
}

.messages-container {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
  background: #ffffff;
}

.messages-empty {
  margin-top: 48px;
}

.message-row {
  display: flex;
  margin-bottom: 14px;
}

.message-row--mine {
  justify-content: flex-end;
}

.message-bubble {
  max-width: 70%;
  padding: 10px 14px;
  background: #f2f5f4;
  border-radius: 14px;
}

.message-row--mine .message-bubble {
  background: #d8ede6;
}

.message-content {
  margin: 0;
  font-size: 14px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

.message-attachment {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #556761;
  font-size: 13px;
}

.message-image {
  max-width: 220px;
  max-height: 220px;
  border-radius: 8px;
}

.message-meta {
  display: flex;
  margin-top: 6px;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
}

.message-time {
  color: #93a39e;
  font-size: 11px;
}

.message-delete {
  padding: 0;
  color: #b0685f;
  font-size: 11px;
  background: transparent;
  border: none;
  cursor: pointer;
}

.message-delete:hover {
  color: #d64b3f;
}

.chat-input {
  display: flex;
  padding: 14px 20px;
  align-items: flex-end;
  gap: 12px;
  border-top: 1px solid #edf1ef;
}

.attachment-upload {
  line-height: 1;
}

.text-input {
  flex: 1;
}

.send-button {
  background: #24735b;
  border-color: #24735b;
}
</style>
