# 校园二手交易平台 - API 文档

> 版本：v1.0 | 更新日期：2026-07-07  
> 基础地址：`http://localhost:5141`

---

## 目录

- [一、用户模块](#一用户模块)
  - [1. 注册](#1-注册)
  - [2. 查询用户信息](#2-查询用户信息)
- [二、商品模块](#二商品模块)
  - [3. 查所有商品](#3-查所有商品)
  - [4. 查在售商品](#4-查在售商品)
  - [5. 查单个商品](#5-查单个商品)
  - [6. 发布商品](#6-发布商品)
  - [7. 修改商品](#7-修改商品)
  - [8. 删除商品](#8-删除商品)
- [三、错误码说明](#三错误码说明)

---

## 一、用户模块

### 1. 注册

新用户注册，成功后返回用户信息。

```
POST /api/user/register
```

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 最大长度 | 说明 |
|------|------|------|---------|------|
| email | string | ✅ | 50 | 邮箱，不可重复 |
| password | string | ✅ | — | 明文密码，后端自动加密 |
| userName | string | ✅ | 20 | 用户昵称 |
| phoneNumber | string | ❌ | 11 | 手机号，不可重复 |

**请求示例：**

```json
{
  "email": "test@example.com",
  "password": "123456",
  "userName": "小明",
  "phoneNumber": "13800138000"
}
```

**响应：**

```http
HTTP/1.1 201 Created
```

**响应体（JSON）：**

| 字段 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户ID（自增） |
| email | string | 邮箱 |
| phoneNumber | string? | 手机号 |
| userName | string | 昵称 |
| registerTime | datetime | 注册时间 |

```json
{
  "userId": 1,
  "email": "test@example.com",
  "phoneNumber": "13800138000",
  "userName": "小明",
  "registerTime": "2026-07-06T22:34:17.149Z"
}
```

---

### 2. 查询用户信息

```
GET /api/user/{userId}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| userId | int | 用户ID |

**请求示例：**

```
GET /api/user/1
```

**响应：**

```http
HTTP/1.1 200 OK
```

```json
{
  "userId": 1,
  "email": "test@example.com",
  "phoneNumber": "13800138000",
  "userName": "小明",
  "registerTime": "2026-07-06T22:34:17.149Z"
}
```

> 用户不存在返回 `404 Not Found`

---

## 二、商品模块

### 3. 查所有商品

```
GET /api/product
```

获取平台所有商品（含已售、已下架）。

**请求示例：**

```
GET /api/product
```

**响应：**

```http
HTTP/1.1 200 OK
```

```json
[
  {
    "productId": 1,
    "name": "二手 iPhone 15",
    "price": 2999.00,
    "info": "九成新，无维修",
    "status": "available",
    "releaseDate": "2026-07-06T22:34:17.149Z",
    "userId": 1,
    "categoryId": 1,
    "categoryName": "电子产品",
    "images": [
      {
        "imgId": 1,
        "imgUrl": "/uploads/xxx.jpg",
        "imgIndex": 1
      }
    ]
  }
]
```

| 字段 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |
| name | string | 商品名（≤30字） |
| price | decimal | 价格 |
| info | string? | 商品描述（≤100字） |
| status | string | available=在售 / sold=已售 / removed=已下架 |
| releaseDate | datetime | 发布时间 |
| userId | int | 卖家用户ID |
| categoryId | long | 分类ID |
| categoryName | string? | 分类名称 |
| images | array | 商品图片列表 |

---

### 4. 查在售商品

获取目前正在出售的商品。

```
GET /api/product/available
```

返回格式同 [查所有商品](#3-查所有商品)。

---

### 5. 查单个商品

```
GET /api/product/{productId}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| productId | long | 商品ID |

**请求示例：**

```
GET /api/product/1
```

返回格式同上，单个对象。不存在返回 `404`。

---

### 6. 发布商品

```
POST /api/product
```

> ⚠️ 需要先注册用户，并确保分类 ID 存在。

**请求体（JSON）：**

| 参数 | 类型 | 必填 | 最大长度 | 说明 |
|------|------|------|---------|------|
| name | string | ✅ | 30 | 商品名称 |
| price | decimal | ✅ | — | 价格，如 2999.00 |
| info | string | ❌ | 100 | 商品描述 |
| userId | int | ✅ | — | 发布者用户ID |
| categoryId | long | ✅ | — | 分类ID |

**请求示例：**

```json
{
  "name": "二手 iPhone 15",
  "price": 2999.00,
  "info": "九成新，没有维修过",
  "userId": 1,
  "categoryId": 1
}
```

**响应：**

```http
HTTP/1.1 201 Created
Location: /api/product/1
```

```json
{
  "productId": 1,
  "name": "二手 iPhone 15",
  "price": 2999.00,
  "info": "九成新，没有维修过",
  "status": "available",
  "releaseDate": "2026-07-06T22:34:17.149Z",
  "userId": 1,
  "categoryId": 1,
  "categoryName": null,
  "images": []
}
```

---

### 7. 修改商品

```
PUT /api/product/{productId}
```

**路径参数：** `productId` — 商品ID

**请求体（JSON）— 所有字段均可选，只传要改的：**

| 参数 | 类型 | 说明 |
|------|------|------|
| name | string? | 新的商品名称 |
| price | decimal? | 新的价格 |
| info | string? | 新的描述 |
| status | string? | available / sold / removed |

**请求示例：**

```json
{
  "price": 2599.00,
  "info": "降价了，急出"
}
```

```json
{
  "status": "sold"
}
```

**响应：**

```http
HTTP/1.1 200 OK
```

返回更新后的商品对象。商品不存在返回 `404`。

---

### 8. 删除商品

```
DELETE /api/product/{productId}
```

**请求示例：**

```
DELETE /api/product/1
```

**响应：**

```http
HTTP/1.1 204 No Content
```

无返回体。商品不存在返回 `404`。

---

## 三、错误码说明

| HTTP 状态码 | 含义 | 常见原因 |
|------------|------|---------|
| 200 | 成功 | — |
| 201 | 创建成功 | 注册/发布成功 |
| 204 | 删除成功 | 无返回内容 |
| 400 | 请求错误 | 参数格式不对 |
| 404 | 不存在 | 用户/商品ID不存在 |

---

## 附录：测试流程

1. **注册用户** → `POST /api/user/register`
2. **（手动）插入分类** → 数据库里跑 `INSERT INTO "category" ("category_name") VALUES ('电子产品');`
3. **发布商品** → `POST /api/product`，填 userId 和 categoryId
4. **查看商品** → `GET /api/product/1`
5. **查看列表** → `GET /api/product` 或 `GET /api/product/available`
