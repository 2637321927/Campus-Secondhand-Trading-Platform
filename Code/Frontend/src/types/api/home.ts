import type { ProductCardDto } from './product'
import type { CategoryDto } from './category'

export interface UserQuickEntryDto{
    favoriteCount:number;
    publishedProductCount:number;
    unreadMessageCount:number;
}

export interface HomeResponseDto{
    recommendedProducts:ProductCardDto[];
    categories:CategoryDto[];
    userQuickEntry:UserQuickEntryDto|null;
}