@ECHO OFF

MKDIR Release

XCOPY /S /Y /I /EXCLUDE:ignore.txt BuildSum\bin\Release\net9.0 Release
XCOPY /S /Y /I /EXCLUDE:ignore.txt HCopy\bin\Release\net9.0 Release
XCOPY /S /Y /I /EXCLUDE:ignore.txt HCopyW\bin\Release\net9.0-windows10.0.22000.0 Release
