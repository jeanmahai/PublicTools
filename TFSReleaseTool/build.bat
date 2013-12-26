@echo off

set currentPath=%cd%
echo current path : %cd%

set nodebobAppFolder=D:\jean.h.ma\Downloads\github\nodebob\app
echo nodebob path : %nodebobAppFolder%

set buildPath=D:\jean.h.ma\Downloads\github\nodebob
echo build path : %buildPath%

set buildedRelease=D:\jean.h.ma\Downloads\github\nodebob\release
echo builded release path : %buildedRelease%

set targetRelease=%cd%\Release
echo target release path : %targetRelease%
echo ================================
echo delete all files from nodebob app folder...
del %nodebobAppFolder%\*.* /y

echo copy files  to nodebob app folder...
xcopy %cd%\*.js %nodebobAppFolder% /s/r/y
xcopy %cd%\*.html %nodebobAppFolder% /s/r/y
xcopy %cd%\*.htm %nodebobAppFolder% /s/r/y
xcopy %cd%\*.css %nodebobAppFolder% /s/r/y
xcopy %cd%\*.png %nodebobAppFolder% /s/r/y
xcopy %cd%\*.gif %nodebobAppFolder% /s/r/y
xcopy %cd%\*.jpeg %nodebobAppFolder% /s/r/y
xcopy %cd%\*.json %nodebobAppFolder% /s/r/y

echo ================================
echo call build.bat ....
cd %buildPath%
call build.bat
cd %currentPath% 
echo ================================
xcopy %buildedRelease%\* %targetRelease% /s/r/y

%targetRelease%\nw.exe
