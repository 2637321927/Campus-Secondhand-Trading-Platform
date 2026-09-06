/**
 * 第8模块：系统通知类型
 *
 * 说明：后端暂无 Notification 模型，字段为按接口文档语义推断，
 * 以后端最终 DTO 为准。
 */
export interface NotificationDto {
    notificationId: number
    title: string
    content: string
    /** 通知类型，例如 system / order / review */
    type: string
    isRead: boolean
    createTime: string
    /** 关联对象 ID（如订单、商品），无则 null */
    relatedId: number | null
}
