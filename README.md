# 接口说明

# 目录

- [概述](#概述)
 - [JsonResult类型说明](#jsonresult类型说明)
- [鉴权接口](#鉴权接口)
  - [获取Code](#获取Code)
  - [获取Token](#获取Token)
  - [刷新Token](#刷新Token)
  - [注销Token](#注销token)
  - [验证身份和权限](#验证身份和权限)
- [短信接口](#短信接口)
  - [获取短信验证码](#获取短信验证码)
  - [验证短信验证码](#验证短信验证码)
- [用户接口](#用户接口)
  - [注册用户](#注册用户)
  - [获取用户信息](#获取用户信息)
  - [修改密码](#修改密码)
  - [重置密码](#重置密码)
  - [设置支付密码](#设置支付密码)

# 概述

这是一个支持RBAC权限模型的REST风格的WCF服务框架。

主要功能：

1.独立的身份验证服务，支持单点登录
2.可扩展的支持鉴权的身份验证服务
3.内置日志服务器，可通过配置日志规则实现写日志到文件或数据库
4.支持组织机构管理
5.支持用户/用户组管理
6.支持角色管理，可对角色进行操作权限（允许/拒绝）和数据权限（只读/读写）的授权，角色成员支持：用户/用户组/职位
7.支持可配置的连续流水号的业务编码（如订单号）生成
8.支持6位数字短信验证码的生成
9.可通过简单的服务注册实现多个服务的管理和扩展

接口访问形式：GET、POST、PUT、DELETE。

令牌由Request的Authorization头部承载，令牌的编码方式为Base64。

[回目录](#目录)

## JsonResult类型说明

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|successful|bool|接口调用是否成功|
|code|string|错误代码|
|message|string|错误消息，描述了接口调用失败原因|
|option|object|附加数据|
|data|\<T>|接口返回数据(泛型)|

[回目录](#目录)

# 数据接口

## 鉴权接口

### 获取Code

方法：GET 

URL：authapi/v1.0/tokens/codes

- 令牌：无

- 参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|account|string|登录账号|
|type|int|0、密码登录(默认);1、验证码登录|

返回数据：一次性密钥

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|无|string|用以在获取Token时加密数据，5秒内有效|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": "a4edbed144df4584810ef45428db7baf"
}
```

[回目录](#目录)

### 获取Token

方法：GET 

URL：authapi/v1.0/tokens

令牌：无

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|account|string|登录账号|
|tenantid|string|租户ID，令牌无需鉴权时可为空|
|appid|string|应用ID|
|signature|string|用户签名：md5(md5(account + md5(password)) + code)|
|deptid|string|登录部门ID(可为空)|

返回数据：令牌数据包

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|accessToken|string|在接口调用时附加在请求头|
|refreshToken|string|刷新令牌时使用|
|expiryTime|int|accessToken过期秒数，过期后可调用刷新接口刷新令牌|
|failureTime|int|accessToken失效秒数，失效后必须重新获取令牌|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "accessToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiN2YwNmYwZmIyZjAxNDU5YjliMWE3MDBhOTJiYTFhOTgifQ==",
    "refreshToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiNzI3YmYyM2JhZmJhNGQ0MDk3OWU1YjZlMjRiYmQzZDYifQ==",
    "expiryTime": 7200,
    "failureTime": 86400
  }
}
```

[回目录](#目录)

### 刷新Token

方法：PUT

URL：authapi/v1.0/tokens

令牌：refreshToken

参数：无

返回数据：令牌数据包

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|accessToken|string|在接口调用时附加在请求头|
|refreshToken|string|刷新令牌时使用|
|expiryTime|int|accessToken过期秒数，过期后可调用刷新接口刷新令牌|
|failureTime|int|accessToken失效秒数，失效后必须重新获取令牌|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "accessToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiN2YwNmYwZmIyZjAxNDU5YjliMWE3MDBhOTJiYTFhOTgifQ==",
    "refreshToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiNzI3YmYyM2JhZmJhNGQ0MDk3OWU1YjZlMjRiYmQzZDYifQ==",
    "expiryTime": 7200,
    "failureTime": 86400
  }
}
```

[回目录](#目录)

### 注销Token

方法：DELETE

URL：authapi/v1.0/tokens

令牌：accessToken

参数：无

返回数据：无

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": null
}
```

[回目录](#目录)

### 验证身份和权限

方法：GET 

URL：authapi/v1.0/tokens/secret

令牌：accessToken

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|action|string|权限代码|

返回数据：用户信息

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|id|string|用户ID|
|tenantId|string|租户ID|
|deptId|string|用户当前登录部门ID|
|name|string|用户名称|
|account|string|注册账号|
|mobile|string|绑定手机号|
|email|string|绑定邮箱|
|remark|string|用户备注|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "id": "71f94cb6-9247-40f8-aaef-e771b9a0961d",
    "tenantId": "30a1307b-b85c-4cc5-b12f-be6a708311f4",
    "deptId": null,
    "name": "管理员",
    "account": "2810",
    "mobile": null,
    "email": null,
    "remark": null
  }
}
```

[回目录](#目录)

## 短信接口

### 获取短信验证码

方法：GET 

URL：authapi/v1.0/smscodes

令牌：无

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|mobile|string|手机号|
|type|int|类型：0、验证手机号;1、注册用户账号;2、重置密码;3、修改支付密码;4、登录验证码|
|life|int|过期时间(分钟)，默认15分钟|
|length|int|验证码位数，默认6位|

返回数据：短信验证码

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|无|string|验证码|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": "123456"
}
```

备注：每客户端每10秒可访问1次

[回目录](#目录)

### 验证短信验证码

方法：DELETE

URL：authapi/v1.0/smscodes

令牌：无

参数：BODY

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|mobile|string|手机号|
|code|string|验证码|
|type|int|类型：0、验证手机号;1、注册用户账号;2、重置密码;3、修改支付密码;4、登录验证码|
|remove|bool|是否验证成功后删除记录|

请求数据示例：
```
{
  "code": 
  {
    "code": "123456",
    "mobile": "13958085903",
    "type": 0,
    "remove": true
  }
}
```

返回数据：无

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": null
}
```

备注：参数相同时，每客户端每10分钟可访问1次

[回目录](#目录)

## 用户接口

### 注册用户

方法：POST

URL：userapi/v1.0/users/signup

令牌：accessToken

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|appid|string|应用ID|
|code|string|短信验证码|

参数：BODY

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|id|string|用户ID，可为空，或传长度不超过36位的字符串|
|name|string|用户名，不可为空|
|account|string|登录账号，不可为空，唯一|
|mobile|string|绑定手机号，可为空，唯一|
|email|string|绑定邮箱，可为空，唯一|
|password|string|密码(MD5)|
|remark|string|用户备注(可为空)|

请求数据示例：
```
{
  "user": 
  {
    "name": "宣炳刚",
    "account": "xbg",
    "mobile": "13958085903",
    "email": null,
    "password": "e10adc3949ba59abbe56e057f20f883e",
    "remark": null
  }
}
```

返回数据：令牌数据包

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|accessToken|string|在接口调用时附加在请求头|
|refreshToken|string|刷新令牌时使用|
|expiryTime|int|accessToken过期秒数，过期后可调用刷新接口刷新令牌|
|failureTime|int|accessToken失效秒数，失效后必须重新获取令牌|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "accessToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiN2YwNmYwZmIyZjAxNDU5YjliMWE3MDBhOTJiYTFhOTgifQ==",
    "refreshToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiNzI3YmYyM2JhZmJhNGQ0MDk3OWU1YjZlMjRiYmQzZDYifQ==",
    "expiryTime": 7200,
    "failureTime": 86400
  }
}
```

[回目录](#目录)

### 修改密码

方法：PUT

URL：userapi/v1.0/users/{id}/signature

令牌：accessToken

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|id|string|用户ID|

参数：BODY

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|password|string|新密码|

请求数据示例：
```
{
  "password": "e10adc3949ba59abbe56e057f20f883e"
}
```

返回数据：用户信息

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|id|string|用户ID|
|tenantId|string|租户ID|
|deptId|string|用户当前登录部门ID|
|name|string|用户名称|
|account|string|注册账号|
|mobile|string|绑定手机号|
|email|string|绑定邮箱|
|remark|string|用户备注|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "id": "71f94cb6-9247-40f8-aaef-e771b9a0961d",
    "tenantId": "30a1307b-b85c-4cc5-b12f-be6a708311f4",
    "deptId": null,
    "name": "管理员",
    "account": "2810",
    "mobile": null,
    "email": null,
    "remark": null
  }
}
```

[回目录](#目录)

### 重置密码

方法：PUT

URL：userapi/v1.0/users/{account}/resetpw

令牌：无

参数：URL

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|account|string|登录账号|

参数：BODY

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|appid|string|应用ID|
|password|string|新密码|
|code|string|短信验证码|
|mobile|string|手机号|

请求数据示例：
```
{
  "appid": "b4e8dff6-a2eb-46c6-982d-ee395a4261e0"
  "password": "e10adc3949ba59abbe56e057f20f883e"
  "code": "1c320d4fc1614f54800d34b86a53941e"
  "mobile": "13958085903"
}
```

返回数据：令牌数据包

|属性|数据类型|属性说明|
| ------------ | ------------ | ------------ |
|accessToken|string|在接口调用时附加在请求头|
|refreshToken|string|刷新令牌时使用|
|expiryTime|int|accessToken过期秒数，过期后可调用刷新接口刷新令牌|
|failureTime|int|accessToken失效秒数，失效后必须重新获取令牌|

返回数据示例：
```
{
  "successful": true,
  "code": "200",
  "name": "OK",
  "message": "接口调用成功",
  "option": null,
  "data": {
    "accessToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiN2YwNmYwZmIyZjAxNDU5YjliMWE3MDBhOTJiYTFhOTgifQ==",
    "refreshToken": "eyJpZCI6ImE0ZWRiZWQxNDRkZjQ1ODQ4MTBlZjQ1NDI4ZGI3YmFmIiwidXNlcklkIjoiMDAwMDAwMDAtMDAwMC0wMDAwLTAwMDAtMDAwMDAwMDAwMDAwIiwic2VjcmV0IjoiNzI3YmYyM2JhZmJhNGQ0MDk3OWU1YjZlMjRiYmQzZDYifQ==",
    "expiryTime": 7200,
    "failureTime": 86400
  }
}
```

[回目录](#目录)
