import type {
    WorkOrderDto
} from './report'

// re-export，供 api 模块与其他页面按模块导入 WorkOrderDto
export type { WorkOrderDto }

/**
 * 发起申诉请求
 */
export interface CreateAppealDto {
    reason: string
    info?: string | null
    appealAgainstId?: number | null
    targetType?: string | null
    targetId?: number | null
}

/**
 * 申诉补充说明请求
 */
export interface AppendAppealMessageDto {
    message: string
}

/**
 * 申诉时间线条目
 */
export interface AppealTimelineDto {
    timelineId: number
    workOrderId: number
    action: string
    note: string | null
    adminId: number | null
    createTime: string
}

/**
 * 申诉详情复用 WorkOrderDto
 */
export type AppealDto = WorkOrderDto
