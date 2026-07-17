# 校园二手交易平台 - API 文档

> 版本：v2.0 | 更新日期：2026-07-17  
> 基础地址：`http://localhost:5141`  
> Swagger：`http://localhost:5141/swagger`

---

## 目录

- [一、认证模块](#一认证模块)
- [二、首页模块](#二首页模块)
- [三、分类模块](#三分类模块)
- [四、商品模块](#四商品模块)
- [五、收藏模块](#五收藏模块)
- [六、商品留言模块](#六商品留言模块)
- [七、用户模块](#七用户模块)
- [八、认证说明](#八认证说明)

---

## 一、认证模块

> 基础路径：`/api/auth`

### 1. 注册

```
POST /api/auth/register
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string | ✅ | 邮箱，不可重复 |
| password | string | ✅ | 明文密码，后端 BCrypt 加密 |
| userName | string | ✅ | 昵称 |
| phoneNumber | string | ❌ | 手机号 |

**请求示例：**
```json
{
  "email": "test@example.com",
  "password": "123456",
  "userName": "小明",
  "phoneNumber": "13800138000"
}
```

**成功响应：** `201 Created`
```json
{
  "message": "注册成功",
  "user": { "userId": 1, "email": "test@example.com", "userName": "小明" }
}
```

> 邮箱/手机号已注册 → `409 Conflict`

---

### 2. 登录

```
POST /api/auth/login
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| email | string? | 二选一 | 邮箱 |
| phoneNumber | string? | 二选一 | 手机号 |
| password | string | ✅ | 密码 |

**成功响应：** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "userId": 1,
  "userType": 0,
  "userName": "小明"
}
```

---

### 3. 退出登录

```
POST /api/auth/logout    🔒 需要登录
```

**成功响应：** `200 OK` → `{ "message": "已退出登录" }`

---

### 4. 获取当前用户信息

```
GET /api/auth/me    🔒 需要登录
```

**成功响应：** `200 OK`
```json
{
  "userId": 1, "email": "test@example.com", "userName": "小明",
  "userType": 0, "gender": "unknown", "avatarFileId": null,
  "isBanned": 0, "bannedUntil": null
}
```

---

### 5. 修改密码

```
PUT /api/auth/password    🔒 需要登录
```

| 参数 | 类型 | 说明 |
|------|------|------|
| oldPassword | string | 原密码 |
| newPassword | string | 新密码 |

> 原密码错误 → `400 Bad Request`

---

### 6. 发起重置密码

```
POST /api/auth/password/reset-request
```

| 参数 | 类型 | 说明 |
|------|------|------|
| email | string? | 二选一 |
| phoneNumber | string? | 二选一 |

> 验证码打印到控制台，有效期 15 分钟

---

### 7. 确认重置密码

```
POST /api/auth/password/reset-confirm
```

| 参数 | 类型 | 说明 |
|------|------|------|
| email / phoneNumber | string | 与上一步相同 |
| resetToken | string | 6位数字验证码 |
| newPassword | string | 新密码 |

---

### 8. 权限校验

```
GET /api/auth/permission-check?permission=admin    🔒 需要登录
```

---

## 二、首页模块

> 基础路径：`/api/home`

### 9. 首页聚合数据

```
GET /api/home
```

**响应：** `200 OK`
```json
{
  "recommendedProducts": [ /* ProductCardDto[] - 最新10条 */ ],
  "categories": [ /* CategoryDto[] */ ],
  "userQuickEntry": {
    "favoriteCount": 5,
    "publishedProductCount": 3,
    "unreadMessageCount": 0
  }
}
```

### 10. 推荐商品

```
GET /api/home/recommended-products
```

最新上架 10 个在售商品，返回 `ProductCardDto[]`

### 11. 热门商品

```
GET /api/home/hot-products
```

浏览量最高 20 个在售商品，返回 `ProductCardDto[]`

### ProductCardDto

| 字段 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |
| name | string | 商品名 |
| price | decimal | 价格 |
| coverImageUrl | string? | 封面图URL |
| sellerName | string | 卖家昵称 |
| releaseDate | datetime | 发布时间 |
| viewCount | int | 浏览量 |

---

## 三、分类模块

> 基础路径：`/api/categories`

### 12. 全部分类

```
GET /api/categories
```

返回 `CategoryDto[]`

| 字段 | 类型 | 说明 |
|------|------|------|
| categoryId | long | 分类ID |
| categoryName | string | 分类名 |
| parentId | long? | 父分类ID |
| parentName | string? | 父分类名 |
| children | CategoryDto[] | 子分类列表 |

### 13. 单个分类

```
GET /api/categories/{categoryId}
```

### 14. 子分类列表

```
GET /api/categories/{categoryId}/children
```

### 15. 分类下商品

```
GET /api/categories/{categoryId}/products
```

返回 `ProductDto[]`（含浏览量）

---

## 四、商品模块

> 基础路径：`/api/products`

### 16. 商品详情

```
GET /api/products/{productId}
```

🔒 登录后自动记录浏览（自己看自己的不记录）

**ProductDto：**
| 字段 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |
| name | string | 商品名 |
| price | decimal | 价格 |
| info | string? | 描述 |
| status | int | 0=在售, 1=已售, 2=已下架 |
| userId | int | 卖家ID |
| categoryId | long | 分类ID |
| categoryName | string? | 分类名 |
| viewCount | int | 浏览量 |
| images | ProductImageDto[] | 图片列表 |

### 17. 记录浏览

```
POST /api/products/{productId}/view-record    🔒
```

无请求体，返回 `204 No Content`

### 18. 发布商品

```
POST /api/products    🔒
```

**multipart/form-data：**

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| name | string | ✅ | 商品名 |
| price | decimal | ✅ | 价格 |
| info | string | ❌ | 描述 |
| categoryId | long | ✅ | 分类ID |
| images | file[] | ❌ | 图片 |

返回 `201 Created` → `ProductDto`

### 19. 修改商品

```
PUT /api/products/{productId}    🔒 仅卖家
```

**multipart/form-data：**

| 参数 | 类型 | 说明 |
|------|------|------|
| name | string | 商品名 |
| price | decimal | 价格 |
| info | string | 描述 |
| status | int | 0/1/2 |
| categoryId | long | 分类ID |
| newImages | file[] | 新增图片 |
| toRemoveImageIds | long[] | 要删除的图片ID |

返回 `200 OK` → `ProductDto`

### 20. 删除商品

```
DELETE /api/products/{productId}    🔒 仅卖家
```

返回 `204 No Content`

---

## 五、收藏模块

> 基础路径：`/api/collections`

### 21. 收藏/取消（Toggle）

```
POST /api/collections/{productId}    🔒
```

无请求体，自动判断。返回 `{ "isCollected": true/false }`

> 不能收藏自己的商品 → `400`

### 22. 查询收藏状态

```
GET /api/collections/{productId}    🔒
```

返回 `{ "isCollected": true/false }`

### 23. 我的收藏列表

```
GET /api/collections    🔒
```

返回 `ProductCardDto[]`（含封面图、卖家名、浏览量）

### 24. 收藏搜索

```
GET /api/collections/search?keyword=手机    🔒
```

按商品名模糊搜索，不区分大小写，返回 `ProductCardDto[]`

### 25. 批量取消收藏

```
DELETE /api/collections    🔒
```

**请求体：** `[1, 3, 5]` — productId 数组  
返回 `{ "deleted": 3 }`

### 26. 收藏总数

```
GET /api/collections/count    🔒
```

返回 `{ "count": 12 }`

---

## 六、商品留言模块

> 基础路径：`/api/products/{productId}/comments`

### 27. 留言列表

```
GET /api/products/{productId}/comments
```

返回 `ProductCommentDto[]`（支持嵌套回复）：

| 字段 | 类型 | 说明 |
|------|------|------|
| commentId | long | 留言ID |
| content | string | 内容 |
| userName | string | 用户名 |
| createTime | datetime | 时间 |
| parentId | long? | 回复目标（null=顶级） |
| replies | ProductCommentDto[] | 子回复列表 |

### 28. 发表留言

```
POST /api/products/{productId}/comments    🔒
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| content | string | ✅ | 内容 |
| responseToId | long? | ❌ | 回复哪条留言 |

### 29. 删除留言

```
DELETE /api/products/{productId}/comments/{commentId}    🔒 仅本人
```

---

## 七、用户模块

### 30. 查看用户主页

```
GET /api/users/{userId}
```

返回：`{ "userId": 1, "email": "...", "userName": "小明", ... }`

---

## 八、认证说明

| 标记 | 含义 |
|------|------|
| 🔒 | 需 Header `Authorization: Bearer {token}` |
| 无标记 | 公开接口 |

Token 有效期 72 小时。

---

> 📊 已实现接口：**30 个** | 接口定义清单见 `二手商品平台后端接口定义.md`
