import { onBeforeUnmount, ref } from 'vue'
import request from '../api/http'

/**
 * 加载用户头像并转换为可展示的 Blob URL。
 *
 * 头像文件通过 /api/files/{fileId} 获取（会随请求自动携带 Token），
 * 组件卸载或重新加载时自动释放旧的 Blob URL，避免内存泄漏。
 */
export function useAvatarImage() {
    const avatarUrl = ref('')

    function clearAvatar(): void {
        if (avatarUrl.value) {
            URL.revokeObjectURL(avatarUrl.value)
            avatarUrl.value = ''
        }
    }

    async function loadAvatar(
        fileId: number | null | undefined
    ): Promise<void> {
        if (!fileId) {
            clearAvatar()
            return
        }

        try {
            const response = await request.get(
                `/api/files/${fileId}`,
                { responseType: 'blob' }
            )

            const blob = response.data as Blob
            const nextUrl = URL.createObjectURL(blob)

            clearAvatar()
            avatarUrl.value = nextUrl
        } catch (error) {
            clearAvatar()
            console.error('头像加载失败：', error)
        }
    }

    onBeforeUnmount(clearAvatar)

    return {
        avatarUrl,
        loadAvatar,
        clearAvatar
    }
}
