import request from '../http'
import type {
    AppendAppealMessageDto,
    AppealTimelineDto,
    CreateAppealDto,
    WorkOrderDto
} from '../../types/api/appeal'

// ===== 模块12：申诉中心 =====

/**
 * 发起申诉
 */
export function createAppeal(data: CreateAppealDto) {
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
 * 申诉详情
 */
export function getAppealDetail(appealId: number) {
    return request.get<WorkOrderDto>(
        `/api/appeals/${appealId}`
    )
}

/**
 * 追加补充说明
 */
export function appendAppealMessage(
    appealId: number,
    data: AppendAppealMessageDto
) {
    return request.post<WorkOrderDto>(
        `/api/appeals/${appealId}/messages`,
        data
    )
}

/**
 * 上传申诉附件
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
 * 撤销申诉
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
    return request.get<AppealTimelineDto[]>(
        `/api/appeals/${appealId}/timeline`
    )
}
