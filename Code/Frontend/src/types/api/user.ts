export interface PublicUserDto {
    userId: number
    userName: string
}

export interface PublicUserApiResponse {
    userId: number
    userName: string
    email?: string
    phoneNumber?: string | null
    registerTime?: string
}