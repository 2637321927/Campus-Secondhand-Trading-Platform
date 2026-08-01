/**
 * 商品状态：
 * 0 = 在售
 * 1 = 已售
 * 2 = 已下架
 * 3 = 草稿
 *
 * 状态 3 为前端先行约定，联调时需要与后端确认最终枚举值。
 */
export type ProductStatus = 0 | 1 | 2 | 3

export interface ProductCardDto{
    productId:number;
    name:string;
    price:number;
    coverImageUrl?:string|null;
    sellerName:string;
    releaseDate:string;
    viewCount:number;
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
    releaseDate?: string;
    shippingMethodId?: number|null;
    addressId?: number|null;
}

export interface ProductImageDto{
    imgFileId: number; //实际文件 ID，用于拼接图片访问地址
    imgIndex: number; //图片展示顺序，数值越小越靠前。
    imageId?: number //商品图片关系 ID。
}

export interface CreateProductRequest {
    name: string
    price: number
    info?: string
    categoryId: number
    images: File[]

    /**
     * 以下字段来自完整商品发布规划，
     * 具体字段名和数据格式联调时再与后端统一。
     */
    shippingMethodId?: number
    addressId?: number

    /**
     * true 表示保存为草稿，false 表示正式发布。
     */
    saveAsDraft?: boolean
}

export interface UpdateProductRequest {
    name: string
    price: number
    info?: string
    categoryId: number
    status: ProductStatus

    newImages: File[] //本次编辑中新选择的图片文件。

    toRemoveImageIds: number[] //用户准备删除的已有商品图片 ID。

    shippingMethodId?: number
    addressId?: number
}

export interface UpdateProductStatusRequest {
    status: ProductStatus
}

export interface UploadProductImagesRequest {
    images: File[]
}

export interface SortProductImagesRequest {
    imageIds: number[] //按目标展示顺序排列的商品图片关系 ID。
}
