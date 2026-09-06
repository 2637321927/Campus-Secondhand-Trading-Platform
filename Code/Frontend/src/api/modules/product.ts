import request from "../http";
import type {
    ProductDto,
    CreateProductRequest,
    UpdateProductRequest,
    ProductImageDataDto,
    SearchProductParams,
    SearchProductResultDto
} from "../../types/api/product";

const PRODUCT_UPLOAD_TIMEOUT = 60000

export function searchProducts(params: SearchProductParams) {
    return request.get<SearchProductResultDto>(
        '/api/search/product',
        { params }
    )
}

export function getProductImages(fileIds: number[]) {
    return request.post<ProductImageDataDto[]>(
        '/api/products/images',
        fileIds
    )
}

export function getProducts(){
    return request.get<ProductDto[]>(
        `/api/products`
    )
}

export function getProductDetail(productId: number) {
    return request.get<ProductDto>(
        `/api/products/${productId}`
    )
}

export function createProduct(data: CreateProductRequest) {
    const formData = new FormData()

    formData.append('name', data.name)
    formData.append('price', String(data.price))
    formData.append('categoryId', String(data.categoryId))

    if (data.info) {
        formData.append('info', data.info)
    }

    data.images.forEach((file) => {
        formData.append('images', file)
    })

    formData.append('shippingType', String(data.shippingType))
    formData.append('allowPickup', String(data.allowPickup))

    if (data.shippingFee !== undefined && data.shippingFee !== null) {
        formData.append('shippingFee', String(data.shippingFee))
    }

    return request.post<ProductDto>(
        '/api/products',
        formData,
        {
            timeout: PRODUCT_UPLOAD_TIMEOUT
        }
    )
}

export function updateProduct(
    productId:number,
    data:UpdateProductRequest
){
    const formData=new FormData()

    formData.append('name', data.name)
    formData.append('price', String(data.price))
    formData.append('categoryId', String(data.categoryId))
    formData.append('status', String(data.status))

    if (data.info !== undefined) {
        formData.append('info', data.info)
    }

    data.newImages.forEach((file) => {
        formData.append('newImages', file)
    })

    data.toRemoveImageIds.forEach((imgFileId) => {
        formData.append('toRemoveImageIds', String(imgFileId))
    })

    formData.append('shippingType', String(data.shippingType))
    formData.append('allowPickup', String(data.allowPickup))

    if (data.shippingFee !== undefined && data.shippingFee !== null) {
        formData.append('shippingFee', String(data.shippingFee))
    }

    return request.put<ProductDto>(
        `/api/products/${productId}`,
        formData,
        {
            timeout: PRODUCT_UPLOAD_TIMEOUT
        }
    )
}

export function deleteProduct(productId:number){
    return request.delete<void>(
        `/api/products/${productId}`
    )
}
