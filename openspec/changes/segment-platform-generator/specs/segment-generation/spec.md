## ADDED Requirements

### Requirement: 段落式行序列生成
系统 SHALL 生成由连续段落组成的平台行序列，每段包含相同类型的砖块（可破碎=1、不可破碎=2、空隙=3）。段类型按配置权重随机选取，段长度在配置的范围内随机确定。

#### Scenario: 基本段落生成
- **WHEN** 系统生成一行新平台
- **THEN** 输出为 int[] 数组，每个元素为 1/2/3，相邻相同类型的元素形成连续段落（非逐块独立随机）

#### Scenario: 段类型权重
- **WHEN** 配置权重为 breakableWeight=35, solidWeight=40, gapWeight=25
- **THEN** 长期生成中，可破碎段约占 35%，不可破碎段约占 40%，空隙段约占 25% 的出现频率

#### Scenario: 段长度范围
- **WHEN** 砖块段（类型1或2）生成时
- **THEN** 段长度 SHALL 在 segmentMinLength(默认1) 到 segmentMaxLength(默认5) 之间随机
- **WHEN** 空隙段（类型3）生成时
- **THEN** 段长度 SHALL 在 gapMinLength(默认2) 到 gapMaxLength(默认3) 之间随机

### Requirement: 行首行尾约束
系统 SHALL 确保每行平台的首尾位置为砖块（类型1或2），不可为空隙。

#### Scenario: 行首为空隙时修复
- **WHEN** 段落生成后行首位置为空隙(3)
- **THEN** 系统 SHALL 将行首的空隙段替换为随机砖块类型(1或2)

#### Scenario: 行尾为空隙时修复
- **WHEN** 段落生成后行尾位置为空隙(3)
- **THEN** 系统 SHALL 将行尾的空隙段替换为随机砖块类型(1或2)

### Requirement: 连续空隙限制
系统 SHALL 确保行内连续空隙不超过 maxGapLength（默认3）。

#### Scenario: 超长空隙截断
- **WHEN** 生成的空隙段长度超过 maxGapLength
- **THEN** 系统 SHALL 将超出部分替换为砖块

### Requirement: 连续空隙段禁止
系统 SHALL 确保不会出现两段连续的空隙段（即空隙段后面不能紧跟空隙段）。

#### Scenario: 空隙段后选类型
- **WHEN** 上一段为空隙段(3)，需要选择下一段类型
- **THEN** 系统 SHALL 只从砖块类型(1或2)中选择，跳过空隙类型(3)

### Requirement: 行间空隙防对齐
系统 SHALL 记录上一行的空隙位置，生成新行时检查新空隙是否与上一行空隙完全重叠，防止形成死区。

#### Scenario: 空隙完全重叠时偏移
- **WHEN** 新行的某段空隙的位置范围完全被上一行的空隙位置范围包含
- **THEN** 系统 SHALL 将该段空隙随机偏移或替换为砖块类型

#### Scenario: 空隙部分重叠时允许
- **WHEN** 新行的某段空隙与上一行空隙仅部分重叠
- **THEN** 系统 SHALL 允许该空隙保留不变

### Requirement: 砖块数量适配屏幕
系统 SHALL 根据相机 orthographicSize 和屏幕宽高比动态计算每行砖块数量，确保砖块铺满可用屏幕宽度。

#### Scenario: 竖屏 9:16 计算
- **WHEN** orthographicSize=5，宽高比=9:16，blockSize=0.5，spacing=0.02，sideMargin=0.3
- **THEN** 每行砖块数量 SHALL 约为 9~10 块

### Requirement: 难度参数接口
系统 SHALL 暴露可在运行时调整的难度参数，用于随时间推移增加难度。

#### Scenario: 运行时调整权重
- **WHEN** 外部系统调用增加空隙权重的方法
- **THEN** 后续生成的行 SHALL 使用更新后的权重值