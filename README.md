# Unity 学习项目

## 项目初始化

### Git

好像现在默认设置里 Editor 的 Asset Serialization - Mode 就是 **Force Text** 了？

设置的 Version Control 的 Mode 是 **Visible Meta Files**

项目的 `.gitignore` 可以参考：https://github.com/github/gitignore/blob/main/Unity.gitignore

### Rider

在“编辑” - “首选项” - “外部工具” 的“外部脚本编辑器”处进行修改

### 场景

“编辑” - “首选项” - “ ‘场景’ 视图” 的 General - **在原点创建对象**，将其勾选

### 输入系统

“编辑” - “项目设置” - “玩家” - 其他设置 - 配置 - Api 兼容级别，改为 **.NET Framework**

“活动输入处理” 改为 “**输入系统包（新）**”

## 笔记速记

### 编辑器

`F2` 重命名

调整碰撞体，按住 `Alt` 沿中心对称调整

### 3D 场景

#### 视角控制

- `Alt + 鼠标` 绕目标点轴心旋转视角
- `鼠标右键` 以当前相机位置为轴心旋转视角
- `鼠标中键` 以当前相机视角的上下左右平移
- `前后左右光标键` 移动
- `滚轮` 缩放
- `F` 聚焦到当前选定游戏对象

#### 左上角模式快捷键

- `Q` 查看工具（好像相当于鼠标中键？）
- `W` 移动工具
- `E` 旋转工具
- `R` 缩放工具
- `T` 矩形工具
- `Y` 变换组件工具

### 2D 场景

#### 平铺调色板

矩形工具时 `Shift + 鼠标左键` 对应擦除

## CatlikeCoding

- **[Basics - Measuring Performance](https://catlikecoding.com/unity/tutorials/basics/measuring-performance/)**：找不到 URP 的 SRP Batcher 在哪里关闭……

  - 重点参考如下文档：https://docs.unity.cn/Packages/com.unity.render-pipelines.universal@16.0/manual/universalrp-asset.html

    - > 任何使用通用渲染管线（URP）的 Unity 项目都必须具有 URP 资产来配置设置。当您使用 URP 模板创建项目时，Unity 会在 **Settings** 项目文件夹中创建 URP 资产，并在 Project Settings 中分配它们。如果你要将现有项目迁移到 URP，则需要[创建一个 URP 资产，并在图形设置中分配该资产 ](https://docs.unity.cn/Packages/com.unity.render-pipelines.universal@16.0/manual/InstallURPIntoAProject.html)。

    - > 在 URP 资产的任意分段中，点击垂直省略号图标（⋮），然后选择 **显示其他属性（Show Additional Properties）**

  - 默认在 `Assets` 中会有 `UniversalRenderPipelineGlobalSettings.asset`

    - 对应也就是 Project Settings 下 Graphics - URP Global Settings 里有跳转链接（也在这里可以更改设置）

  - 而对应使用的资产，则可以在 Project Settings 下的 Graphics 中的 Scriptable Render Pipeline Settings 看到，点击可跳转

    - 项目中默认是 `Assets/Settings/URP-HighFidelity.asset`

  - 而修改 SRP Batcher，则需要在其中的 Rendering 右侧点击三个点的按钮，选择 Show Additional Properties。这样其中才会有 SRP Batcher 单选框

