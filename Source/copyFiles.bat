@echo off
SET "ProjectName=ExtremeColds"
SET "SolutionDir=C:\Users\robin\Desktop\Games\RimWorld Modding\Source\ExtremeColds\Source"
@echo on

xcopy /S /Y "%SolutionDir%\..\About\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\About\"