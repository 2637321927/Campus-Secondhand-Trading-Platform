export interface PublicUserDto {
    userId: number
    userName: string
}

export interface PublicUserApiResponse {
    userId: number
    userName: string
    email?: string
    phoneNumber?: string | null
    registerTime?: string
}

/**
 * 性别取值（后端以字符串存储）
 */
export type Gender = 'male' | 'female' | 'unknown'

/**
 * 个人中心资料响应
 */
export interface UserProfileDto {
    userId: number
    email: string
    phoneNumber: string | null
    userName: string
    gender: string
    profile: string | null
    avatarFileId: number | null
    credit: number
    registerTime: string
}

/**
 * 修改个人中心资料请求（部分更新，字段为 null 时不修改）
 */
export interface UpdateProfileRequest {
    userName?: string | null
    phoneNumber?: string | null
    gender?: string | null
    profile?: string | null
}

/**
 * 头像上传成功响应
 */
export interface AvatarUploadResponse {
    avatarFileId: number
    fileName: string
}

/**
 * 订单（购买记录）响应
 */
export interface PurchaseDto {
    purchaseId: number
    status: string
    createTime: string
    cancelTime: string | null
    payTime: string | null
    shippingTime: string | null
    deliveryTime: string | null
    completeTime: string | null
    shippingFees: number
    responsibleForShip: number
    buyerId: number
    buyerName: string
    productId: number
    productName: string
    productPrice: number
    productImageFileId: number | null
}

/**
 * 其他用户公开主页信息
 */
export interface UserDto {
    userId: number
    email: string
    phoneNumber: string | null
    userName: string
    registerTime: string
}

/**
 * 浏览历史条目
 */
export interface BrowseHistoryDto {
    viewId: number
    productId: number
    productName: string
    productPrice: number
    productImageFileId: number | null
    viewTime: string
}
