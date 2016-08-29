@echo off
 setlocal
 
 cd /d %~dp0

 if /i "%processor_architecture%"=="AMD64" GOTO AMD64
 if /i "%PROCESSOR_ARCHITEW6432%"=="AMD64" GOTO AMD64
 if /i "%processor_architecture%"=="x86" GOTO x86
 GOTO ERR
 
 :AMD64

start /b /wait ETM03x64.exe %*

 GOTO END
 :x86

start /b /wait ETM03x86.exe %*
 :END