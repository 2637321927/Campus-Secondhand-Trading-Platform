import request from '../http'
import type {
    HomeResponseDto
} from '../../types/api/home'
import type {
    ProductCardDto
} from '../../types/api/product'

export function getHomeData(){
    return request.get<HomeResponseDto>(
        '/api/home'
    )
}

export function getRecommendedProducts(){
    return request.get<ProductCardDto[]>(
        '/api/home/recommended-products'
    )
}

export function getHotProducts(){
    return request.get<ProductCardDto[]>(
        '/api/home/hot-products'
    )
}