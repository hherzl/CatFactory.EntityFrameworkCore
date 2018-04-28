cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.EntityFrameworkCore\CatFactory.EntityFrameworkCore
set testPath=%cd%\CatFactory.EntityFrameworkCore\CatFactory.EntityFrameworkCore.Tests
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
