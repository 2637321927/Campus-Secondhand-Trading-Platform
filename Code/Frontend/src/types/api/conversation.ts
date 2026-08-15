/**
 * 第8模块：消息、会话与通知 —— 会话/消息类型
 *
 * 说明：后端第8模块接口尚未实现，以下字段为按接口文档语义与
 * 后端 Conversation/Message 模型（session_id、msg_index 等）推断，
 * 以后端最终 DTO 为准。
 */

/**
 * 消息类型：0=文本，1=文件
 */
export type MessageType = 0 | 1

/**
 * 会话列表项
 */
export interface ConversationDto {
    /** 会话 ID（对应后端 session_id） */
    conversationId: number
    productId: number
    productName: string
    productCoverFileId: number | null
    /** 对方用户（卖家或买家）ID */
    otherUserId: number
    /** 对方用户名称 */
    otherUserName: string
    /** 最后一条消息内容 */
    lastMessage: string
    /** 最后一条消息时间 */
    lastMessageTime: string
    /** 未读消息数量 */
    unreadCount: number
}

/**
 * 会话详情（含关联商品、对方用户与消息记录）
 */
export interface ConversationDetailDto {
    conversationId: number
    productId: number
    productName: string
    productCoverFileId: number | null
    productPrice: number
    otherUserId: number
    otherUserName: string
    messages: MessageDto[]
}

/**
 * 会话内的一条消息
 */
export interface MessageDto {
    /** 消息 ID（对应后端 msg_index） */
    messageId: number
    senderId: number
    msgType: MessageType
    /** 文本内容（msgType=0 时有效） */
    content: string
    /** 附件文件 ID（msgType=1 时有效） */
    fileId: number | null
    fileName: string | null
    sendTime: string
    isRead: boolean
}

/**
 * 创建/打开会话请求
 */
export interface CreateConversationRequest {
    productId: number
    /** 对方用户 ID，一般由商品推断，可省略 */
    otherUserId?: number
}

/**
 * 发送文本消息请求
 */
export interface SendMessageRequest {
    content: string
}

/**
 * 未读消息数量
 */
export interface UnreadCountDto {
    unreadCount: number
}
