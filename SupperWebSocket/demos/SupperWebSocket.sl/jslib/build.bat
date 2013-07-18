@echo off

cd ../..
set source_path=%cd%
cd ../Client
set target_path=%cd%

@echo %source_path%
@echo %target_path%

cd ../jslib
xcopy *.js %target_path% /s /h /y

