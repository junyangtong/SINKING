## 1. 砖块类型体系

- [x] 1.1 创建 `SolidBlock.cs` — 不可破碎砖块脚本（纯碰撞体，无消失逻辑，支持对象池 OnEnable 重置）
- [x] 1.2 修改 `Ground.cs` — 新增对 hitBox 攻击的 OnTriggerEnter2D 响应，被攻击时也触发溶解/破碎
- [x] 1.3 标记 `BreakableBlock.cs` 为弃用（攻击碎裂逻辑已整合到 Ground.cs）

## 2. 段落式生成算法核心

- [x] 2.1 在 `PlatformGenerator.cs` 中新增段落生成配置参数（breakableWeight, solidWeight, gapWeight, segmentMinLength, segmentMaxLength, gapMinLength, gapMaxLength, maxGapLength）
- [x] 2.2 实现 `GenerateRowPattern(int blockCount)` 方法 — 返回 int[] 段落序列，按权重逐段填充
- [x] 2.3 实现段落约束后处理 — 行首行尾不可为空隙、连续空隙不超过 maxGapLength、空隙段后不紧跟空隙段
- [x] 2.4 实现行间空隙防对齐 — 记录上一行空隙位置，新行空隙完全重叠时偏移或替换

## 3. 平台生成重构

- [x] 3.1 调整 blockSize 默认值为 0.5，blockSpacing 默认值为 0.02
- [x] 3.2 重写 `CreateNewPlatform()` — 调用 GenerateRowPattern() 获取序列，按序列实例化砖块（1=breakablePrefab, 2=solidPrefab, 3=跳过）
- [x] 3.3 重构对象池回收逻辑 — 回收时销毁所有子砖块，复用父容器 GameObject
- [x] 3.4 修复 `SpawnPlatform()` — 从池取出后按新序列重新生成子砖块，而非复用旧子砖块

## 4. 参数调优与难度接口

- [x] 4.1 暴露公共方法 `SetDifficultyParams()` 供外部调整权重参数
- [x] 4.2 在 Inspector 中整理参数分组，添加 Header 和 Tooltip 注释
