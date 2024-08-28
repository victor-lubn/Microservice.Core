@ECHO OFF
CALL "%VS160COMNTOOLS%VsDevCmd.bat"
SET XsdFolder=..\src\NuGet.Dgml\Dgml
xsd "%XsdFolder%\dgml.xsd" /nologo /c /l:CS /n:NuGet.Dgml /o:"%XsdFolder%"
