/**
 * 文件上传响应
 */
export interface FileUploadDto {
    fileId: number
    fileName: string
    fileSize: number
    contentType: string
}
