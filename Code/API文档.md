# 校园二手交易平台 - API 文档

> 版本：v2.1 | 更新日期：2026-08-18  
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
- [九、管理员用户模块](#九管理员用户模块)
- [十、管理员商品模块](#十管理员商品模块)
- [十一、管理员举报与申诉模块](#十一管理员举报与申诉模块)

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

> 新商品发布后默认状态为 `3 待审核 PendingReview`，管理员审核通过后变为 `0 在售`

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
| status | int | 0=在售，1=已售，2=下架，3=待审核，4=驳回 |
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

## 九、管理员用户模块

> 基础路径：`/api/admin/users`  
> 所有接口需要管理员 Token：`Authorization: Bearer {token}`，非管理员返回 403

### 31. 管理员用户列表

```text
GET /api/admin/users?keyword=小明&userType=0&accountStatus=1&creditMin=80&registerStart=2026-01-01&page=1&pageSize=20    🔒 管理员
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| keyword | string | ❌ | 邮箱/手机号/昵称关键词 |
| userType | int | ❌ | 0=普通用户，1=管理员 |
| accountStatus | int | ❌ | 0=正常，1=禁言，2=限制发布，3=封禁 |
| creditMin / creditMax | int | ❌ | 信誉分范围 |
| registerStart / registerEnd | datetime | ❌ | 注册时间范围 |
| page | int | ❌ | 页码，默认 1 |
| pageSize | int | ❌ | 每页条数，默认 20，最大 100 |

返回 `AdminUserPageDto`：

| 字段 | 类型 | 说明 |
|------|------|------|
| items | AdminUserListItemDto[] | 用户列表 |
| totalCount | int | 总数 |
| page / pageSize | int | 分页信息 |
| totalPages | int | 总页数 |

`AdminUserListItemDto` 字段：

| 字段 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户ID |
| email | string | 邮箱 |
| phoneNumber | string? | 手机号 |
| userName | string | 昵称 |
| userType | int | 0=普通用户，1=管理员 |
| accountStatus | int | 0=正常，1=禁言，2=限制发布，3=封禁 |
| isBanned | int | 是否封禁 |
| bannedUntil | datetime? | 封禁截止时间 |
| credit | int | 信誉分 |
| registerTime | datetime | 注册时间 |
| productCount | int | 发布商品数 |
| orderCount | int | 相关订单数 |
| warningCount | int | 警告数 |
| violationCount | int | 违规数 |

### 32. 用户统计

```text
GET /api/admin/users/statistics    🔒 管理员
```

返回：

| 字段 | 类型 | 说明 |
|------|------|------|
| totalUsers | int | 用户总数 |
| normalUsers / mutedUsers / publishRestrictedUsers / bannedUsers | int | 各状态用户数 |
| newUsersToday / newUsersThisWeek | int | 今日/近7天新增 |
| usersWithProducts | int | 发布过商品的用户数 |
| totalOrders | int | 订单总数 |
| totalWorkOrders / pendingWorkOrders | int | 工单总数/待处理数 |
| totalWarnings | int | 警告总数 |

### 33. 用户详情

```text
GET /api/admin/users/{userId}    🔒 管理员
```

返回 `AdminUserDetailDto`，包含列表项字段，并附加：

| 字段 | 类型 | 说明 |
|------|------|------|
| gender | string | 性别 |
| profile | string? | 个性签名 |
| avatarFileId | long? | 头像文件ID |

### 34. 用户发布商品

```text
GET /api/admin/users/{userId}/products    🔒 管理员
```

返回 `ProductDto[]`，包含该用户全部状态商品。

### 35. 用户相关订单

```text
GET /api/admin/users/{userId}/orders    🔒 管理员
```

返回 `PurchaseDto[]`，包含该用户买入和卖出的订单。

### 36. 用户相关举报

```text
GET /api/admin/users/{userId}/reports    🔒 管理员
```

返回 `AdminModerationWorkOrderDto[]`，包含该用户发起的和被举报的工单。

### 37. 用户申诉

```text
GET /api/admin/users/{userId}/appeals    🔒 管理员
```

返回该用户发起的申诉工单列表。

### 38. 用户信誉与违规概览

```text
GET /api/admin/users/{userId}/reputation    🔒 管理员
```

返回：

| 字段 | 类型 | 说明 |
|------|------|------|
| summary | ReputationSummaryDto | 信誉概览 |
| totalViolations | int | 违规数 |
| pendingViolations | int | 待处理违规数 |
| warningCount | int | 警告数 |
| recentWarnings | AdminUserWarningDto[] | 最近警告 |

### 39. 修改用户状态

```text
PATCH /api/admin/users/{userId}/status    🔒 管理员
```

**请求体：**

```json
{
  "status": 3,
  "bannedUntil": null,
  "reason": "违规封禁"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| status | int | ✅ | 0=正常，1=禁言，2=限制发布，3=封禁 |
| bannedUntil | datetime? | ❌ | 封禁截止时间，null=永久封禁 |
| reason | string | ❌ | 修改原因 |

返回 `AdminUserDetailDto`。

### 40. 发送用户警告

```text
POST /api/admin/users/{userId}/warning    🔒 管理员
```

**请求体：**

```json
{
  "reason": "请勿发布违规商品"
}
```

返回 `AdminUserWarningDto`。

---

## 十、管理员商品模块

> 基础路径：`/api/admin/products`  
> 所有接口需要管理员 Token

### 41. 管理员商品列表

```text
GET /api/admin/products?keyword=手机&status=3&categoryId=1&sellerId=23&page=1&pageSize=20    🔒 管理员
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| keyword | string | ❌ | 商品名/描述/卖家昵称 |
| status | int | ❌ | 0=在售，1=已售，2=下架，3=待审核，4=驳回 |
| categoryId | long | ❌ | 分类ID |
| sellerId | int | ❌ | 卖家ID |
| page / pageSize | int | ❌ | 分页 |

返回 `AdminProductPageDto`，列表项 `AdminProductListItemDto`：

| 字段 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |
| name / price / info | - | 商品信息 |
| status | int | 商品状态 |
| sellerName | string | 卖家昵称 |
| categoryName | string? | 分类名 |
| viewCount / favoriteCount / commentCount | int | 浏览/收藏/留言数 |
| imageCount | int | 图片数 |
| rejectReason | string? | 驳回原因 |
| reviewedByAdminId | int? | 审核管理员ID |
| reviewedAt | datetime? | 审核时间 |

### 42. 待审核商品列表

```text
GET /api/admin/products/pending-review?page=1&pageSize=20    🔒 管理员
```

返回状态为 `3 待审核` 的商品分页结果。

### 43. 商品审核统计

```text
GET /api/admin/products/statistics    🔒 管理员
```

返回：

| 字段 | 类型 | 说明 |
|------|------|------|
| totalProducts | int | 商品总数 |
| availableCount / soldCount / removedCount | int | 在售/已售/下架数 |
| pendingReviewCount / rejectedCount | int | 待审核/驳回数 |
| newProductsToday | int | 今日新增商品数 |
| totalAuditLogs / todayAuditLogs | int | 审核日志总数/今日数 |

### 44. 管理员商品详情

```text
GET /api/admin/products/{productId}    🔒 管理员
```

返回 `AdminProductDetailDto`，附加：

| 字段 | 类型 | 说明 |
|------|------|------|
| images | AdminProductImageDto[] | 图片列表 |
| auditLogs | AdminProductAuditLogDto[] | 审核日志 |

### 45. 审核通过

```text
PATCH /api/admin/products/{productId}/approve    🔒 管理员
```

仅待审核商品可操作。返回 `AdminProductDetailDto`。

### 46. 审核驳回

```text
PATCH /api/admin/products/{productId}/reject    🔒 管理员
```

**请求体：**

```json
{
  "reason": "图片不清晰，请重新上传"
}
```

驳回原因必填。

### 47. 强制下架

```text
PATCH /api/admin/products/{productId}/remove    🔒 管理员
```

**请求体：**

```json
{
  "reason": "涉嫌违规"
}
```

### 48. 恢复商品

```text
PATCH /api/admin/products/{productId}/restore    🔒 管理员
```

仅已下架商品可恢复为在售。

### 49. 删除商品

```text
DELETE /api/admin/products/{productId}    🔒 管理员
```

严重违规商品可删除。存在关联订单/会话时返回 `400`，建议改为强制下架。

### 50. 商品审核日志

```text
GET /api/admin/products/{productId}/audit-logs    🔒 管理员
```

返回 `AdminProductAuditLogDto[]`：

| 字段 | 类型 | 说明 |
|------|------|------|
| auditId | long | 日志ID |
| adminId | int | 管理员ID |
| action | string | approve/reject/remove/restore/delete |
| reason | string? | 原因 |
| oldStatus / newStatus | int | 操作前后状态 |
| createTime | datetime | 操作时间 |

---

## 十一、管理员举报与申诉模块

> 基础路径：`/api/admin/reports`、`/api/admin/appeals`、`/api/admin/moderation`  
> 所有接口需要管理员 Token

### 51. 举报列表

```text
GET /api/admin/reports?keyword=违规&status=waiting&targetType=product&page=1&pageSize=20    🔒 管理员
```

| 参数 | 类型 | 必填 | 说明 |
|------|------|------|------|
| keyword | string | ❌ | 原因/描述/昵称/商品名 |
| status | string | ❌ | waiting/processing/done |
| targetType | string | ❌ | product/user/comment/message/order |

返回 `AdminModerationPageDto`。

### 52. 举报详情

```text
GET /api/admin/reports/{reportId}    🔒 管理员
```

返回 `AdminModerationDetailDto`，包含处理时间线。

### 53. 举报成立

```text
PATCH /api/admin/reports/{reportId}/accept    🔒 管理员
```

设置状态为 `done`，结果 `accepted`，并写入时间线。

### 54. 举报不成立

```text
PATCH /api/admin/reports/{reportId}/reject    🔒 管理员
```

设置状态为 `done`，结果 `rejected`。

### 55. 举报综合处理

```text
PATCH /api/admin/reports/{reportId}/handle    🔒 管理员
```

**请求体：**

```json
{
  "action": "remove_product",
  "reason": "商品违规，强制下架"
}
```

`action` 可选值：

| 值 | 说明 |
|------|------|
| none | 仅记录处理结果 |
| remove_product | 下架关联商品 |
| restore_product | 恢复关联商品 |
| ban_user | 封禁关联用户 |
| mute_user | 禁言关联用户 |
| restrict_publish | 限制关联用户发布 |
| unban_user | 解除用户限制 |
| warn_user | 向关联用户发送警告 |

### 56. 申诉列表

```text
GET /api/admin/appeals?keyword=申诉&status=waiting&page=1&pageSize=20    🔒 管理员
```

### 57. 申诉详情

```text
GET /api/admin/appeals/{appealId}    🔒 管理员
```

返回申诉详情及原处理工单信息。

### 58. 申诉通过

```text
PATCH /api/admin/appeals/{appealId}/approve    🔒 管理员
```

如果原工单是下架商品，会自动恢复商品；如果是封禁/禁言/限制发布，会自动恢复用户状态。

### 59. 申诉驳回

```text
PATCH /api/admin/appeals/{appealId}/reject    🔒 管理员
```

### 60. 管理员回复申诉

```text
POST /api/admin/appeals/{appealId}/reply    🔒 管理员
```

**请求体：**

```json
{
  "reply": "请补充更多证明材料"
}
```

### 61. 管理员待办任务

```text
GET /api/admin/moderation/tasks    🔒 管理员
```

返回：

| 字段 | 类型 | 说明 |
|------|------|------|
| totalPending | int | 待处理总数 |
| waitingCount / processingCount | int | 待处理/处理中数量 |
| reportCount / appealCount | int | 举报/申诉总数 |
| recentTasks | AdminModerationWorkOrderDto[] | 最近工单 |

---

> 📊 已实现接口：**61 个** | 接口定义清单见 `二手商品平台后端接口定义.md`
