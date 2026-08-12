namespace Backend.Dtos.User;

public class AddressDto
{
    public int AddressId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class CreateAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
