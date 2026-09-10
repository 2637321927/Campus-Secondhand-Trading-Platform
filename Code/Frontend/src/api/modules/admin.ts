// 管理员API接口封装
import request from '../http'
import type {
  AdminProductListParams,
  AdminProductListItem,
  AdminProductDetail,
  AdminUserListParams,
  AdminUserListItem,
  AdminUserDetail,
  AdminWorkOrderListParams,
  AdminWorkOrderDetail,
  AdminWorkOrderPage,
  ModerationTasks
} from '../../types/api/admin'

// ==================== 商品管理 ====================

// 管理员商品列表
export function getAdminProducts(params: AdminProductListParams) {
  return request.get<{
    items: AdminProductListItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
  }>('/api/admin/products', { params })
}

// 待审核商品列表
export function getPendingProducts(page = 1, pageSize = 20) {
  return request.get('/api/admin/products/pending-review', {
    params: { page, pageSize }
  })
}

// 商品审核统计
export function getProductStatistics() {
  return request.get('/api/admin/products/statistics')
}

//管理员商品详情
export function getAdminProductDetail(productId: number) {
  return request.get<AdminProductDetail>(`/api/admin/products/${productId}`)
}

// 审核通过
export function approveProduct(productId: number) {
  return request.patch(`/api/admin/products/${productId}/approve`)
}

// 审核驳回
export function rejectProduct(productId: number, data: { reason: string }) {
  return request.patch(`/api/admin/products/${productId}/reject`, data)
}

// 强制下架
export function removeProduct(productId: number, data: { reason: string }) {
  return request.patch(`/api/admin/products/${productId}/remove`, data)
}

// 恢复商品
export function restoreProduct(productId: number) {
  return request.patch(`/api/admin/products/${productId}/restore`)
}

// 删除商品
export function deleteProduct(productId: number) {
  return request.delete(`/api/admin/products/${productId}`)
}

// 商品审核日志
export function getAuditLogs(productId: number) {
  return request.get(`/api/admin/products/${productId}/audit-logs`)
}

// ==================== 用户管理 ====================

// 管理员用户列表
export function getAdminUsers(params: AdminUserListParams) {
  return request.get<{
    items: AdminUserListItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
  }>('/api/admin/users', { params })
}

// 用户统计
export function getUserStatistics() {
  return request.get('/api/admin/users/statistics')
}

// 用户详情
export function getAdminUserDetail(userId: number) {
  return request.get<AdminUserDetail>(`/api/admin/users/${userId}`)
}

// 用户发布商品
export function getUserProducts(userId: number) {
  return request.get(`/api/admin/users/${userId}/products`)
}

// 用户相关订单
export function getUserOrders(userId: number) {
  return request.get(`/api/admin/users/${userId}/orders`)
}

// 用户相关举报
export function getUserReports(userId: number) {
  return request.get(`/api/admin/users/${userId}/reports`)
}

// 用户申诉
export function getUserAppeals(userId: number) {
  return request.get(`/api/admin/users/${userId}/appeals`)
}

// 用户信誉与违规概览
export function getUserReputation(userId: number) {
  return request.get(`/api/admin/users/${userId}/reputation`)
}

// 修改用户状态
export function updateUserStatus(userId: number, data: {
  status: 0 | 1 | 2 | 3
  bannedUntil?: string | null
  reason?: string
}) {
  return request.patch(`/api/admin/users/${userId}/status`, data)
}

// 发送用户警告
export function sendUserWarning(userId: number, data: { reason: string }) {
  return request.post(`/api/admin/users/${userId}/warning`, data)
}

// ==================== 订单管理 ====================

// 获取订单列表
export function getOrderList(params: {
  orderId?: string
  status?: string
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}) {
  return request.get('/api/admin/orders', { params })
}

// 订单统计
export function getOrderStatistics() {
  return request.get('/api/admin/orders/statistics')
}

// 取消订单
export function cancelOrder(orderId: string) {
  return request.patch(`/api/admin/orders/${orderId}/cancel`)
}

// 完成订单
export function completeOrder(orderId: string) {
  return request.patch(`/api/admin/orders/${orderId}/complete`)
}

// 订单详情
export function getOrderDetail(orderId: string) {
  return request.get(`/api/admin/orders/${orderId}`)
}

// ==================== 工单管理 ====================

// 工单列表（统一举报与申诉）
export function getWorkOrders(params: AdminWorkOrderListParams) {
  return request.get<AdminWorkOrderPage>('/api/admin/work-orders', { params })
}

// 工单详情
export function getWorkOrderDetail(workOrderId: number) {
  return request.get<AdminWorkOrderDetail>(`/api/admin/work-orders/${workOrderId}`)
}

// 驳回工单
export function rejectWorkOrder(workOrderId: number) {
  return request.patch(`/api/admin/work-orders/${workOrderId}/reject`)
}

// 处理工单
export function processWorkOrder(workOrderId: number, data: {
  action: 'none' | 'remove_product' | 'ban_user' | 'mute_user' | 'warn_user' | 'approve'
  reason: string
}) {
  return request.patch<AdminWorkOrderDetail>(`/api/admin/work-orders/${workOrderId}/handle`, data)
}

// 管理员待办任务
export function getModerationTasks() {
  return request.get<ModerationTasks>('/api/admin/moderation/tasks')
}

// ==================== 公告管理 ====================

// 获取公告列表
export function getAnnouncements(params: {
  keyword?: string
  status?: string
  page?: number
  pageSize?: number
}) {
  return request.get('/api/admin/announcements', { params })
}

// 公告统计
export function getAnnouncementStatistics() {
  return request.get('/api/admin/announcements/statistics')
}

// 创建公告
export function createAnnouncement(data: {
  title: string
  content: string
  isPinned: boolean
  status: 'draft' | 'published'
}) {
  return request.post('/api/admin/announcements', data)
}

// 更新公告
export function updateAnnouncement(id: number, data: {
  title?: string
  content?: string
  isPinned?: boolean
  status?: 'draft' | 'published'
}) {
  return request.put(`/api/admin/announcements/${id}`, data)
}

// 发布公告
export function publishAnnouncement(id: number) {
  return request.patch(`/api/admin/announcements/${id}/publish`)
}

// 下架公告
export function archiveAnnouncement(id: number) {
  return request.patch(`/api/admin/announcements/${id}/archive`)
}

// 删除公告
export function deleteAnnouncement(id: number) {
  return request.delete(`/api/admin/announcements/${id}`)
}

// 公告详情
export function getAnnouncementDetail(id: number) {
  return request.get(`/api/admin/announcements/${id}`)
}

// ==================== 类型定义 ====================

// 商品审核统计
export interface ProductStatistics {
  totalProducts: number
  availableCount: number
  soldCount: number
  removedCount: number
  pendingReviewCount: number
  rejectedCount: number
  newProductsToday: number
  totalAuditLogs?: number
  todayAuditLogs?: number
}

// 用户统计
export interface UserStatistics {
  totalUsers: number
  normalUsers: number
  mutedUsers: number
  publishRestrictedUsers: number
  bannedUsers: number
  newUsersToday: number
  newUsersThisWeek: number
  usersWithProducts: number
  totalOrders: number
  totalWorkOrders?: number
  pendingWorkOrders?: number
  totalWarnings: number
}
