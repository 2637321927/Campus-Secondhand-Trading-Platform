/**
 * 第8模块：系统通知类型
 *
 * type：announcement=系统公告，warning=用户收到的平台警告
 */
export interface NotificationDto {
    notificationId: number
    title: string
    content: string
    /** 通知类型：公告、用户警告或商品/订单动态 */
    type: 'announcement' | 'warning' | 'product' | 'order' | 'system'
    createTime: string
    /** 关联对象 ID（如订单、商品），无则 null */
    relatedId: number | null
}
