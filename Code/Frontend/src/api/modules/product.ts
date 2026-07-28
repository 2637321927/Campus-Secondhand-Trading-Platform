import request from "../http";
import type { ProductDto } from "../../types/api/product";

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