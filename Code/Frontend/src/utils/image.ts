const apiBaseUrl = (
    import.meta.env.VITE_API_BASE_URL ?? ''
).replace(/\/$/, '')

export function resolveImageUrl(imageUrl: string | null ): string {
    if(!imageUrl)
        return ''
    else if (imageUrl.startsWith('http://') ||
        imageUrl.startsWith('https://') ||
        imageUrl.startsWith('data:') ||
        imageUrl.startsWith('blob:')){
            return imageUrl
        }
    else{
        const normalizedPath =
            imageUrl.startsWith('/')
                ? imageUrl
                : `/${imageUrl}`

        return `${apiBaseUrl}${normalizedPath}`
    }
}

export function resolveFileUrl(
    fileId: number | null | undefined
): string {
    if (
        !Number.isInteger(fileId) ||
        (fileId ?? 0) <= 0
    ) {
        return ''
    }

    return `${apiBaseUrl}/api/files/${fileId}`
}
