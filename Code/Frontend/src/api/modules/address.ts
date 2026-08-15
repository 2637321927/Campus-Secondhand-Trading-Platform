import request from '../http'
import type {
    AddressDto,
    CreateAddressRequest,
    UpdateAddressRequest
} from '../../types/api/address'

// ===== 第6模块：地址管理 =====

/**
 * 获取当前用户地址列表（默认地址排最前）
 */
export function getMyAddresses() {
    return request.get<AddressDto[]>(
        '/api/users/me/addresses'
    )
}

/**
 * 新增收货或交易地址
 */
export function createAddress(data: CreateAddressRequest) {
    return request.post<AddressDto>(
        '/api/users/me/addresses',
        data
    )
}

/**
 * 获取单个地址详情
 */
export function getAddress(addressId: number) {
    return request.get<AddressDto>(
        `/api/users/me/addresses/${addressId}`
    )
}

/**
 * 修改地址
 */
export function updateAddress(
    addressId: number,
    data: UpdateAddressRequest
) {
    return request.put<AddressDto>(
        `/api/users/me/addresses/${addressId}`,
        data
    )
}

/**
 * 删除地址
 */
export function deleteAddress(addressId: number) {
    return request.delete<void>(
        `/api/users/me/addresses/${addressId}`
    )
}

/**
 * 设置指定地址为默认地址
 */
export function setDefaultAddress(addressId: number) {
    return request.patch<AddressDto>(
        `/api/users/me/addresses/${addressId}/default`
    )
}

/**
 * 获取当前用户的默认地址
 */
export function getDefaultAddress() {
    return request.get<AddressDto>(
        '/api/users/me/default-address'
    )
}
