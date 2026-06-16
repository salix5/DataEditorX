
★Environment
This program is based on .NET Framework 4.8

★File association
DataEditorX can open files with specified extension:
.cdb (DataEditorX)
.lua (Visual Studio Code)

★Feedback
Please send bug report to https://github.com/salix5/DataEditorX/issues with the format below.

Version:
DataEditorX X.X.X
Error message:
The error message text
(If there is a error message box, You can press Ctrl+C to copy the message to clipboard)
Detail:
Card message; antivirus; program location; your operation at that time.

★Settings
DataEditorX.exe.config ★Language，★Image，★CodeEditor
data/language_xxx.txt Interface and prompt message 
data/cardinfo_xxx.txt types/series

★Language setting
DataEditorX.exe.config
<add key="language" value="english" />
If you want to add a language，you will need two file(xxx is a type of language):
data/language_xxx.txt
data/cardinfo_xxx.txt

★Image settings
image_quality	1-100
image			Width/height of image, four number
image_other		pendulum of other cards
image_xyz		pendulum of Xyz monsters
image_pendulum	Pendulum

★CodeEditor Settings
use_IME			Editors Input Method
wordwrap	 
tabisspace	tab→space
fontname     
fontsize	 

★DataEditor：
If you need to input Attack "?", you can use any of ？/?/-2 instead. 
The folders of pics, script and cdb should be in the same folder.

★Read cardlist from ydk and folder pics
Support：png, jpg files with card number

★Database comparison

★Copy a card：
Copy and Replace: If there's a card with same name, replace it.
Copy without Replace: If there's a card with same name, ignore it.

★Card search
1. Now it can not support search by Pendulum Scale 
2. You can search card with card name/effect/Attribute/Types/Level（racnk）/effect type/card number
3. Search by ATK,DEF：
	If there is a "0", input"-1"or"."
	If there is a "?", input"-2"or"?"
4. Search by card name：
	AOJ%%		start with AOJ
	Shooting%%Dragon		start with “Shooting” and end with “Dragon”
	%%Warrior		end with “Warrior”

5.Search by card id
id or alias = 10000000
id: 10000000, alias: 0

alias = 10000000
id: 0, alias: 10000000

id between 10000000 and 20000000
id: 10000000, alias: 20000000
