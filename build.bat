copy /b ^
  src\*.cs ^
  SpaceGameboy.lib.cs
echo.> SpaceGameboy.min.cs
echo.>> SpaceGameboy.min.cs
CSharpMinifier\CSharpMinify --locals --members --types --spaces --regions --comments --namespaces --to-string-methods --enum-to-int --line-length 100000 --skip-compile SpaceGameboy.lib.cs >> SpaceGameboy.min.cs
copy /b ^
  Main.cs +^
  SpaceGameboy.min.cs ^
  SpaceGameboy.cs
copy /b ^
  Main.cs +^
  SpaceGameboy.lib.cs ^
  SpaceGameboy.debug.cs  
del SpaceGameboy.lib.cs
del SpaceGameboy.min.cs
pause
