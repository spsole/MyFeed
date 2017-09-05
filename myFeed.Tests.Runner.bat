@echo off
cd myFeed.Tests
chcp 65001
call dotnet restore
call dotnet test
pause