/**
 * 举报原因选项
 */
export interface ReportReason {
    code: string
    name: string
}

/**
 * 发起举报请求
 */
export interface CreateReportDto {
    targetType: string
    targetId: number
    reason: string
    info?: string | null
    accusedId?: number | null
    productId?: number | null
}

/**
 * 工单（举报/申诉）通用返回项
 */
export interface WorkOrderDto {
    id: number
    type: number
    reason: string
    info: string | null
    status: string
    result: string | null
    response: string | null
    createTime: string
    targetType: string | null
    targetId: number | null
    appealAgainstId: number | null
}

/**
 * 被举报商品信息摘要
 */
export interface ReportProductInfoDto {
    productId: number
    name: string
    sellerId: number
    status: string
}

/**
 * 被举报用户信息摘要
 */
export interface ReportUserInfoDto {
    userId: number
    userName: string
    profile: string | null
}
