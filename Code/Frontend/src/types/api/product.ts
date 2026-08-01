export type ProductStatus = 0 | 1 | 2  //0 = 在售, 1 = 已售, 2 = 已下架

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
}

export interface ProductImageDto{
    imgFileId:number;
    imgIndex:number;
}