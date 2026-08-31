import request from '../http'
import type {
    ConversationDto,
    ConversationDetailDto,
    CreateConversationRequest,
    SendMessageRequest,
    MessageDto,
    UnreadCountDto
} from '../../types/api/conversation'

// ===== 第8模块：消息、会话 =====

/**
 * 获取当前用户会话列表
 */
export function getConversations() {
    return request.get<ConversationDto[]>(
        '/api/conversations'
    )
}

/**
 * 创建或打开与某个用户、某个商品相关的会话
 */
export function createConversation(data: CreateConversationRequest) {
    return request.post<ConversationDto>(
        '/api/conversations',
        data
    )
}

/**
 * 获取会话详情（含关联商品和对方用户信息）
 */
export function getConversation(conversationId: number) {
    return request.get<ConversationDetailDto>(
        `/api/conversations/${conversationId}`
    )
}

/**
 * 删除或隐藏会话
 */
export function deleteConversation(conversationId: number) {
    return request.delete<void>(
        `/api/conversations/${conversationId}`
    )
}

/**
 * 将会话标记为已读
 */
export function markConversationRead(conversationId: number) {
    return request.patch<void>(
        `/api/conversations/${conversationId}/read`
    )
}

/**
 * 搜索会话
 */
export function searchConversations(keyword: string) {
    return request.get<ConversationDto[]>(
        '/api/conversations/search',
        { params: { keyword } }
    )
}

/**
 * 获取会话消息记录
 */
export function getConversationMessages(conversationId: number) {
    return request.get<MessageDto[]>(
        `/api/conversations/${conversationId}/messages`
    )
}

/**
 * 发送文本消息
 */
export function sendMessage(
    conversationId: number,
    data: SendMessageRequest
) {
    return request.post<MessageDto>(
        `/api/conversations/${conversationId}/messages`,
        data
    )
}

/**
 * 发送或上传会话附件（multipart/form-data，字段名为 file）
 */
export function sendAttachment(
    conversationId: number,
    file: File
) {
    const formData = new FormData()
    formData.append('file', file)

    return request.post<MessageDto>(
        `/api/conversations/${conversationId}/attachments`,
        formData
    )
}

/**
 * 撤回或删除消息
 */
export function deleteMessage(
    conversationId: number,
    messageId: number
) {
    return request.delete<void>(
        `/api/conversations/${conversationId}/messages/${messageId}`
    )
}

/**
 * 获取当前用户未读消息数量
 */
export function getUnreadCount() {
    return request.get<UnreadCountDto>(
        '/api/messages/unread-count'
    )
}
