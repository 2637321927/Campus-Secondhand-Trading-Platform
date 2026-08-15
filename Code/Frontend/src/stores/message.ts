import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type {
    ConversationDto,
    ConversationDetailDto
} from '../types/api/conversation'
import {
    getConversations,
    getUnreadCount
} from '../api/modules/conversation'

/**
 * 消息模块状态：未读消息数量、会话列表、当前会话。
 *
 * 说明：第8模块后端尚未实现，接口调用在骨架阶段可能 404，
 * 故这里全部采用静默容错，不影响页面其它功能。
 */
export const useMessageStore = defineStore('message', () => {
    const unreadCount = ref(0)
    const conversations = ref<ConversationDto[]>([])
    const currentConversation = ref<ConversationDetailDto | null>(null)
    const loading = ref(false)
    const initialized = ref(false)

    const hasUnread = computed(() => unreadCount.value > 0)

    async function loadUnreadCount(): Promise<void> {
        try {
            const response = await getUnreadCount()

            unreadCount.value = response.data?.unreadCount ?? 0
        } catch (error) {
            console.warn('未读消息数量加载失败（后端可能未实现）：', error)
        }
    }

    async function loadConversations(): Promise<void> {
        loading.value = true

        try {
            const response = await getConversations()

            conversations.value = response.data ?? []
        } catch (error) {
            console.warn('会话列表加载失败（后端可能未实现）：', error)

            conversations.value = []
        } finally {
            loading.value = false
        }
    }

    function setCurrentConversation(
        conversation: ConversationDetailDto | null
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
