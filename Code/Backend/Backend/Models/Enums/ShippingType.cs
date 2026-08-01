namespace Backend.Models.Enums;

/// <summary>
/// 发货方式
/// </summary>
public enum ShippingType
{
    Free = 0,      //包邮
    Distance = 1,  //不包邮，按距离计费
    Fixed = 2,     //不包邮，固定邮费
    None = 3       //无需邮寄
}
