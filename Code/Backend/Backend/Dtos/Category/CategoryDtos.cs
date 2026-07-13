namespace Backend.Dtos.Category;
///<summary>
///创建分类请求（只是示例，目前好像没有创建分类的需要，直接在数据库里创建就行了）
///</summary>
public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

///<summary>
///分类信息响应
///</summary>
public class CategoryDto
{
    public int CategoryId { get; set; } 
    public string CategoryName { get; set; }= string.Empty;
    public int? ParentId { get; set; }
    public string? ParentName { get; set; }
    //我觉得带上parentname算是合理的冗余
}