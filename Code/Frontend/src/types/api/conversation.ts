/**
 * 第8模块：消息、会话与通知 —— 会话/消息类型
 *
 * 字段以后端实际返回为准（来源：Dtos/Communication/CommunicationDtos.cs）：
 * - ConversationDto：会话列表/详情项（列表和详情共用同一个 DTO）
 * - MessageDto：单条聊天消息
 *
 * 注意：
 * - 后端会话详情接口（GET /api/conversations/{id}）不包含 messages，
 *   消息记录需单独请求 GET /api/conversations/{id}/messages
 * - 后端不返回对方用户名、商品封面、商品价格，如需展示要另行请求
 */

/**
 * 消息类型：0=文字，1=图片（对应后端 MessageType 枚举）
 */
export type MessageType = 0 | 1

/**
 * 会话列表项 / 会话详情（后端两者共用同一 DTO）
 */
export interface ConversationDto {
    /** 会话 ID（对应后端 session_id） */
    conversationId: number
    productId: number
    productName: string
    /** 买家用户 ID */
    buyerId: number
    /** 卖家用户 ID（来自会话商品的发布者） */
    sellerId: number
    /** 会话创建时间 */
    createTime: string
    /** 我方未读消息数 */
    unreadCount: number
}

/**
 * 会话内的一条消息
 */
export interface MessageDto {
    conversationId: number
    /** 消息 ID（对应后端 msg_index，会话内自增） */
    messageId: number
    /** 发送者用户 ID */
    senderId: number
    /** 消息类型：0=文字，1=图片 */
    messageType: MessageType
    /** 附件文件 ID（messageType=1 时有效） */
    fileId: number | null
    /** 文本内容；纯图片消息后端存占位符 "[图片]" */
    content: string
    /** 发送时间 */
    sendTime: string
    /** 对方是否已读 */
    isRead: boolean
}

/**
 * 创建/打开会话请求（买家为当前登录用户，无需传对方 ID）
 */
export interface CreateConversationRequest {
    productId: number
}

/**
 * 发送文字消息请求（JSON；附件走 multipart 的 /attachments 接口）
 */
export interface SendMessageRequest {
    content: string
}

/**
 * 未读消息数量（GET /api/messages/unread-count 直接返回数字本身，无包裹对象）
 */
export type UnreadCountDto = number
