import request from '../http'
import type {
    CategoryDto
} from '../../types/api/category'
import type {
    ProductDto
} from '../../types/api/product'

export function getCategories(){
    return request.get<CategoryDto[]>(
        '/api/categories'
    )
}

export function getCategoryDetail(categoryId:number){
    return request.get<CategoryDto>(
        `/api/categories/${categoryId}`
    )
}

export function getCategoryChildren(categoryId: number){
    return request.get<CategoryDto[]>(
        `/api/categories/${categoryId}/children`
    )
}

export function getCategoryProducts(categoryId: number){
    return request.get<ProductDto[]>(
        `/api/categories/${categoryId}/products`
    )
}