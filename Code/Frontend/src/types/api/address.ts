/**
 * 地址响应 DTO
 */
export interface AddressDto {
    addressId: number
    name: string
    phoneNumber: string
    detailAddress: string
    isDefault: boolean
}

/**
 * 新增地址请求
 */
export interface CreateAddressRequest {
    name: string
    phoneNumber: string
    detailAddress: string
    isDefault: boolean
}

/**
 * 修改地址请求（部分更新，字段为 null 时不修改）
 */
export interface UpdateAddressRequest {
    name?: string | null
    phoneNumber?: string | null
    detailAddress?: string | null
}
