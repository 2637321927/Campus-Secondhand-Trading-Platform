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
  sendMessage,
  sendAttachment,
  deleteMessage,
  markConversationRead
} from '../../api/modules/conversation'
import type {
  ConversationDetailDto,
  MessageDto
} from '../../types/api/conversation'
import { useProductImages } from '../../composables/useProductImages'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const messageStore = useMessageStore()

const conversationId = Number(route.params.conversationId)

const conversation = ref<ConversationDetailDto | null>(null)
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

async function loadConversation(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getConversation(conversationId)

    conversation.value = response.data
    messageStore.setCurrentConversation(response.data)

    if (response.data?.productCoverFileId) {
      await loadProductImages([response.data.productCoverFileId])
    }

    await scrollToBottom()

    void markConversationRead(conversationId).catch((error) => {
      console.warn('标记会话已读失败（后端可能未实现）：', error)
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

    if (sentMessage && conversation.value) {
      conversation.value.messages = [
        ...conversation.value.messages,
        sentMessage
      ]
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

    if (sentMessage && conversation.value) {
      conversation.value.messages = [
        ...conversation.value.messages,
        sentMessage
      ]
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

    if (conversation.value) {
      conversation.value.messages =
        conversation.value.messages.filter(
          (item) => item.messageId !== message.messageId
        )
    }

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
            {{ conversation.otherUserName?.charAt(0) ?? '对' }}
          </el-avatar>

          <div class="partner-info">
            <span class="partner-name">
              {{ conversation.otherUserName }}
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
              getProductImageUrl(conversation.productCoverFileId)
            "
            :src="
              getProductImageUrl(conversation.productCoverFileId)
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

          <span class="product-price">
            ¥{{ conversation.productPrice }}
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
              (conversation?.messages ?? []).length === 0
            "
            description="暂无消息，发送一条消息开始交流吧"
            class="messages-empty"
          />

          <div
            v-for="message in conversation?.messages ?? []"
            :key="message.messageId"
            class="message-row"
            :class="{ 'message-row--mine': isMine(message) }"
          >
            <div class="message-bubble">
              <p
                v-if="message.msgType === 0"
                class="message-content"
              >
                {{ message.content }}
              </p>

              <div
                v-else
                class="message-attachment"
              >
                <el-icon><Paperclip /></el-icon>

                <span>{{ message.fileName ?? '附件' }}</span>
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
  font-size: 14px;
  cursor: pointer;
}

.back-button:hover {
  background: #eef7f3;
}

.chat-partner {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 12px;
}

.partner-avatar {
  flex-shrink: 0;
  color: #ffffff;
  background: #3e9b79;
  font-weight: 700;
}

.partner-info {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 2px;
}

.partner-name {
  color: #1e2a26;
  font-size: 16px;
  font-weight: 600;
}

.partner-product {
  padding: 0;
  color: #6c7a74;
  background: transparent;
  border: none;
  font-size: 13px;
  text-align: left;
  cursor: pointer;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.partner-product:hover {
  color: #24735b;
}

/* 商品信息卡片 */
.product-card {
  display: flex;
  padding: 12px 20px;
  align-items: center;
  gap: 14px;
  background: #f8fbf9;
  border-bottom: 1px solid #edf1ef;
}

.product-cover {
  width: 52px;
  height: 52px;
  flex-shrink: 0;
  overflow: hidden;
  border-radius: 10px;
  background: #e3e9e6;
}

.product-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.product-cover-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #9aa6a0;
  font-size: 14px;
}

.product-info {
  display: flex;
  min-width: 0;
  flex: 1;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.product-name {
  min-width: 0;
  color: #1e2a26;
  font-size: 15px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-price {
  flex-shrink: 0;
  color: #f3a95f;
  font-size: 17px;
  font-weight: 700;
}

/* 消息区 */
.chat-body {
  display: flex;
  min-height: 0;
  flex: 1;
  flex-direction: column;
}

.messages-container {
  flex: 1;
  min-height: 0;
  padding: 20px;
  overflow-y: auto;
}

.messages-empty {
  padding: 60px 0;
}

.message-row {
  display: flex;
  margin-bottom: 16px;
  justify-content: flex-start;
}

.message-row--mine {
  justify-content: flex-end;
}

.message-bubble {
  max-width: 72%;
  padding: 10px 14px;
  background: #f2f5f3;
  border-radius: 14px;
}

.message-row--mine .message-bubble {
  background: #e3f2ec;
}

.message-content {
  margin: 0;
  color: #1e2a26;
  font-size: 15px;
  line-height: 1.6;
  overflow-wrap: anywhere;
  white-space: pre-wrap;
}

.message-attachment {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #24735b;
  font-size: 14px;
}

.message-meta {
  display: flex;
  margin-top: 4px;
  align-items: center;
  gap: 8px;
  justify-content: flex-end;
}

.message-time {
  color: #9aa6a0;
  font-size: 11px;
}

.message-delete {
  padding: 0;
  color: #d9544d;
  background: transparent;
  border: none;
  font-size: 11px;
  cursor: pointer;
}

/* 输入区 */
.chat-input {
  display: flex;
  padding: 12px 16px;
  align-items: flex-end;
  gap: 10px;
  border-top: 1px solid #edf1ef;
}

.attachment-upload {
  flex-shrink: 0;
}

.text-input {
  flex: 1;
}

.send-button {
  flex-shrink: 0;
  min-height: 40px;
  padding: 0 22px;
  border-radius: 10px;
}

@media (max-width: 640px) {
  .chat-container {
    height: calc(100vh - 72px - 24px);
  }

  .message-bubble {
    max-width: 86%;
  }
}
</style>
