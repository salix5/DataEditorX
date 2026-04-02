# DataEditorX
Card database (.cdb file) editor for [ygopro](https://github.com/salix5/ygopro).

## Download
https://github.com/salix5/DataEditorX/releases/latest

Require:  
.NET Framework 4.8

## Features
* Create and edit card databases.   
* Compare, copy and paste cards across databases.   
* Read card records from ygopro decks (.ydk file) or card picture directories (like pics/ of ygooro).  
* Export and import [MSE](https://github.com/247321453/MagicSetEditor2) sets.   

> **FAQ**   
Q: How to add a new archetype?  
A: First choose a hex number for the new archetype. Avoid using existing setcodes. Then type it in the text box on the right of the combo box of archetype. To show the name of the new archetype in the combo box. Open data/cardinfo_x.txt (x is language), add a new line between "##setname" and "#end", write the setcode (starts with 0x) and the archetype name separated by a Tab.

## Language
Open Help -> Language to choose language, then restart the application.   
If you want to add a language x for DataEditorX, you need 2 files:

data/language_x.txt  
text in UI

data/cardinfo_x.txt  
text in card information    

Each line in language_english.txt, cardinfo_english.txt is separated by a Tab.  
Translate the content on the right and write to language_x.txt/cardinfo_x.txt.
