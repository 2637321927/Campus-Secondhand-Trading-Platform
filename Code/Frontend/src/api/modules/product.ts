import request from "../http";
import type {
    ProductDto,
    CreateProductRequest,
    UpdateProductRequest,
    UpdateProductStatusRequest,
    UploadProductImagesRequest,
    SortProductImagesRequest
} from "../../types/api/product";

const PRODUCT_UPLOAD_TIMEOUT = 60000

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

    if (data.shippingMethodId!==undefined) {
        formData.append('shippingMethodId', String(data.shippingMethodId))
    }

    if (data.addressId!==undefined) {
        formData.append('addressId', String(data.addressId))
    }

    if(data.saveAsDraft!==undefined){
        formData.append('saveAsDraft',String(data.saveAsDraft))
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
    formData.append('status',String(data.status))

    if (data.info !== undefined) {
        formData.append('info', data.info)
    }

    data.newImages.forEach((file) => {
        formData.append('newImages', file)
    })

    data.toRemoveImageIds.forEach((imageId) => {
        formData.append('toRemoveImageIds', String(imageId))
    })

    if(data.shippingMethodId!==undefined){
        formData.append('shippingMethodId',String(data.shippingMethodId))
    }

    if(data.addressId!==undefined){
        formData.append('addressId',String(data.addressId))
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

export function updateProductStatus(
    productId:number,
    data:UpdateProductStatusRequest
){
    return request.patch<ProductDto>(
        `/api/products/${productId}/status`,
        data
    )
}

export function uploadProductImages(
    productId:number,
    data:UploadProductImagesRequest
){
    const formData=new FormData()

    data.images.forEach((file) => {
        formData.append('images', file)
    })

    return request.post(
        `/api/products/${productId}/images`,
        formData,
        {
            timeout: PRODUCT_UPLOAD_TIMEOUT
        }
    )
}

export function sortProductImages(
    productId:number,
    data:SortProductImagesRequest
){
    return request.put(
        `/api/products/${productId}/images/sort`,
        data
    )
}

export function deleteProductImage(
    productId:number,
    imageId:number
){
    return request.delete<void>(
        `/api/products/${productId}/images/${imageId}`
    )
}
