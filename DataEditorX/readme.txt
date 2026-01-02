[DataEditorX]3.1.0[DataEditorX]

★运行环境(Environment)
本程序基于.NET Framework 4.8开发


★文件关联(File association)
.cdb DataEditorX
.lua Visual Studio Code


★设置
DataEditorX.exe.config 语言设置，图片设置，CodeEditor设置
data/language_xxx.txt 界面和消息提示文字
data/cardinfo_xxx.txt 种族，类型，系列名

★其他设置
async				后台加载数据为true，直接加载数据为false
sync_with_card		修改卡片同时也修改卡图和脚本。
open_file_in_this	用自带的脚本编辑器打开lua

★语言设置
DataEditorX.exe.config
<add key="language" value="chinese" />
其他语言请自己添加，需要2个文件，xxx为语言
data/language_xxx.txt
data/cardinfo_xxx.txt

★DataEditor：
攻击力为？，可以输入？，?，-2任意一个。
文件夹pics和script和cdb所在文件夹一致。

★从ydk和图片文件夹读取卡片列表
支持：密码的png/jpg图片

★卡片复制：
替换复制：如果已存在，就取代
不替换复制：如果已存在，就跳过

★卡片搜索
1.仅支持一个系列名搜索
2.ATK,DEF搜索：
	如果是0，则输入-1或.
	如果是?，则输入-2或?
3.卡片名称搜索：
	AOJ%%		以“AOJ”开头
	流%%天		以“流”开头，“天”结尾
	%%战士		以“战士”结尾

5.id范围搜索示例：
id or alias = 10000000
id: 10000000, alias: 0

alias = 10000000
id: 0, alias: 10000000

id between 10000000 and 20000000
id: 10000000, alias: 20000000




★图片设置
在裁剪和导入图片时候使用。
	image_quality	保存的图片质量 1-100
	image			游戏图片大小，小图宽/高，大图宽/高，共4个值
	image_other		一般卡图裁剪
	image_xyz		xyz卡图裁剪
	image_pendulum	Pendulum卡图裁剪

★MSE存档
读取
存档结构：(要求：每张卡的内容，开头是card，最后一行是gamecode，在MSE的card_fields修改gamecode为最后的元素)
card:
....
	gamecode: 123456

★MSE图片
支持：密码，带0密码，卡名的png，jpg图片
在“设置为MSE图片库”（“Set MSE'Image ”）打勾，导入卡图都是放到MSE的图片文件夹

★Magic Set Editor 2
https://github.com/247321453/MagicSetEditor2

★MSE存档生成设置
在每个语言的mse_xxx.txt修改\r\n会替换为换行，\t会替换为tab
简体转繁体
cn2tw = false
每个存档最大数，0则是无限
maxcount = 0

从下面的文件夹找图片添加到存档，名字为密码/卡名.png/jpg
imagepath = ./Images
魔法陷阱标志，%%替换为符号，如果只是%% ，需要设置下面的ST mark is text: yes
spell = [魔法卡%%]
trap = [陷阱卡%%]
游戏yugioh，风格standard，语言CN，Edition：MSE，P怪的中间图不包含P文本区域
head = mse version: 0.3.8\r\ngame: yugioh\r\nstylesheet: standard\r\nset info:\r\n\tlanguage: CN\r\n\tedition: MSE\r\n\tST mark is text: no\r\n\tpendulum image is small: yes
读取存档，卡片描述
text =【摇摆效果】\n%ptext%\n【怪兽效果】\n%text%\n
获取P文本
pendulum-text = 】[\s\S]*?\n([\S\s]*?)\n【
获取怪兽文本
monster-text = [果|介|述|報]】\n([\S\s]*)
替换特殊字
replace = ([鮟|鱇|・|·]) <i>$1</i>
把空格替换为^，（占1/3字宽）
#replace = \s <sym-auto>^</sym-auto>
把A-Z替换为另外一种字体
#replace = ([A-Z]) <i>$1</i>
