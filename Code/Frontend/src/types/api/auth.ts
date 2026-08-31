export type UserType=0|1 //0为普通用户，1为管理员
export type BannedStatus=0|1
export type PermissionName='admin'|'user'|'seller'

//注册请求类型定义
export interface RegisterRequest {
    email:string;
    password:string;
    userName:string;
    phoneNumber?:string;
}

export interface RegisteredUser{
    userId:number;
    email:string;
    phoneNumber:string|null;
    userName:string;
    registerTime:string;
}

export interface RegisterResponse{
    message:string;
    user:RegisteredUser;
}

//登录请求类型定义
export interface LoginRequest{
    email?:string;
    phoneNumber?:string;
    password:string;
}

//登录响应类型定义
export interface LoginResponse{
    token:string;
    userId:number;
    userType:UserType;
    userName:string;
}

//当前用户类型定义
export interface CurrentUser{
    userId:number;
    userName:string;
    email:string;
    userType:UserType;
    gender:string;
    phoneNumber:string|null;
    avatarFileId:number|null;
    isBanned:BannedStatus;
    bannedUntil:string|null;
}

export interface ChangePasswordRequest{
    oldPassword:string;
    newPassword:string;
}

export interface MessageResponse{
    message:string;
}

export interface ResetPasswordRequest{
    email?:string;
    phoneNumber?:string;
}

export interface ResetPasswordConfirmRequest{
    email?:string;
    phoneNumber?:string;
    newPassword:string;
    resetToken:string;
}

export interface PermissionCheckResponse{
    hasPermission:boolean;
    permission: PermissionName;
}