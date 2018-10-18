cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.EntityFrameworkCore
set testPath=%cd%\CatFactory.EntityFrameworkCore.Tests
set outputBasePath=C:\Temp\CatFactory.EntityFrameworkCore
cd %srcPath%
dotnet build
cd %testPath%
dotnet test
cd %outputBasePath%\Store.Core.Tests
dotnet test
cd %outputBasePath%\StoreWithDataAnnotations.Core.Tests
dotnet test
cd %outputBasePath%\Northwind.Core.Tests
dotnet test
cd %outputBasePath%\AdventureWorks.Core.Tests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
