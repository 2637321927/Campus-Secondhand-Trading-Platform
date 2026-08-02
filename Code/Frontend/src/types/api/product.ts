/**
 * 商品状态：
 * 0 = 在售
 * 1 = 已售
 * 2 = 已下架
 */
export type ProductStatus = 0 | 1 | 2
export type ShippingType = 0 | 1 | 2 | 3

export interface ProductCardDto{
    productId:number;
    name:string;
    price:number;
    coverImageFileId?:number|null;
    sellerName:string;
    releaseDate:string;
    viewCount:number;
}

export interface SearchProductParams {
    keyword: string
    searchId?: string
    page?: number
    pageSize?: number
    sortBy?: 'relevance' | 'latest' | 'price_asc' | 'price_desc'
}

export interface SearchProductResultDto {
    searchId: string
    items: ProductCardDto[]
    totalCount: number
    page: number
    pageSize: number
    totalPages: number
    expandedTerms: string[]
}

export interface ProductListItemDto {
    productId: number
    name: string
    price: number
    viewCount: number
    coverImageFileId?: number | null
    status?: ProductStatus
    categoryName?: string | null
    info?: string | null
    images?: ProductImageDto[]
}

export interface ProductDto{
    productId: number;
    name: string;
    price: number;
    info?:string|null;
    status:ProductStatus;
    userId:number;
    categoryId:number;
    categoryName?:string|null;
    viewCount: number;
    images:ProductImageDto[];
    releaseDate: string;
    shippingType: ShippingType;
    shippingFee?: number|null;
    allowPickup: 0 | 1;
}

export interface ProductImageDto{
    imgFileId: number; //商品图片记录主键，同时也是实际文件 ID。
    imgIndex: number; //图片展示顺序，数值越小越靠前。
}

export interface ProductImageDataDto {
    fileId: number
    fileName: string
    mimeType: string
    content: string
}

export interface CreateProductRequest {
    name: string
    price: number
    info?: string
    categoryId: number
    images: File[]

    shippingType: ShippingType
    shippingFee?: number | null
    allowPickup: 0 | 1
}
export interface UpdateProductRequest {
    name: string
    price: number
    info?: string
    categoryId: number
    status: ProductStatus
    newImages: File[] //本次编辑中新选择的图片文件。

    toRemoveImageIds: number[] //待删除图片的 imgFileId 列表。

    shippingType: ShippingType
    shippingFee?: number | null
    allowPickup: 0 | 1
}
