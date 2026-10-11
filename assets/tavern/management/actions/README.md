# 酒馆经营动作

使用内置 image_gen.imagegen 生成；传入既有角色原图和 reference-2.png 作为参考。完整提示词见 generation.json。素材保留透明 Alpha，运行时按等分单元读取，不修改生成原图。

每张四列，动作按行排列：

- aelia-actions.png（六行）：点单／结账、托盘、擦桌、收碟、备饮、整理杯碟。选用同朝向的前两帧，0→1→1→0 循环，避免生成图后两列转身造成动作跳变。
- guest-actions.png（四行）：坐姿等待、吃饭、饮用、付款。
- lyse-actions.png（六行）：取料、切配、烹饪、装盘、清洗、补柴。
- luchuan-actions.png（两行）：站立待命、看账本与指导。
- rest-actions.png（两行）：艾莉娅、莉瑟放松休息。
- delivery-walk.png（四行）：背面单盘、背面双盘、正面单盘、正面双盘；右方向水平镜像。

动作按营业状态实际调用；送餐寻路时使用拿托盘走路，抵达服务位置后显示对应动作。收桌先擦拭再收碟。客户结账后离开，员工继续清理。双盘配送分别访问两张桌子的服务位置。

source/TavernHallActions.cs 负责状态映射及精灵表绘制，source/TavernHallNavigation.cs 负责四方向地面寻路，桌椅含避让边距。客人座位不作为行走终点，行走止于桌边通道再切换坐姿。
