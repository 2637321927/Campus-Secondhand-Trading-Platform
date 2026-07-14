namespace Backend.Dtos.Home;
using Backend.Dtos.Product;
using Backend.Dtos.Category;
///<summary>
///主页响应
///</summary>
public class HomeResponseDto
{
    public List<ProductCardDto> RecommendedProducts { get; set; } 
    public List<CategoryDto> Categories { get; set; }= new List<CategoryDto>();  
    public UserQuickEntryDto? UserQuickEntry { get; set; }//登录信息（如登则展示个人）    
}
///<summary>
///用户快捷入口响应
///</summary>
public class UserQuickEntryDto
{
    public int FavoriteCount { get; set; }
    public int UnreadMessageCount { get; set; }
    public int PublishedProductCount { get; set; }
}