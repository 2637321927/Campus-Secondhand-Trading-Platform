import request from '../http'
import type { NotificationDto } from '../../types/api/notification'

// ===== 第8模块：系统通知 =====

/**
 * 获取系统通知列表
 */
export function getNotifications() {
    return request.get<NotificationDto[]>(
        '/api/notifications'
    )
}

/**
 * 获取单条通知详情
 */
export function getNotification(notificationId: number, type?: string) {
    return request.get<NotificationDto>(
        `/api/notifications/${notificationId}`,
        { params: { type } }
    )
}

/**
 * 删除通知
 */
export function deleteNotification(notificationId: number, type?: string) {
    return request.delete<void>(
        `/api/notifications/${notificationId}`,
        { params: { type } }
    )
}

/**
 * 标记单条通知为已读
 */
export function markNotificationRead(notificationId: number, type?: string) {
    return request.patch<void>(
        `/api/notifications/${notificationId}/read`,
        null,
        { params: { type } }
    )
}

/**
 * 全部通知标记为已读
 */
export function markAllNotificationsRead() {
    return request.patch<void>('/api/notifications/read-all')
}
