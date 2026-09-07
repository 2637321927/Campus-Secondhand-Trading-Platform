import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { ConversationDto } from '../types/api/conversation'
import {
    getConversations,
    getUnreadCount
} from '../api/modules/conversation'

/**
 * 消息模块状态：未读消息数量、会话列表、当前会话。
 */
export const useMessageStore = defineStore('message', () => {
    const unreadCount = ref(0)
    const conversations = ref<ConversationDto[]>([])
    const currentConversation = ref<ConversationDto | null>(null)
    const loading = ref(false)
    const initialized = ref(false)

    const hasUnread = computed(() => unreadCount.value > 0)

    async function loadUnreadCount(): Promise<void> {
        try {
            const response = await getUnreadCount()

            unreadCount.value = response.data ?? 0
        } catch (error) {
            console.warn('未读消息数量加载失败：', error)
        }
    }

    async function loadConversations(keyword?: string): Promise<void> {
        loading.value = true

        try {
            const response = await getConversations(keyword)

            conversations.value = response.data ?? []
        } catch (error) {
            console.warn('会话列表加载失败：', error)

            conversations.value = []
        } finally {
            loading.value = false
        }
    }

    function setCurrentConversation(
        conversation: ConversationDto | null
    ): void {
        currentConversation.value = conversation
    }

    function clear(): void {
        unreadCount.value = 0
        conversations.value = []
        currentConversation.value = null
        initialized.value = false
    }

    return {
        unreadCount,
        conversations,
        currentConversation,
        loading,
        initialized,
        hasUnread,
        loadUnreadCount,
        loadConversations,
        setCurrentConversation,
        clear
    }
})
