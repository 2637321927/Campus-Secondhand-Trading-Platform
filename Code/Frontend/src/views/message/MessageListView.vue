<script setup lang="ts">
import {
  onMounted,
  ref,
  watch
} from 'vue'
import { useRouter } from 'vue-router'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import { Search } from '@element-plus/icons-vue'
import { useMessageStore } from '../../stores/message'
import {
  deleteConversation
} from '../../api/modules/conversation'
import {
  getNotifications,
  deleteNotification
} from '../../api/modules/notification'
import type { ConversationDto } from '../../types/api/conversation'
import type { NotificationDto } from '../../types/api/notification'
import { getApiErrorMessage } from '../../utils/error'

const router = useRouter()
const messageStore = useMessageStore()

const activeTab = ref('conversation')

// ===== 会话 =====
const keyword = ref('')
const conversations = ref<ConversationDto[]>([])
const loadingConversations = ref(false)
const conversationsError = ref('')

const deletingConversationIds = ref<number[]>([])

// ===== 通知 =====
const notifications = ref<NotificationDto[]>([])
const loadingNotifications = ref(false)
const notificationsError = ref('')
const deletingNotificationIds = ref<string[]>([])

function formatTime(value?: string): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  const now = new Date()
  const isSameDay =
    date.getFullYear() === now.getFullYear() &&
    date.getMonth() === now.getMonth() &&
    date.getDate() === now.getDate()

  const options: Intl.DateTimeFormatOptions = isSameDay
    ? { hour: '2-digit', minute: '2-digit' }
    : { month: '2-digit', day: '2-digit' }

  return date.toLocaleString('zh-CN', options)
}

function notificationKey(notification: NotificationDto): string {
  return `${notification.type || 'announcement'}:${notification.notificationId}`
}

/**
 * 对方显示名：后端会话列表不含用户名，
 * 用商品名代替，双方都能对应上（买家和卖家聊的都是同一件商品）
 * TODO: 后端 ConversationDto 补 otherUserName 字段后替换
 */
function displayName(conversation: ConversationDto): string {
  return `关于「${conversation.productName}」`
}

async function loadConversations(): Promise<void> {
  loadingConversations.value = true
  conversationsError.value = ''

  try {
    // 后端搜索 = 列表接口的 keyword 参数（无独立 search 路由）
    const keywordText = keyword.value.trim()

    await messageStore.loadConversations(keywordText || undefined)

    conversations.value = messageStore.conversations
  } catch (error) {
    conversationsError.value = '会话列表加载失败，请稍后重试'

    console.error('会话列表加载失败：', error)
  } finally {
    loadingConversations.value = false
  }
}

async function loadNotifications(): Promise<void> {
  loadingNotifications.value = true
  notificationsError.value = ''

  try {
    const response = await getNotifications()

    notifications.value = response.data ?? []
  } catch (error) {
    notificationsError.value = '通知列表加载失败，请稍后重试'

    console.error('通知列表加载失败：', error)
  } finally {
    loadingNotifications.value = false
  }
}

function openConversation(conversation: ConversationDto): void {
  void router.push({
    name: 'message-chat',
    params: { conversationId: String(conversation.conversationId) }
  })
}

function isDeletingConversation(id: number): boolean {
  return deletingConversationIds.value.includes(id)
}

async function handleDeleteConversation(
  conversation: ConversationDto
): Promise<void> {
  if (isDeletingConversation(conversation.conversationId)) {
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定删除与「${conversation.productName}」相关的会话吗？`,
      '删除会话',
      {
        type: 'warning',
        confirmButtonText: '删除',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  deletingConversationIds.value = [
    ...deletingConversationIds.value,
    conversation.conversationId
  ]

  try {
    await deleteConversation(conversation.conversationId)

    ElMessage.success('会话已删除')

    await loadConversations()

    void messageStore.loadUnreadCount()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '删除会话失败，请稍后重试')
    )

    console.error('删除会话失败：', error)
  } finally {
    deletingConversationIds.value =
      deletingConversationIds.value.filter(
        (id) => id !== conversation.conversationId
      )
  }
}

function isDeletingNotification(notification: NotificationDto): boolean {
  return deletingNotificationIds.value.includes(notificationKey(notification))
}

async function handleDeleteNotification(
  notification: NotificationDto
): Promise<void> {
  if (isDeletingNotification(notification)) {
    return
  }

  const key = notificationKey(notification)

  try {
    await ElMessageBox.confirm(
      '确定删除这条通知吗？',
      '删除通知',
      {
        type: 'warning',
        confirmButtonText: '删除',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  deletingNotificationIds.value = [
    ...deletingNotificationIds.value,
    key
  ]

  try {
    await deleteNotification(notification.notificationId, notification.type)

    ElMessage.success('通知已删除')

    notifications.value = notifications.value.filter(
      (item) =>
        notificationKey(item) !== key
    )
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '删除通知失败，请稍后重试')
    )

    console.error('删除通知失败：', error)
  } finally {
    deletingNotificationIds.value =
      deletingNotificationIds.value.filter(
        (id) => id !== key
      )
  }
}

watch(keyword, () => {
  void loadConversations()
})

onMounted(() => {
  void loadConversations()
  void loadNotifications()
  void messageStore.loadUnreadCount()
})
</script>

<template>
  <main class="message-page">
    <div class="message-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">MESSAGES</p>

          <h1>消息</h1>

          <p class="page-description">
            查看你与他人的会话，以及平台发送的系统通知。
          </p>
        </div>
      </header>

      <section class="message-panel">
        <el-tabs
          v-model="activeTab"
          class="message-tabs"
        >
          <!-- 会话 -->
          <el-tab-pane
            label="会话"
            name="conversation"
          >
            <div class="pane-toolbar">
              <el-input
                v-model="keyword"
                class="search-input"
                clearable
                placeholder="搜索会话"
                :prefix-icon="Search"
              />
            </div>

            <!-- 加载中 -->
            <div
              v-if="loadingConversations"
              class="pane-skeleton"
            >
              <el-skeleton
                v-for="index in 3"
                :key="index"
                :rows="2"
                animated
              />
            </div>

            <!-- 错误 -->
            <el-result
              v-else-if="
                conversationsError &&
                conversations.length === 0
              "
              icon="error"
              title="会话列表加载失败"
              :sub-title="conversationsError"
            >
              <template #extra>
                <el-button
                  type="primary"
                  @click="loadConversations"
                >
                  重新加载
                </el-button>
              </template>
            </el-result>

            <!-- 空状态 -->
            <el-empty
              v-else-if="conversations.length === 0"
              description="暂无会话"
              class="pane-empty"
            />

            <!-- 会话列表 -->
            <ul
              v-else
              class="conversation-list"
            >
              <li
                v-for="conversation in conversations"
                :key="conversation.conversationId"
                class="conversation-item"
              >
                <button
                  class="conversation-main"
                  type="button"
                  @click="openConversation(conversation)"
                >
                  <el-avatar
                    class="conversation-avatar"
                    :size="46"
                  >
                    对
                  </el-avatar>

                  <span class="conversation-body">
                    <span class="conversation-top">
                      <span class="conversation-name">
                        {{ displayName(conversation) }}
                      </span>

                      <span class="conversation-time">
                        {{ formatTime(conversation.createTime) }}
                      </span>
                    </span>

                    <span class="conversation-product">
                      商品 #{{ conversation.productId }}
                    </span>

                    <span class="conversation-preview">
                      <!-- TODO: 后端补 lastMessage 摘要后替换 -->
                      {{ conversation.unreadCount > 0 ? `${conversation.unreadCount} 条未读消息` : '点击查看消息' }}
                    </span>
                  </span>
                </button>

                <el-badge
                  v-if="conversation.unreadCount > 0"
                  :value="conversation.unreadCount"
                  :max="99"
                  class="conversation-badge"
                />

                <el-button
                  type="danger"
                  link
                  class="conversation-delete"
                  :loading="
                    isDeletingConversation(conversation.conversationId)
                  "
                  :disabled="deletingConversationIds.length > 0"
                  @click="handleDeleteConversation(conversation)"
                >
                  删除
                </el-button>
              </li>
            </ul>
          </el-tab-pane>

          <!-- 通知 -->
          <el-tab-pane
            name="notification"
          >
            <template #label>
              <span class="tab-label">
                通知
              </span>
            </template>

            <!-- 加载中 -->
            <div
              v-if="loadingNotifications"
              class="pane-skeleton"
            >
              <el-skeleton
                v-for="index in 3"
                :key="index"
                :rows="2"
                animated
              />
            </div>

            <!-- 错误 -->
            <el-result
              v-else-if="
                notificationsError &&
                notifications.length === 0
              "
              icon="error"
              title="通知列表加载失败"
              :sub-title="notificationsError"
            >
              <template #extra>
                <el-button
                  type="primary"
                  @click="loadNotifications"
                >
                  重新加载
                </el-button>
              </template>
            </el-result>

            <!-- 空状态 -->
            <el-empty
              v-else-if="notifications.length === 0"
              description="暂无通知"
              class="pane-empty"
            />

            <!-- 通知列表 -->
            <ul
              v-else
              class="notification-list"
            >
              <li
                v-for="notification in notifications"
                :key="notificationKey(notification)"
                class="notification-item"
                :class="{
                  'notification-item--warning':
                    notification.type === 'warning'
                }"
              >
                <div class="notification-main">
                  <div class="notification-top">
                    <h3 class="notification-title">
                      {{ notification.title }}
                    </h3>

                    <span class="notification-time">
                      {{ formatTime(notification.createTime) }}
                    </span>
                  </div>

                  <p class="notification-content">
                    {{ notification.content }}
                  </p>
                </div>

                <div class="notification-actions">
                  <el-button
                    v-if="notification.type !== 'warning'"
                    type="danger"
                    link
                    :loading="isDeletingNotification(notification)"
                    :disabled="deletingNotificationIds.length > 0"
                    @click="handleDeleteNotification(notification)"
                  >
                    删除
                  </el-button>
                </div>
              </li>
            </ul>
          </el-tab-pane>
        </el-tabs>
      </section>
    </div>
  </main>
</template>

<style scoped>
.message-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.message-container {
  width: 100%;
  max-width: 820px;
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

.message-panel {
  padding: 8px 28px 28px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.message-tabs :deep(.el-tabs__nav-wrap::after) {
  height: 1px;
  background-color: #edf1ef;
}

.message-tabs :deep(.el-tabs__item.is-active) {
  color: #24735b;
}

.message-tabs :deep(.el-tabs__active-bar) {
  background-color: #24735b;
}

.tab-label {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.pane-toolbar {
  padding: 16px 0 8px;
}

.search-input {
  max-width: 320px;
}

.pane-skeleton {
  padding: 16px 0;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.pane-empty {
  padding: 60px 0 40px;
}

/* 会话列表 */
.conversation-list,
.notification-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.conversation-item {
  display: flex;
  padding: 16px 0;
  align-items: center;
  gap: 14px;
  border-bottom: 1px solid #edf1ef;
}

.conversation-item:last-child {
  border-bottom: 0;
}

.conversation-main {
  display: flex;
  min-width: 0;
  flex: 1;
  align-items: center;
  gap: 14px;
  padding: 0;
  color: inherit;
  background: transparent;
  border: none;
  text-align: left;
  cursor: pointer;
}

.conversation-avatar {
  flex-shrink: 0;
  color: #ffffff;
  background: #3e9b79;
  font-weight: 700;
}

.conversation-body {
  display: flex;
  min-width: 0;
  flex: 1;
  flex-direction: column;
  gap: 4px;
}

.conversation-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.conversation-name {
  color: #1e2a26;
  font-size: 16px;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.conversation-time {
  flex-shrink: 0;
  color: #9aa6a0;
  font-size: 12px;
}

.conversation-product {
  color: #24735b;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.conversation-preview {
  color: #6c7a74;
  font-size: 14px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.conversation-delete {
  flex-shrink: 0;
}

/* 通知列表 */
.notification-item {
  display: flex;
  padding: 18px 0;
  align-items: flex-start;
  gap: 16px;
  border-bottom: 1px solid #edf1ef;
}

.notification-item:last-child {
  border-bottom: 0;
}

.notification-item--warning .notification-title {
  color: #b3342a;
}

.notification-main {
  min-width: 0;
  flex: 1;
}

.notification-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.notification-title {
  margin: 0;
  color: #1e2a26;
  font-size: 16px;
  font-weight: 600;
}

.notification-time {
  flex-shrink: 0;
  color: #9aa6a0;
  font-size: 12px;
}

.notification-content {
  margin: 8px 0 0;
  color: #6c7a74;
  font-size: 14px;
  line-height: 1.7;
  overflow-wrap: anywhere;
}

.notification-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 4px;
}

@media (max-width: 640px) {
  .notification-item,
  .conversation-item {
    flex-wrap: wrap;
  }

  .notification-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
