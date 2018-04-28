cls
set initialPath=%cd%
set srcPath=%cd%\src\CatFactory.EfCore
set testPath=%cd%\test\CatFactory.EfCore.Tests
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause