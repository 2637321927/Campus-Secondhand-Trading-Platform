import request from '../http'
import type {
    PublicUserApiResponse,
    PublicUserDto
} from '../../types/api/user'

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