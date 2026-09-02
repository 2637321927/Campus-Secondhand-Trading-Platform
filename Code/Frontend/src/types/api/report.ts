/**
 * 举报与申诉模块类型（第10模块）
 *
 * 字段与后端完全对应（来源：Controllers/ReportController.cs、AppealController.cs，
 * DTO 定义在 Dtos/Communication/CommunicationDtos.cs 的 WorkOrderDto / CreateReportDto / CreateAppealDto）。
 * 后端把举报和申诉统一落在 work_order 表，两者共用 WorkOrderDto。
 */

/**
 * 工单类型（后端 WorkOrderType 枚举）：1=举报，2=申诉
 */
export type WorkOrderType = 1 | 2

/**
 * 工单状态（后端 status 字符串）：waiting=待处理，processing=处理中，done=已完成
 */
export type WorkOrderStatus = 'waiting' | 'processing' | 'done'

/**
 * 举报/申诉工单（举报列表、申诉列表、详情共用）
 */
export interface WorkOrderDto {
    id: number
    /** 1=举报，2=申诉 */
    type: WorkOrderType
    reason: string
    /** 补充说明；上传附件后后端会以 "\n[附件:fileId:fileName]" 格式追加到该字段 */
    info: string | null
    status: WorkOrderStatus
    /** 处理结果（管理员填写，未处理为 null） */
    result: string | null
    /** 处理回复（管理员填写，未处理为 null） */
    response: string | null
    createTime: string
    /** 举报/申诉对象类型（如 product/user） */
    targetType: string | null
    targetId: number | null
    /** 申诉针对的工单 ID（仅申诉有） */
    appealAgainstId: number | null
}

/**
 * 发起举报请求
 */
export interface CreateReportRequest {
    /** 举报对象类型：product=商品，user=用户 */
    targetType: 'product' | 'user'
    /** 被举报的商品 ID 或用户 ID */
    targetId: number
    /** 举报原因（从原因列表选择的 code 或名称） */
    reason: string
    /** 补充说明 */
    info?: string
    /** 被举报用户 ID（举报用户时传；举报商品时后端可从商品推断） */
    accusedId?: number
    /** 关联商品 ID（举报用户且与商品相关时传） */
    productId?: number
}

/**
 * 发起申诉请求
 */
export interface CreateAppealRequest {
    reason: string
    info?: string
    /** 申诉针对的工单 ID（对举报处理结果申诉时传） */
    appealAgainstId?: number
    /** 申诉对象类型 */
    targetType?: string
    targetId?: number
}

/**
 * 举报原因选项（GET /api/report-reasons）
 */
export interface ReportReasonDto {
    code: string
    name: string
}

/**
 * 申诉类型选项（GET /api/system/appeal-types，带 description 的完整版）
 */
export interface AppealTypeDto {
    code: string
    name: string
    description: string
}

/**
 * 被举报商品信息摘要（GET /api/products/{id}/report-info，举报页回显）
 */
export interface ProductReportInfoDto {
    productId: number
    name: string
    sellerId: number
    status: string
}

/**
 * 被举报用户信息摘要（GET /api/users/{id}/report-info，举报页回显）
 */
export interface UserReportInfoDto {
    userId: number
    userName: string
    profile: string | null
}
