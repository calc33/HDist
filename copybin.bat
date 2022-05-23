@ECHO OFF

SET DEST=C:\bin

PUSHD BuildSum\bin\Release\net5.0
XCOPY /Y /I *.exe %DEST%
XCOPY /Y /I *.dll %DEST%
XCOPY /Y /I *.deps.json %DEST%
XCOPY /Y /I *.runtimeconfig.json %DEST%
POPD

PUSHD HCopy\bin\Release\net5.0
XCOPY /Y /I *.exe %DEST%
XCOPY /Y /I *.dll %DEST%
XCOPY /Y /I *.deps.json %DEST%
XCOPY /Y /I *.runtimeconfig.json %DEST%
POPD

PUSHD HCopyW\bin\Release\net5.0-windows
XCOPY /Y /I *.exe %DEST%
XCOPY /Y /I *.dll %DEST%
XCOPY /Y /I *.deps.json %DEST%
XCOPY /Y /I *.runtimeconfig.json %DEST%
POPD

PAUSE
