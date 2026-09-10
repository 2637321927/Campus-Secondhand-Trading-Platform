import { onBeforeUnmount, ref } from 'vue'
import request from '../api/http'

/**
 * 加载通用文件并转换为可展示的 Blob URL。
 *
 * 通用文件（聊天图片附件、头像等）通过 /api/files/{fileId} 获取二进制流，
 * 与商品图片的 base64 通道（/api/products/images）不同。
 * 组件卸载或重新加载时自动释放 Blob URL，避免内存泄漏。
 */
export function useFileImages() {
    const fileUrls = ref<Record<number, string>>({})
    let loadVersion = 0

    function clearFileImages(): void {
        loadVersion += 1

        for (const url of Object.values(fileUrls.value)) {
            URL.revokeObjectURL(url)
        }

        fileUrls.value = {}
    }

    async function loadFileImages(
        fileIds: Array<number | null | undefined>
    ): Promise<void> {
        const ids = [
            ...new Set(
                fileIds.filter(
                    (fileId): fileId is number =>
                        Number.isInteger(fileId) && (fileId ?? 0) > 0
                )
            )
        ]

        if (ids.length === 0) {
            return
        }

        const currentVersion = ++loadVersion
        const nextUrls: Record<number, string> = {}

        const results = await Promise.allSettled(
            ids.map(
                async (fileId): Promise<{ fileId: number; url: string }> => {
                    const response = await request.get(
                        `/api/files/${fileId}`,
                        { responseType: 'blob' }
                    )

                    return {
                        fileId,
                        url: URL.createObjectURL(response.data as Blob)
                    }
                }
            )
        )

        for (const result of results) {
            if (result.status === 'fulfilled') {
                nextUrls[result.value.fileId] = result.value.url
            }
        }

        if (currentVersion !== loadVersion) {
            for (const url of Object.values(nextUrls)) {
                URL.revokeObjectURL(url)
            }

            return
        }

        fileUrls.value = {
            ...fileUrls.value,
            ...nextUrls
        }
    }

    function getFileImageUrl(
        fileId: number | null | undefined
    ): string {
        if (!fileId) {
            return ''
        }

        return fileUrls.value[fileId] ?? ''
    }

    onBeforeUnmount(clearFileImages)

    return {
        fileUrls,
        loadFileImages,
        getFileImageUrl,
        clearFileImages
    }
}