# Unity场景配置指南

## 场景说明
项目有三个场景：
- **MenuScene** - 主菜单
- **SingleGameScene** - 单人关卡
- **GameScene** - 双人关卡

---

## 自动处理逻辑
GameInitializer脚本会**根据场景名称自动判断模式**：
- `SingleGameScene` → 自动设置为单人模式
- `GameScene` → 自动设置为双人模式

---

## 需要在Unity中手动配置的内容

### 步骤1: 在每个游戏场景中添加GameManager

#### SingleGameScene（单人关卡）
1. 打开 `SingleGameScene.unity`
2. Hierarchy → 右键 → Create Empty → 重命名为 `GameManager`
3. Add Component → 搜索 `GameInitializer`
4. 配置引用：
   | 字段 | 拖拽 |
   |------|------|
   | Player1 | 场景中的玩家物体 |
   | Main Camera | MainCamera |

#### GameScene（双人关卡）
1. 打开 `GameScene.unity`
2. Hierarchy → 右键 → Create Empty → 重命名为 `GameManager`
3. Add Component → 搜索 `GameInitializer`
4. 配置引用：
   | 字段 | 拖拽 |
   |------|------|
   | Player1 | Player1物体 |
   | Player2 | Player2物体 |
   | Main Camera | 主相机 |
   | Player2 Camera | 第二个相机（需要创建） |

---

### 步骤2: 双人关卡需要创建第二个相机

在 `GameScene` 中：
1. Hierarchy → 右键 → Camera → 重命名为 `Player2Camera`
2. 设置Camera组件：
   - Clear Flags: Solid Color
   - Projection: Orthographic
   - Size: 与主相机相同
3. Add Component → `Follow` 脚本
   - 勾选 `Is Portrait Mode`
   - 设置 `Fixed Y Position`

---

### 步骤3: 菜单场景（MenuScene）

菜单场景**不需要**添加GameInitializer，只需要确保按钮正确跳转：
- 单人按钮 → 加载 `SingleGameScene`
- 双人按钮 → 加载 `GameScene`

---

## 触屏操作说明

| 操作 | 动作 |
|------|------|
| 上滑 | 跳跃 |
| 下滑 | 攻击 |
| 左滑 | 向左移动 |
| 右滑 | 向右移动 |

**双人模式分屏：**
- 下半屏触控 → Player1
- 上半屏触控 → Player2

---

## 自动创建的管理器

以下内容**不需要手动添加**：
- SwipeInputManager（触屏输入管理器）
- SplitScreenManager（分屏管理器）

---

## 关于Parallax脚本

Parallax脚本用于视差滚动效果，目前场景中没有使用。如果需要：
1. 创建背景物体
2. Add Component → `Parallax`
3. 设置 `Cam` = MainCamera
4. 勾选 `Is Portrait Mode`
