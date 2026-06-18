[DataEditorX]3.9.0[DataEditorX]

★运行环境(Environment)
.NET Framework 4.8


★文件关联(File association)
.cdb DataEditorX
.lua Visual Studio Code


★设置
DataEditorX.exe.config	语言设置，图片设置
data/language_xxx.txt	界面和消息提示文字
data/cardinfo_xxx.txt	种族，类型，系列名

★其他设置
async				true：后台加载数据，false：直接加载数据
sync_with_card		修改卡片同时也修改卡图和脚本。

★语言设置
DataEditorX.exe.config
<add key="language" value="chinese" />
其他语言请自己添加，需要2个文件，xxx为语言
data/language_xxx.txt
data/cardinfo_xxx.txt

★DataEditor：
★从ydk和图片文件夹读取卡片列表
支持：密码的png/jpg图片

★卡片复制：
替换复制：如果已存在，就取代
不替换复制：如果已存在，就跳过

★卡片搜索
1.仅支持一个系列名搜索

2.ATK,DEF搜索：
?: 输入-2或?

3.卡片名称搜索：
AOJ%%		以“AOJ”开头
流%%天		以“流”开头，“天”结尾
%%战士		以“战士”结尾

4.id范围搜索：
id or alias = 10000000
id: 10000000, alias: 0

alias = 10000000
id: 0, alias: 10000000

id between 10000000 and 20000000
id: 10000000, alias: 20000000


★图片设置
在裁剪和导入图片时候使用。
	image_quality	保存的图片质量 1-100
	image			游戏图片大小，大图宽/高，共2个值
	image_other		一般卡图裁剪
	image_xyz		xyz卡图裁剪
	image_pendulum	Pendulum卡图裁剪
