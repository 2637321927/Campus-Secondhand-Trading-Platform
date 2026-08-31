import { onBeforeUnmount, ref } from 'vue'
import { getProductImages } from '../api/modules/product'
import type { ProductImageDataDto } from '../types/api/product'

function base64ToBlobUrl(image: ProductImageDataDto): string {
  const binary = window.atob(image.content)
  const bytes = new Uint8Array(binary.length)

  for (let index = 0; index < binary.length; index += 1) {
    bytes[index] = binary.charCodeAt(index)
  }

  const blob = new Blob(
    [bytes],
    { type: image.mimeType || 'application/octet-stream' }
  )

  return URL.createObjectURL(blob)
}

export function useProductImages() {
  const imageUrls = ref<Record<number, string>>({})
  let loadVersion = 0

  function clearProductImages(): void {
    loadVersion += 1

    for (const url of Object.values(imageUrls.value)) {
      URL.revokeObjectURL(url)
    }

    imageUrls.value = {}
  }

  async function loadProductImages(
    fileIds: Array<number | null | undefined>
  ): Promise<void> {
    const ids = [...new Set(
      fileIds.filter(
        (fileId): fileId is number =>
          Number.isInteger(fileId) && (fileId ?? 0) > 0
      )
    )]

    const currentVersion = ++loadVersion

    if (ids.length === 0) {
      clearProductImages()
      return
    }

    const response = await getProductImages(ids)
    const nextUrls: Record<number, string> = {}

    for (const image of response.data ?? []) {
      if (
        Number.isInteger(image.fileId) &&
        image.fileId > 0 &&
        image.content
      ) {
        nextUrls[image.fileId] = base64ToBlobUrl(image)
      }
    }

    if (currentVersion !== loadVersion) {
      for (const url of Object.values(nextUrls)) {
        URL.revokeObjectURL(url)
      }
      return
    }

    for (const url of Object.values(imageUrls.value)) {
      URL.revokeObjectURL(url)
    }

    imageUrls.value = nextUrls
  }

  function getProductImageUrl(
    fileId: number | null | undefined
  ): string {
    if (!fileId) {
      return ''
    }

    return imageUrls.value[fileId] ?? ''
  }

  onBeforeUnmount(clearProductImages)

  return {
    imageUrls,
    loadProductImages,
    getProductImageUrl,
    clearProductImages
  }
}
