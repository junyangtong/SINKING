## ADDED Requirements

### Requirement: 不可破碎砖块基本行为
SolidBlock SHALL 作为永不消失的安全平台，玩家踩踏和攻击均不会导致其消失或状态改变。

#### Scenario: 玩家踩踏
- **WHEN** 玩家角色站在不可破碎砖块上
- **THEN** 砖块 SHALL 保持不变，提供稳定的碰撞支撑

#### Scenario: 玩家攻击
- **WHEN** 玩家的 hitBox 触碰不可破碎砖块
- **THEN** 砖块 SHALL 不受影响，不触发任何破碎或溶解效果

### Requirement: 碰撞体配置
SolidBlock SHALL 包含 BoxCollider2D 组件，尺寸与砖块 blockSize 匹配。

#### Scenario: 碰撞检测
- **WHEN** 不可破碎砖块被实例化
- **THEN** 其 BoxCollider2D SHALL 正确启用，玩家可以站立在上面

### Requirement: 对象池兼容
SolidBlock SHALL 支持对象池复用，OnEnable 时重置状态。

#### Scenario: 复用时重置
- **WHEN** 不可破碎砖块从对象池中重新激活
- **THEN** 其 Collider 和视觉状态 SHALL 恢复为初始状态

### Requirement: 视觉区分
SolidBlock SHALL 使用与可破碎砖块不同的视觉外观（Sprite 或颜色），使玩家能够在游戏中区分两种砖块类型。

#### Scenario: 外观区分
- **WHEN** 一行平台中同时存在可破碎和不可破碎砖块
- **THEN** 两种砖块 SHALL 在视觉上明显可区分（颜色/纹理不同）