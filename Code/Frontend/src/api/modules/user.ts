import request from '../http'
import type {
    PublicUserApiResponse,
    PublicUserDto,
    UserProfileDto,
    UpdateProfileRequest,
    AvatarUploadResponse,
    PurchaseDto,
    UserDto,
    BrowseHistoryDto
} from '../../types/api/user'
import type {
    ProductDto,
    SearchProductParams,
    SearchProductResultDto
} from '../../types/api/product'

export async function getPublicUser(
    userId: number
): Promise<PublicUserDto> {
    const response = await request.get<PublicUserApiResponse>(
        `/api/users/${userId}`
    )

    return {
        userId: response.data.userId,
        userName: response.data.userName
    }
}

export function getUserProductIds(userId: number) {
    return request.get<number[]>(
        `/api/products/user/${userId}`
    )
}

// ===== 第5模块：个人中心与用户资料 =====

/**
 * 获取当前用户个人中心资料
 */
export function getMyProfile() {
    return request.get<UserProfileDto>(
        '/api/users/me/profile'
    )
}

/**
 * 修改当前用户个人中心资料
 */
export function updateMyProfile(data: UpdateProfileRequest) {
    return request.put<UserProfileDto>(
        '/api/users/me/profile',
        data
    )
}

/**
 * 上传/更换当前用户头像（multipart/form-data，字段名为 file）
 */
export function uploadAvatar(file: File) {
    const formData = new FormData()
    formData.append('file', file)

    return request.post<AvatarUploadResponse>(
        '/api/users/me/avatar',
        formData
    )
}

/**
 * 当前用户「我发布」的商品列表
 */
export function getMyPublishedProducts() {
    return request.get<ProductDto[]>(
        '/api/users/me/published-products'
    )
}

/**
 * 当前用户「我卖出」的订单列表
 */
export function getMySoldOrders() {
    return request.get<PurchaseDto[]>(
        '/api/users/me/sold-orders'
    )
}

/**
 * 当前用户「我购买」的订单列表
 */
export function getMyPurchaseOrders() {
    return request.get<PurchaseDto[]>(
        '/api/users/me/purchase-orders'
    )
}

/**
 * 查看其他用户公开主页信息
 */
export function getUserById(userId: number) {
    return request.get<UserDto>(
        `/api/users/${userId}`
    )
}

/**
 * 查看某用户已发布商品
 */
export function getUserProducts(userId: number) {
    return request.get<ProductDto[]>(
        `/api/users/${userId}/products`
    )
}

/**
 * 查看某用户已卖出商品
 */
export function getUserSoldProducts(userId: number) {
    return request.get<ProductDto[]>(
        `/api/users/${userId}/sold-products`
    )
}

/**
 * 在某用户主页内搜索其发布的商品
 */
export function searchUserProducts(
    userId: number,
    params: SearchProductParams
) {
    return request.get<SearchProductResultDto>(
        `/api/users/${userId}/product-search`,
        { params }
    )
}

/**
 * 获取当前用户浏览历史
 */
export function getMyBrowseHistory() {
    return request.get<BrowseHistoryDto[]>(
        '/api/users/me/browse-history'
    )
}

/**
 * 清空当前用户浏览历史
 */
export function clearMyBrowseHistory() {
    return request.delete<void>(
        '/api/users/me/browse-history'
    )
}

/**
 * 删除某条商品浏览历史（按商品 ID）
 */
export function deleteBrowseHistoryItem(productId: number) {
    return request.delete<void>(
        `/api/users/me/browse-history/${productId}`
    )
}
