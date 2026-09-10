// TypeScript类型定义
// ==================== 商品管理类型 ====================

export interface AdminProductListParams {
  keyword?: string
  status?: 0 | 1 | 2 | 3 | 4 | 5  // 0=在售 1=已售 2=下架 3=待审核 4=驳回 5=交易中
  categoryId?: number
  sellerId?: number
  page?: number
  pageSize?: number
}

export interface AdminProductListItem {
  productId: number
  name: string
  price: number
  info: string | null
  status: 0 | 1 | 2 | 3 | 4 | 5
  releaseDate: string
  userId: number
  categoryId: number
  sellerName: string
  categoryName: string | null
  viewCount: number
  favoriteCount: number
  commentCount: number
  imageCount: number
  rejectReason: string | null
  reviewedByAdminId: number | null
  reviewedAt: string | null
}

export interface AdminProductDetail extends AdminProductListItem {
  images: AdminProductImage[]
  auditLogs: AdminProductAuditLog[]
}

export interface AdminProductImage {
  fileId: number
  imgIndex: number
}

export interface AdminProductAuditLog {
  auditId: number
  adminId: number
  action: 'approve' | 'reject' | 'remove' | 'restore' | 'delete'
  reason: string | null
  oldStatus: number
  newStatus: number
  createTime: string
}

// ==================== 用户管理类型 ====================

export interface AdminUserListParams {
  keyword?: string
  userType?: 0 | 1
  accountStatus?: 0 | 1 | 2 | 3  // 0=正常 1=禁言 2=限制发布 3=封禁
  creditMin?: number
  creditMax?: number
  registerStart?: string
  registerEnd?: string
  page?: number
  pageSize?: number
}

export interface AdminUserListItem {
  userId: number
  email: string
  phoneNumber: string | null
  userName: string
  userType: 0 | 1
  accountStatus: 0 | 1 | 2 | 3
  isBanned: 0 | 1
  bannedUntil: string | null
  credit: number
  registerTime: string
  productCount: number
  orderCount: number
  warningCount: number
  violationCount: number
}

export interface AdminUserDetail extends AdminUserListItem {
  gender: string
  profile: string | null
  avatarFileId: number | null
}

// ==================== 工单管理类型 ====================

export type AdminWorkOrderFilterType = 'report' | 'appeal'
export type AdminWorkOrderStatus = 'waiting' | 'done'
export type AdminWorkOrderTargetType = 'product' | 'user' | 'comment' | 'message' | 'order'
export type AdminWorkOrderResult = 'accepted' | 'rejected' | 'handled' | 'approved'

export interface AdminWorkOrderListParams {
  keyword?: string
  status?: AdminWorkOrderStatus
  type?: AdminWorkOrderFilterType
  targetType?: AdminWorkOrderTargetType
  page?: number
  pageSize?: number
}

export interface AdminWorkOrder {
  workOrderId: number
  type: 1 | 2
  targetType: AdminWorkOrderTargetType | null
  targetId: number | null
  reason: string
  info: string | null
  status: AdminWorkOrderStatus
  result: AdminWorkOrderResult | null
  handleAction: string | null
  createTime: string
  response: string | null
  responseTime: string | null
  initiatorId: number
  initiatorName: string
  accusedId: number | null
  accusedName: string | null
  productId: number | null
  productName: string | null
  appealAgainstWorkOrderId: number | null
  appealAgainstReason: string | null
  adminId: number | null
}

export interface AdminWorkOrderDetail extends AdminWorkOrder {
  timeline: AdminWorkOrderTimeline[]
  attachments: AdminWorkOrderAttachment[]
}

export interface AdminWorkOrderAttachment {
  fileId: number
  fileName: string
}

export interface AdminWorkOrderTimeline {
  timelineId: number
  action: string
  note: string | null
  adminId: number | null
  createTime: string
}

export interface AdminWorkOrderPage {
  items: AdminWorkOrder[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ModerationTasks {
  totalPending: number
  waitingCount: number
  doneCount: number
  reportCount: number
  appealCount: number
  recentTasks: AdminWorkOrder[]
}
