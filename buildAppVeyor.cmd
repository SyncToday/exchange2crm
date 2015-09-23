@echo off
cls

ECHO F| xcopy "tests\exchange2crm.Tests\appSettings.sample.config" "tests\exchange2crm.Tests\appSettings.config" /i /d /y
ECHO F|xcopy "src\exchange2crm\Secret.Sample.fs" "src\exchange2crm\Secret.fs" /i /d /y

build.cmd
