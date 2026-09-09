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

// ==================== Mock 数据开关 ====================
// 当后端接口未实现时，设置为 true 启用 Mock 数据
const USE_MOCK = true

// ==================== Mock 数据 ====================
let mockAnnouncements: any[] = [
  {
    id: 1,
    title: '🎉 平台正式上线公告',
    content: '欢迎使用校园二手交易平台！本平台旨在为在校师生提供安全、便捷的二手物品交易服务。\n\n平台特色：\n1. 校内实名认证，交易更安全\n2. 支持校内当面交易\n3. 商品分类清晰，查找方便\n4. 内置聊天功能，沟通更便捷',
    status: 'published',
    isPinned: true,
    publishTime: '2026-09-01T10:00:00',
    createdAt: '2026-09-01T10:00:00',
    updatedAt: '2026-09-01T10:00:00'
  },
  {
    id: 2,
    title: '🔒 交易安全提醒',
    content: '为了保障您的交易安全，请注意以下事项：\n\n1. 建议选择校内公共场所进行当面交易\n2. 交易前请仔细检查商品实际情况\n3. 请勿脱离平台进行转账或付款\n4. 如遇可疑情况，请及时举报',
    status: 'published',
    isPinned: false,
    publishTime: '2026-09-05T14:30:00',
    createdAt: '2026-09-05T14:30:00',
    updatedAt: '2026-09-05T14:30:00'
  },
  {
    id: 3,
    title: '📱 发布商品功能已上线',
    content: '现在你可以在平台上发布自己的闲置物品了！\n\n发布步骤：\n1. 点击右上角"发布闲置"按钮\n2. 填写商品标题、描述、价格\n3. 上传商品图片\n4. 选择商品分类\n5. 点击发布即可',
    status: 'published',
    isPinned: false,
    publishTime: '2026-09-08T09:00:00',
    createdAt: '2026-09-08T09:00:00',
    updatedAt: '2026-09-08T09:00:00'
  },
  {
    id: 4,
    title: '公告功能开发中（草稿）',
    content: '这是一个草稿公告，用于测试编辑和发布功能。',
    status: 'draft',
    isPinned: false,
    publishTime: null,
    createdAt: '2026-09-09T08:00:00',
    updatedAt: '2026-09-09T08:00:00'
  }
]

let mockIdCounter = 100

// 模拟延迟
const mockDelay = () => new Promise(resolve => setTimeout(resolve, 300))

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

// ==================== 举报与申诉管理 ====================

// 举报列表
export function getReports(params: ReportListParams) {
  return request.get('/api/admin/reports', { params })
}

// 举报详情
export function getReportDetail(reportId: number) {
  return request.get<ReportDetail>(`/api/admin/reports/${reportId}`)
}

// 举报成立
export function acceptReport(reportId: number) {
  return request.patch(`/api/admin/reports/${reportId}/accept`)
}

// 举报不成立
export function rejectReport(reportId: number) {
  return request.patch(`/api/admin/reports/${reportId}/reject`)
}

// 举报综合处理
export function handleReport(reportId: number, data: {
  action: 'none' | 'remove_product' | 'restore_product' | 'ban_user' | 'mute_user' | 'restrict_publish' | 'unban_user' | 'warn_user'
  reason: string
}) {
  return request.patch(`/api/admin/reports/${reportId}/handle`, data)
}

// 申诉列表
export function getAppeals(params: { keyword?: string; status?: string; page?: number; pageSize?: number }) {
  return request.get('/api/admin/appeals', { params })
}

// 申诉详情
export function getAppealDetail(appealId: number) {
  return request.get(`/api/admin/appeals/${appealId}`)
}

// 申诉通过
export function approveAppeal(appealId: number) {
  return request.patch(`/api/admin/appeals/${appealId}/approve`)
}

// 申诉驳回
export function rejectAppeal(appealId: number) {
  return request.patch(`/api/admin/appeals/${appealId}/reject`)
}

// 管理员回复申诉
export function replyAppeal(appealId: number, data: { reply: string }) {
  return request.post(`/api/admin/appeals/${appealId}/reply`, data)
}

// 管理员待办任务
export function getModerationTasks() {
  return request.get('/api/admin/moderation/tasks')
}

// ==================== 公告管理 ====================

// 获取公告列表
export function getAnnouncements(params: {
  keyword?: string
  status?: string
  page?: number
  pageSize?: number
}) {
  // 如果启用 Mock，返回模拟数据
  if (USE_MOCK) {
    return mockGetAnnouncements(params)
  }
  return request.get('/api/admin/announcements', { params })
}

// 公告统计
export function getAnnouncementStatistics() {
  if (USE_MOCK) {
    return mockGetAnnouncementStatistics()
  }
  return request.get('/api/admin/announcements/statistics')
}

// 创建公告
export function createAnnouncement(data: {
  title: string
  content: string
  isPinned: boolean
  status: 'draft' | 'published'
}) {
  if (USE_MOCK) {
    return mockCreateAnnouncement(data)
  }
  return request.post('/api/admin/announcements', data)
}

// 更新公告
export function updateAnnouncement(id: number, data: {
  title?: string
  content?: string
  isPinned?: boolean
  status?: 'draft' | 'published'
}) {
  if (USE_MOCK) {
    return mockUpdateAnnouncement(id, data)
  }
  return request.put(`/api/admin/announcements/${id}`, data)
}

// 发布公告
export function publishAnnouncement(id: number) {
  if (USE_MOCK) {
    return mockPublishAnnouncement(id)
  }
  return request.patch(`/api/admin/announcements/${id}/publish`)
}

// 下架公告
export function archiveAnnouncement(id: number) {
  if (USE_MOCK) {
    return mockArchiveAnnouncement(id)
  }
  return request.patch(`/api/admin/announcements/${id}/archive`)
}

// 删除公告
export function deleteAnnouncement(id: number) {
  if (USE_MOCK) {
    return mockDeleteAnnouncement(id)
  }
  return request.delete(`/api/admin/announcements/${id}`)
}

// 公告详情
export function getAnnouncementDetail(id: number) {
  if (USE_MOCK) {
    return mockGetAnnouncementDetail(id)
  }
  return request.get(`/api/admin/announcements/${id}`)
}

// ==================== Mock 函数 ====================

async function mockGetAnnouncements(params: {
  keyword?: string
  status?: string
  page?: number
  pageSize?: number
}) {
  await mockDelay()
  
  let list = [...mockAnnouncements]
  
  // 关键词过滤
  if (params.keyword) {
    const keyword = params.keyword.toLowerCase()
    list = list.filter(item => 
      item.title.toLowerCase().includes(keyword) ||
      item.content.toLowerCase().includes(keyword)
    )
  }
  
  // 状态过滤
  if (params.status) {
    list = list.filter(item => item.status === params.status)
  }
  
  // 按创建时间降序排列
  list.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  
  const page = params.page || 1
  const pageSize = params.pageSize || 20
  const start = (page - 1) * pageSize
  const end = start + pageSize
  const paginatedList = list.slice(start, end)
  
  return {
    data: {
      items: paginatedList,
      totalCount: list.length,
      page,
      pageSize,
      totalPages: Math.ceil(list.length / pageSize)
    }
  }
}

async function mockGetAnnouncementStatistics() {
  await mockDelay()
  
  const published = mockAnnouncements.filter(item => item.status === 'published').length
  const draft = mockAnnouncements.filter(item => item.status === 'draft').length
  const archived = mockAnnouncements.filter(item => item.status === 'archived').length
  
  return {
    data: {
      total: mockAnnouncements.length,
      published,
      draft,
      archived
    }
  }
}

async function mockCreateAnnouncement(data: {
  title: string
  content: string
  isPinned: boolean
  status: 'draft' | 'published'
}) {
  await mockDelay()
  
  const now = new Date().toISOString()
  const newAnnouncement = {
    id: mockIdCounter++,
    ...data,
    publishTime: data.status === 'published' ? now : null,
    createdAt: now,
    updatedAt: now
  }
  
  mockAnnouncements.unshift(newAnnouncement)
  
  return {
    data: newAnnouncement
  }
}

async function mockUpdateAnnouncement(id: number, data: {
  title?: string
  content?: string
  isPinned?: boolean
  status?: 'draft' | 'published'
}) {
  await mockDelay()
  
  const index = mockAnnouncements.findIndex(item => item.id === id)
  if (index === -1) {
    throw new Error('公告不存在')
  }
  
  const oldItem = mockAnnouncements[index]
  const updatedItem = {
    ...oldItem,
    ...data,
    updatedAt: new Date().toISOString()
  }
  
  mockAnnouncements[index] = updatedItem
  
  return {
    data: updatedItem
  }
}

async function mockPublishAnnouncement(id: number) {
  await mockDelay()
  
  const index = mockAnnouncements.findIndex(item => item.id === id)
  if (index === -1) {
    throw new Error('公告不存在')
  }
  
  mockAnnouncements[index] = {
    ...mockAnnouncements[index],
    status: 'published',
    publishTime: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  }
  
  return {
    data: mockAnnouncements[index]
  }
}

async function mockArchiveAnnouncement(id: number) {
  await mockDelay()
  
  const index = mockAnnouncements.findIndex(item => item.id === id)
  if (index === -1) {
    throw new Error('公告不存在')
  }
  
  mockAnnouncements[index] = {
    ...mockAnnouncements[index],
    status: 'archived',
    updatedAt: new Date().toISOString()
  }
  
  return {
    data: mockAnnouncements[index]
  }
}

async function mockDeleteAnnouncement(id: number) {
  await mockDelay()
  
  const index = mockAnnouncements.findIndex(item => item.id === id)
  if (index === -1) {
    throw new Error('公告不存在')
  }
  
  mockAnnouncements.splice(index, 1)
  
  return {
    data: { success: true }
  }
}

async function mockGetAnnouncementDetail(id: number) {
  await mockDelay()
  
  const item = mockAnnouncements.find(item => item.id === id)
  if (!item) {
    throw new Error('公告不存在')
  }
  
  return {
    data: item
  }
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