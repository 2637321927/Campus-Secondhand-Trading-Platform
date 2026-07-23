const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

export function resolveImageUrl(imageUrl: string | null ): string {
    if(!imageUrl)
        return ''
    else if (imageUrl.startsWith('http://') ||
        imageUrl.startsWith('https://')){
            return imageUrl
        }
    else{
        return `${apiBaseUrl}${imageUrl}`
    }
}

export function resolveFileUrl(
    fileId: number | null | undefined
): string {
    if (!fileId) {
        return ''
    }

    const apiBaseUrl =
        import.meta.env.VITE_API_BASE_URL.replace(/\/$/, '')

    return `${apiBaseUrl}/api/files/${fileId}`
}
