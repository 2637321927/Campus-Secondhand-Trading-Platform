import request from '../http'
import type {
    ConversationDto,
    CreateConversationRequest,
    SendMessageRequest,
    MessageDto,
    UnreadCountDto
} from '../../types/api/conversation'

// ===== 第8模块：消息、会话 =====

/**
 * 获取当前用户会话列表
 * @param keyword 可选，按商品名模糊搜索（后端列表接口自带过滤，无独立 search 路由）
 */
export function getConversations(keyword?: string) {
    return request.get<ConversationDto[]>(
        '/api/conversations',
        { params: keyword ? { keyword } : undefined }
    )
}

/**
 * 创建或打开与某商品卖家的会话（买家为当前登录用户，同一买家+商品只保留一个会话）
 */
export function createConversation(data: CreateConversationRequest) {
    return request.post<ConversationDto>(
        '/api/conversations',
        data
    )
}

/**
 * 获取会话详情（不含消息记录，消息需调 getConversationMessages）
 */
export function getConversation(conversationId: number) {
    return request.get<ConversationDto>(
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
 * 将会话中对方发来的消息全部标记为已读
 */
export function markConversationRead(conversationId: number) {
    return request.patch<void>(
        `/api/conversations/${conversationId}/read`
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
 * 发送文字消息（JSON 请求体）
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
 * 发送图片等附件（multipart/form-data，字段名为 file）
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
 * 获取当前用户未读消息数量（后端直接返回数字本身）
 */
export function getUnreadCount() {
    return request.get<UnreadCountDto>(
        '/api/messages/unread-count'
    )
}
