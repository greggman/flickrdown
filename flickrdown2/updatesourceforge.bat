@echo off
if not "%PUTTY_PATH%"=="" goto checkname
echo you must set the PUTTY_PATH environment variable to the path to putty
goto error

:checkname
if not "%SOURCEFORGE_NAME"=="" goto update
echo you must set the SOURCEFORGE_NAME environment variable to the username of your sourceforge account
goto error

:update
"%PUTTY_PATH%\pscp.exe" www\index.html %SOURCEFORGE_NAME%@shell.sourceforge.net:/home/groups/f/fl/flickrdown/htdocs/index.html

goto done
:error
pause

:done

