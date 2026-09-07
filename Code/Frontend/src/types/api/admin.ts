// TypeScript类型定义
// ==================== 商品管理类型 ====================

export interface AdminProductListParams {
  keyword?: string
  status?: 0 | 1 | 2 | 3 | 4  // 0=在售 1=已售 2=下架 3=待审核 4=驳回
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
  status: 0 | 1 | 2 | 3 | 4
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
  imgId: number
  imgUrl: string
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

// ==================== 举报管理类型 ====================

export interface ReportListParams {
  keyword?: string
  status?: 'waiting' | 'processing' | 'done'
  targetType?: 'product' | 'user' | 'comment' | 'message' | 'order'
  page?: number
  pageSize?: number
}

export interface ReportDetail {
  reportId: number
  reporterId: number
  reporterName: string
  targetType: string
  targetId: number
  targetName: string
  reason: string
  description: string | null
  status: 'waiting' | 'processing' | 'done'
  result: 'accepted' | 'rejected' | null
  createTime: string
  attachments: ReportAttachment[]
  timeline: ReportTimeline[]
}

export interface ReportAttachment {
  fileId: number
  fileUrl: string
  fileName: string
}

export interface ReportTimeline {
  id: number
  action: string
  description: string
  operatorId: number
  operatorName: string
  createTime: string
}