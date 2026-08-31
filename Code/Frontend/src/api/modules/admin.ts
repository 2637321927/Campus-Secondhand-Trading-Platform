// 管理员API接口封装
import request from '../http'
import type {
  AdminProductListParams,
  AdminProductListItem,
  AdminProductDetail,
  AdminUserListParams,
  AdminUserListItem,
  AdminUserDetail,
  ReportListParams,
  ReportDetail
} from '@/types/api/admin'

// ==================== 商品管理 ====================

// 41. 管理员商品列表
export function getAdminProducts(params: AdminProductListParams) {
  return request.get<{
    items: AdminProductListItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
  }>('/api/admin/products', { params })
}

// 42. 待审核商品列表
export function getPendingProducts(page = 1, pageSize = 20) {
  return request.get('/api/admin/products/pending-review', {
    params: { page, pageSize }
  })
}

// 43. 商品审核统计
export function getProductStatistics() {
  return request.get('/api/admin/products/statistics')
}

// 44. 管理员商品详情
export function getAdminProductDetail(productId: number) {
  return request.get<AdminProductDetail>(`/api/admin/products/${productId}`)
}

// 45. 审核通过
export function approveProduct(productId: number) {
  return request.patch(`/api/admin/products/${productId}/approve`)
}

// 46. 审核驳回
export function rejectProduct(productId: number, data: { reason: string }) {
  return request.patch(`/api/admin/products/${productId}/reject`, data)
}

// 47. 强制下架
export function removeProduct(productId: number, data: { reason: string }) {
  return request.patch(`/api/admin/products/${productId}/remove`, data)
}

// 48. 恢复商品
export function restoreProduct(productId: number) {
  return request.patch(`/api/admin/products/${productId}/restore`)
}

// 49. 删除商品
export function deleteProduct(productId: number) {
  return request.delete(`/api/admin/products/${productId}`)
}

// 50. 商品审核日志
export function getAuditLogs(productId: number) {
  return request.get(`/api/admin/products/${productId}/audit-logs`)
}

// ==================== 用户管理 ====================

// 31. 管理员用户列表
export function getAdminUsers(params: AdminUserListParams) {
  return request.get<{
    items: AdminUserListItem[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
  }>('/api/admin/users', { params })
}

// 32. 用户统计
export function getUserStatistics() {
  return request.get('/api/admin/users/statistics')
}

// 33. 用户详情
export function getAdminUserDetail(userId: number) {
  return request.get<AdminUserDetail>(`/api/admin/users/${userId}`)
}

// 34. 用户发布商品
export function getUserProducts(userId: number) {
  return request.get(`/api/admin/users/${userId}/products`)
}

// 35. 用户相关订单
export function getUserOrders(userId: number) {
  return request.get(`/api/admin/users/${userId}/orders`)
}

// 36. 用户相关举报
export function getUserReports(userId: number) {
  return request.get(`/api/admin/users/${userId}/reports`)
}

// 37. 用户申诉
export function getUserAppeals(userId: number) {
  return request.get(`/api/admin/users/${userId}/appeals`)
}

// 38. 用户信誉与违规概览
export function getUserReputation(userId: number) {
  return request.get(`/api/admin/users/${userId}/reputation`)
}

// 39. 修改用户状态
export function updateUserStatus(userId: number, data: {
  status: 0 | 1 | 2 | 3
  bannedUntil?: string | null
  reason?: string
}) {
  return request.patch(`/api/admin/users/${userId}/status`, data)
}

// 40. 发送用户警告
export function sendUserWarning(userId: number, data: { reason: string }) {
  return request.post(`/api/admin/users/${userId}/warning`, data)
}

// ==================== 举报与申诉管理 ====================

// 51. 举报列表
export function getReports(params: ReportListParams) {
  return request.get('/api/admin/reports', { params })
}

// 52. 举报详情
export function getReportDetail(reportId: number) {
  return request.get<ReportDetail>(`/api/admin/reports/${reportId}`)
}

// 53. 举报成立
export function acceptReport(reportId: number) {
  return request.patch(`/api/admin/reports/${reportId}/accept`)
}

// 54. 举报不成立
export function rejectReport(reportId: number) {
  return request.patch(`/api/admin/reports/${reportId}/reject`)
}

// 55. 举报综合处理
export function handleReport(reportId: number, data: {
  action: 'none' | 'remove_product' | 'restore_product' | 'ban_user' | 'mute_user' | 'restrict_publish' | 'unban_user' | 'warn_user'
  reason: string
}) {
  return request.patch(`/api/admin/reports/${reportId}/handle`, data)
}

// 56. 申诉列表
export function getAppeals(params: { keyword?: string; status?: string; page?: number; pageSize?: number }) {
  return request.get('/api/admin/appeals', { params })
}

// 57. 申诉详情
export function getAppealDetail(appealId: number) {
  return request.get(`/api/admin/appeals/${appealId}`)
}

// 58. 申诉通过
export function approveAppeal(appealId: number) {
  return request.patch(`/api/admin/appeals/${appealId}/approve`)
}

// 59. 申诉驳回
export function rejectAppeal(appealId: number) {
  return request.patch(`/api/admin/appeals/${appealId}/reject`)
}

// 60. 管理员回复申诉
export function replyAppeal(appealId: number, data: { reply: string }) {
  return request.post(`/api/admin/appeals/${appealId}/reply`, data)
}

// 61. 管理员待办任务
export function getModerationTasks() {
  return request.get('/api/admin/moderation/tasks')
}
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

// 待办任务
export interface ModerationTasks {
  totalPending: number
  waitingCount: number
  processingCount: number
  reportCount: number
  appealCount: number
  recentTasks: ModerationTask[]
}

export interface ModerationTask {
  id: number
  type: 'report' | 'appeal'
  title: string
  status: 'waiting' | 'processing'
  createTime: string
}