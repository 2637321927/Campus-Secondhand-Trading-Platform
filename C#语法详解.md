# C# 语法详解 — 结合项目实例

本文档结合项目中的实际代码，讲解 C# 的核心语法和 ASP.NET Core 的常用模式。

---

## 1. `using` — 引用命名空间

```csharp
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
```

**作用**：告诉编译器，我要使用这些命名空间里的代码，就像导入库一样。

- `Backend.Data`：项目自己的数据层代码
- `Backend.Models`：项目自己的模型类
- `Microsoft.EntityFrameworkCore`：EF Core 框架

---

## 2. `namespace` — 命名空间

```csharp
namespace Backend.Repositories;
```

**作用**：给代码分类，避免类名冲突。就像文件夹一样，把相关的代码放在一起。

---

## 3. `class` — 类定义

```csharp
public class AddressRepository : IAddressRepository
```

- `public`：访问修饰符，表示这个类可以被其他类访问
- `class`：声明这是一个类
- `AddressRepository`：类名
- `: IAddressRepository`：表示这个类实现了 `IAddressRepository` 接口

**类比**：类就像一个"模板"或"蓝图"，定义了对象有什么属性和方法。

---

## 4. 字段和属性

### 字段（Field）— 类的内部变量

```csharp
private readonly AppDbContext _context;
```

- `private`：只能在这个类内部访问
- `readonly`：只读，只能在构造函数中赋值，之后不能修改
- `AppDbContext`：类型（EF Core 数据库上下文）
- `_context`：变量名（约定以下划线开头表示私有字段）

### 属性（Property）— 类的对外接口

```csharp
public int AddressId { get; set; }
public string Name { get; set; } = string.Empty;
```

- `{ get; set; }`：自动属性，编译器会自动生成背后的字段
- `= string.Empty`：属性初始化器，设置默认值为空字符串

**为什么用属性而不是字段？**
- 属性可以添加访问控制逻辑（比如验证）
- 属性可以被数据绑定框架使用
- 属性可以有不同的 get 和 set 访问级别

---

## 5. 构造函数 — 创建对象时自动调用

```csharp
public AddressRepository(AppDbContext context) => _context = context;
```

这是**简化写法**，等同于：

```csharp
public AddressRepository(AppDbContext context)
{
    _context = context;
}
```

**作用**：当创建 `AddressRepository` 对象时，必须传入一个 `AppDbContext` 对象，这就是**依赖注入**。

---

## 6. `async/await` — 异步编程

```csharp
public async Task<Address?> GetByIdAsync(int addressId)
    => await _context.Addresses.FindAsync(addressId);
```

### 什么是异步？

**同步**：你去餐厅吃饭，点完菜后一直等着，直到菜上齐才能做别的事。

**异步**：你去餐厅吃饭，点完菜后可以玩手机、聊天，等服务员喊你时再去拿菜。

### 关键语法

- `async`：标记这是一个异步方法
- `Task<T>`：表示这个方法会返回一个"任务"，任务完成后会得到 `T` 类型的结果
- `await`：等待任务完成，但在此期间可以做其他事
- `?`：可空类型，表示可能返回 `null`

**代码解释**：

```csharp
public async Task<Address?> GetByIdAsync(int addressId)
```
↑ 声明一个异步方法，返回一个 `Address` 对象或 `null`

```csharp
=> await _context.Addresses.FindAsync(addressId);
```
↑ 等待数据库查询完成，然后返回结果

### 为什么要用异步？

- **提高性能**：服务器可以同时处理多个请求，不用等一个请求完成
- **避免阻塞**：UI 不会卡住，用户体验更好

---

## 7. Lambda 表达式 — 简化写法

```csharp
public async Task<List<Address>> GetByUserIdAsync(int userId)
    => await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
```

这里的 `a => a.UserId == userId` 就是 Lambda 表达式。

**理解方式**：

```csharp
a => a.UserId == userId
// ↑ 参数  ↑ 表达式体
```

等同于：

```csharp
(Address a) => { return a.UserId == userId; }
```

**`Where` 方法**：过滤集合，只保留满足条件的元素。

**`ToListAsync()`**：把查询结果转换成 `List<Address>`。

---

## 8. EF Core 特性（Attributes）

```csharp
[Table("address")]
public class Address
{
    [Key]
    [Column("address_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AddressId { get; set; }

    [Column("name")]
    [MaxLength(10)]
    public string Name { get; set; } = string.Empty;
}
```

这些是**特性**，用来给类和属性添加元数据。

| 特性 | 作用 |
|------|------|
| `[Table("address")]` | 指定对应的数据库表名 |
| `[Key]` | 指定这是主键 |
| `[Column("address_id")]` | 指定数据库中的列名 |
| `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` | 指定这是自增主键 |
| `[MaxLength(10)]` | 指定字符串最大长度 |
| `[ForeignKey("UserId")]` | 指定外键关联 |

---

## 9. 依赖注入 — 核心概念

在 [Program.cs](file:///d:/数据库课设/Campus-Secondhand-Trading-Platform/Code/Backend/Backend/Program.cs) 中：

```csharp
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
```

**作用**：告诉系统，当需要 `IAddressRepository` 接口时，自动创建一个 `AddressRepository` 对象。

**好处**：
- 解耦：类之间不直接依赖，而是依赖接口
- 方便测试：可以替换成测试用的实现
- 自动管理对象生命周期

---

## 10. 对象初始化器

```csharp
var baseUser = new BaseUser
{
    Email = dto.Email,
    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
    PhoneNumber = dto.PhoneNumber,
    UserType = 0,
    Gender = "unknown",
    RegisterTime = DateTime.Now
};
```

**作用**：创建对象时直接设置属性值，不用一个个赋值。

---

## 11. 空合并运算符 `?.` 和 `??`

```csharp
if (normUser?.BaseUser == null) return null;
```

- `?.`：安全访问运算符，如果 `normUser` 不为 `null`，才访问 `.BaseUser`
- `??`：空合并运算符，如果左边为 `null`，返回右边的值

**等价于**：

```csharp
if (normUser != null && normUser.BaseUser == null) return null;
```

---

## 完整流程示例

以用户注册为例，看完整的代码流程：

```csharp
// 1. Controller 接收请求
[HttpPost("register")]
public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
{
    var user = await _userService.RegisterAsync(dto);  // 调用 Service
    return CreatedAtAction(nameof(GetById), new { id = user.UserId }, user);
}

// 2. Service 处理业务逻辑
public async Task<UserDto> RegisterAsync(RegisterDto dto)
{
    var baseUser = new BaseUser { ... };  // 创建基础用户对象
    await _userRepo.AddAsync(baseUser);   // 调用 Repository 保存
    await _userRepo.SaveAsync();          // 提交到数据库

    var normUser = new NormUser { ... };  // 创建普通用户对象
    await _normUserRepo.AddAsync(normUser);
    await _normUserRepo.SaveAsync();

    return new UserDto { ... };  // 返回 DTO
}

// 3. Repository 操作数据库
public async Task AddAsync(BaseUser user)
    => await _context.BaseUsers.AddAsync(user);
```

---

## 语法速查表

| 语法 | 作用 | 示例 |
|------|------|------|
| `using` | 引用命名空间 | `using Backend.Models;` |
| `namespace` | 定义命名空间 | `namespace Backend.Repositories;` |
| `public` | 公开访问 | `public class Address` |
| `private` | 私有访问 | `private readonly AppDbContext _context;` |
| `{ get; set; }` | 自动属性 | `public string Name { get; set; }` |
| `async` | 标记异步方法 | `public async Task<UserDto> RegisterAsync()` |
| `await` | 等待异步操作 | `await _userRepo.AddAsync(user);` |
| `=>` | Lambda/表达式体 | `x => x * 2` |
| `?` | 可空类型 | `Address?` |
| `?.` | 安全访问 | `normUser?.BaseUser` |
| `[Attribute]` | 特性 | `[Table("address")]` |
