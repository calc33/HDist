@ECHO OFF

MKDIR Release

XCOPY /S /Y /I /EXCLUDE:ignore.txt HDistCore\bin\Release\net5.0 Release
XCOPY /S /Y /I /EXCLUDE:ignore.txt BuildSum\bin\Release\net5.0 Release
XCOPY /S /Y /I /EXCLUDE:ignore.txt HCopy\bin\Release\net5.0 Release
XCOPY /S /Y /I /EXCLUDE:ignore.txt HCopyW\bin\Release\net5.0-windows Release
