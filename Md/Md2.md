# 通用消息枚举

- Result
- CharacterClass 

# 服务层 (Service)

## 1. 用户服务 (UserService)

### 1.1 登录

- 发送登录 : Login(string username, string password)

- 登录回调 : LoginCall(Result,string)

### 1.2 注册

- 发送注册 : Register(string username, string password)
- 注册回调 : RegisterCall(Result,string)

### 1.3 创建角色

- 发送创建角色 : CreateCharacter(string characterName, CharacterClass cls)
- 创建角色回调 : CreateCharacterCall(Result,string)

### 1.4 进入游戏

- 发送进入游戏 : GameEnter(int characterIndex)
- 进入游戏回调 : GameEnterCall(Result,string)

### 1.5 离开游戏

- 发送离开游戏 : GameLeave(bool isQuitGame = false)
- 离开游戏回调 : GameLeaveCall(Result,string)

# 数据存储层 (Model)

## 1. 角色(User)

- 用户信息 : UserInfo
  - 赋值时机 : 登录成功
- 角色信息列表 : CharacterListInfo
  - 赋值时机 : 创建角色成功 删除角色成功 
- 当前选取角色 : CharacterInfo
  - 赋值时机 : 进入游戏成功 离开游戏成功

# 数据管理层 (Manager)