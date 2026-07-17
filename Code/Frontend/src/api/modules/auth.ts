import request from '../http'
import type { 
    RegisterRequest, 
    RegisterResponse,
    LoginRequest,
    LoginResponse,
    MessageResponse,
    CurrentUser,
    ChangePasswordRequest,
    ResetPasswordRequest,
    ResetPasswordConfirmRequest,
    PermissionCheckResponse,
    PermissionName
} from '../../types/api/auth'

export function register(data:RegisterRequest){
    return request.post<RegisterResponse>(
        '/api/auth/register',
        data
    )
}

export function login(data:LoginRequest)
{
    return request.post<LoginResponse>(
        '/api/auth/login',
        data
    )
}

export function logout()
{
    return request.post<MessageResponse>(
        '/api/auth/logout'
    )
}

export function getCurrentUser()
{
    return request.get<CurrentUser>(
        '/api/auth/me'
    )
}

export function changePassword(data:ChangePasswordRequest)
{
    return request.put<MessageResponse>(
        '/api/auth/password',
        data
    )
}

export function requestPasswordReset(data:ResetPasswordRequest)
{
    return request.post<MessageResponse>(
        '/api/auth/password/reset-request',
        data
    )
}

export function confirmPasswordReset(data:ResetPasswordConfirmRequest)
{
    return request.post<MessageResponse>(
        '/api/auth/password/reset-confirm',
        data
    )
}

export function permissionCheck(permission: PermissionName)
{
    return request.get<PermissionCheckResponse>(
        '/api/auth/permission-check',
        {
            params:
            {
                permission
            }
        }
    )
}