namespace Backend.Dtos.Collection;

///<summary>
///收藏状态响应（Toggle后返回）
///</summary>
public class CollectionStatusResponse
{
    ///<summary>
    ///true=已收藏，false=未收藏
    ///</summary>
    public bool IsCollected { get; set; }
}
