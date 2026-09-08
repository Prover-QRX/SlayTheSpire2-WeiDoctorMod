# WeiDoctor v1.0 beta

目标：杀戮尖塔2 beta `v0.111.0` 的博士角色原型包。

本版本范围：
- 博士角色
- 初始卡组：4张“战术指令：前线”、4张“战术指令：后方”
- 开局一阶干员候选：展示10张，选择2张
- 调度中心等级与奖励等阶限制
- 3个部署位
- 部署位满时撤退最早部署干员
- 回合结束触发部署区效果
- 三合一晋升
- 非随机模式数据与随机模式配置占位

当前状态：
- 已生成完整数据、素材索引、设计表和源码草案。
- 已按本机 Slay the Spire 2 `v0.111.0` 与 RitsuLib `0.5.19` 校准 manifest。
- 已记录可用的 RitsuLib 接入口，见 `docs/api_notes.md`。
- 当前源码已可编译，并已接入基于 RitsuLib 的最小真实注册入口。
- 该 DLL 仍是机制骨架：已注册博士卡池、遗物池、部署关键词、1张测试攻击牌和1个调度中心初始遗物，尚未实现完整角色、奖励池、部署区、三合一晋升与随机模式。
- 当前没有生成 `WeiDoctor.pck`，所以 `WeiDoctor.json` 暂时设置 `has_pck: false`。

进入游戏加载测试前需要：
- 使用 `dotnet build source/WeiDoctor.csproj` 重新编译。
- 将根目录的 `WeiDoctor.json`、`WeiDoctor.dll`、`WeiDoctor.deps.json` 与必要数据/素材目录复制到游戏可识别的本地模组目录。
- 启动游戏后检查模组目录下是否生成 `WeiDoctor.load.log`。
- 详细测试步骤见 `LOAD_TEST.md`。

默认本机引用路径已经写入 `source/WeiDoctor.csproj`；换机器时可通过 MSBuild 属性覆盖：

```powershell
dotnet build source/WeiDoctor.csproj `
  -p:Sts2InstallDir="C:/Program Files (x86)/Steam/steamapps/common/Slay the Spire 2" `
  -p:RitsuLibDir="C:/Program Files (x86)/Steam/steamapps/workshop/content/2868840/3747602295/lib/0.111.0"
```
