import request from '../http'
import type {
    CreateReportDto,
    ReportProductInfoDto,
    ReportReason,
    ReportUserInfoDto,
    WorkOrderDto
} from '../../types/api/report'

// ===== 模块11：举报 =====

/**
 * 获取举报原因列表
 */
export function getReportReasons() {
    return request.get<ReportReason[]>(
        '/api/report-reasons'
    )
}

/**
 * 发起举报
 */
export function createReport(data: CreateReportDto) {
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
 * 举报详情
 */
export function getReportDetail(reportId: number) {
    return request.get<WorkOrderDto>(
        `/api/reports/${reportId}`
    )
}

/**
 * 撤销举报
 */
export function cancelReport(reportId: number) {
    return request.patch<void>(
        `/api/reports/${reportId}/cancel`
    )
}

/**
 * 上传举报附件
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
 * 被举报商品的信息摘要
 */
export function getProductReportInfo(productId: number) {
    return request.get<ReportProductInfoDto>(
        `/api/products/${productId}/report-info`
    )
}

/**
 * 被举报用户的信息摘要
 */
export function getUserReportInfo(userId: number) {
    return request.get<ReportUserInfoDto>(
        `/api/users/${userId}/report-info`
    )
}
