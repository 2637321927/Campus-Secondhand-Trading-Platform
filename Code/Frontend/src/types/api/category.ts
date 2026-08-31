export interface CategoryDto{
    categoryId:number;
    categoryName:string;
    parentId:number|null;
    parentName:string|null;
    children?:CategoryDto[];
}