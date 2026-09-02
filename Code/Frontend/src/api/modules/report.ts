import request from '../http'
import type {
    WorkOrderDto,
    CreateReportRequest,
    CreateAppealRequest,
    ReportReasonDto,
    AppealTypeDto,
    ProductReportInfoDto,
    UserReportInfoDto
} from '../../types/api/report'

// ===== 举报与申诉模块 =====

/**
 * 获取举报原因列表（写死四种）
 */
export function getReportReasons() {
    return request.get<ReportReasonDto[]>(
        '/api/report-reasons'
    )
}

/**
 * 获取申诉类型列表（带 description 的完整版）
 */
export function getAppealTypes() {
    return request.get<AppealTypeDto[]>(
        '/api/system/appeal-types'
    )
}

/**
 * 发起举报
 */
export function createReport(data: CreateReportRequest) {
    return request.post<WorkOrderDto>(
        '/api/reports',
        data
    )
}

/**
 * 我发起的举报列表
 */
export function getMyReports() {
    return request.get<WorkOrderDto[]>(
        '/api/reports/me'
    )
}

/**
 * 举报详情（仅发起人可见）
 */
export function getReport(reportId: number) {
    return request.get<WorkOrderDto>(
        `/api/reports/${reportId}`
    )
}

/**
 * 撤销举报（已处理的不能撤）
 */
export function cancelReport(reportId: number) {
    return request.patch<void>(
        `/api/reports/${reportId}/cancel`
    )
}

/**
 * 为举报上传附件（multipart/form-data，字段名 file）
 * 后端把文件 ID/名追加到工单 Info 字段
 */
export function uploadReportAttachment(
    reportId: number,
    file: File
) {
    const formData = new FormData()
    formData.append('file', file)

    return request.post<WorkOrderDto>(
        `/api/reports/${reportId}/attachments`,
        formData
    )
}

/**
 * 被举报商品信息摘要（举报页回显）
 */
export function getProductReportInfo(productId: number) {
    return request.get<ProductReportInfoDto>(
        `/api/products/${productId}/report-info`
    )
}

/**
 * 被举报用户信息摘要（举报页回显）
 */
export function getUserReportInfo(userId: number) {
    return request.get<UserReportInfoDto>(
        `/api/users/${userId}/report-info`
    )
}

// ==================== 申诉 ====================

/**
 * 发起申诉
 */
export function createAppeal(data: CreateAppealRequest) {
    return request.post<WorkOrderDto>(
        '/api/appeals',
        data
    )
}

/**
 * 我发起的申诉列表
 */
export function getMyAppeals() {
    return request.get<WorkOrderDto[]>(
        '/api/appeals/me'
    )
}

/**
 * 申诉详情（仅发起人可见）
 */
export function getAppeal(appealId: number) {
    return request.get<WorkOrderDto>(
        `/api/appeals/${appealId}`
    )
}

/**
 * 为申诉追加补充说明（追加到工单 Info 字段）
 */
export function appendAppealMessage(
    appealId: number,
    message: string
) {
    return request.post<WorkOrderDto>(
        `/api/appeals/${appealId}/messages`,
        { message }
    )
}

/**
 * 为申诉上传附件（multipart/form-data，字段名 file）
 */
export function uploadAppealAttachment(
    appealId: number,
    file: File
) {
    const formData = new FormData()
    formData.append('file', file)

    return request.post<WorkOrderDto>(
        `/api/appeals/${appealId}/attachments`,
        formData
    )
}

/**
 * 撤销申诉（已处理的不能撤）
 */
export function cancelAppeal(appealId: number) {
    return request.patch<void>(
        `/api/appeals/${appealId}/cancel`
    )
}

/**
 * 申诉处理时间线
 */
export function getAppealTimeline(appealId: number) {
    return request.get<unknown[]>(
        `/api/appeals/${appealId}/timeline`
    )
}
