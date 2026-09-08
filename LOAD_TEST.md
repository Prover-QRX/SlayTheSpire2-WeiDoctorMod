# WeiDoctor v1.0 beta 加载测试

当前已复制测试包到两个位置，其中当前 beta 实际应使用第二个：

`C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2/mods_pending/WeiDoctor`

`C:/Program Files (x86)/Steam/steamapps/workshop/content/2868840/3747602295/WeiDoctorLocal`

根据 `godot.log`，当前 beta 只扫描 Steam 返回的已订阅 Workshop item，再在这些 item 目录内递归寻找 manifest。`mods_pending` 和 `workshop/content/2868840/WeiDoctorLocal` 不在订阅列表内，因此不会被调用。

本轮测试目标：

1. 启动 Slay the Spire 2 beta，并启用 RitsuLib 与 WeiDoctor。
2. 确认游戏不会在模组加载阶段报错或闪退。
3. 退出游戏后检查测试目录下是否生成：

`WeiDoctor.load.log`

当前应检查：

`C:/Program Files (x86)/Steam/steamapps/workshop/content/2868840/3747602295/WeiDoctorLocal/WeiDoctor.load.log`

如果该文件存在，说明 `WeiDoctor.dll` 的入口 `Entry.Initialize()` 已被游戏调用。

当前 v1.0 骨架已经注册：

- 博士卡池占位
- 博士遗物池占位
- `部署`关键词
- 博士角色
- 初始遗物 `DispatchCenterRelic`
- 基础攻击牌 `TacticalFront`
- 基础防御牌 `TacticalBack`
- 第一批 I 阶干员测试卡：隐现、角峰、惊蛰、深巡、普罗旺斯、古米
- 调度中心内部部署队列：3 个部署位、满位撤退、回合结束触发部署效果、到期返回抽牌堆

当前尚未完成：

- 专门的三格部署 UI
- 干员卡完整批量注册
- 调度中心升级
- 卡牌奖励等阶过滤
- 三合一晋升
- 随机组合模式

如果启动失败，请把以下内容发回来：

- 游戏弹窗或控制台报错截图
- `workshop/content/2868840/3747602295/WeiDoctorLocal/WeiDoctor.load.log` 内容，如果已经生成
- 游戏日志中包含 `WeiDoctor`、`RitsuLib`、`ModInitializer` 的几行
