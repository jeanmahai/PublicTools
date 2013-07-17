@echo off

cd..

set current_path=%cd%

@echo 打包APP...

set buildPath=D:\jean.h.ma\Downloads\github\node-webkit-v0.6.1-win-ia32\build

@echo 编译器路径:%buildPath%

@echo 删除app\中的所有文件及文件夹....

rd %buildPath%\app\ /s /q

md %buildPath%\app

@echo 删除完成

@echo 复制打包文件到app...

xcopy package.json %buildPath%\app\ 
xcopy *.html %buildPath%\app\ /s /h
xcopy *.htm %buildPath%\app\ /s /h
xcopy *.js %buildPath%\app\ /s /h
xcopy *.css %buildPath%\app\ /s /h

@echo 复制完成

@echo 删除release/中的所有文件及文件夹

rd %buildPath%\release\ /s /q

md %buildPath%\release

@echo 删除完成

@echo 开始打包...

cd %buildPath%

call %buildPath%\build.bat

rd %current_path%\release /s /q

md %current_path%\release

xcopy %buildPath%\release %current_path%\release /s /h

@echo 打包完成
