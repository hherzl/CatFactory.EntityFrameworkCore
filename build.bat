cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.EntityFrameworkCore
set testPath=%cd%\CatFactory.EntityFrameworkCore.Tests
set outputBasePath=C:\Temp\CatFactory.EntityFrameworkCore
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %outputBasePath%\Store.Tests
dotnet test
cd %outputBasePath%\StoreWithDataAnnotations.Tests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
