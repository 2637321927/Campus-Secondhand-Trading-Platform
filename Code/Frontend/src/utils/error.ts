import { isAxiosError } from 'axios'

interface ApiErrorPayload {
    error?: string
    message?: string
}

/**
 * 从接口异常中提取后端返回的错误说明。
 *
 * 后端错误响应形如 { error: "..." } 或 { message: "..." }，
 * 统一优先取 error，其次取 message，都没有时回退到默认文案。
 */
export function getApiErrorMessage(
    error: unknown,
    fallback = '操作失败，请稍后重试'
): string {
    if (isAxiosError<ApiErrorPayload>(error)) {
        const data = error.response?.data

        if (data?.error) {
            return data.error
        }

        if (data?.message) {
            return data.message
        }
    }

    return fallback
}
