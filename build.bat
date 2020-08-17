cls
set initialPath=%cd%
set srcPath=%cd%\CatFactory.EntityFrameworkCore
set testsPath=%cd%\CatFactory.EntityFrameworkCore.Tests
set outputBasePath=C:\Temp\CatFactory.EntityFrameworkCore
cd %srcPath%
dotnet build
cd %testsPath%
dotnet test
cd %outputBasePath%\OnlineStore.Core.Tests
dotnet test
cd %outputBasePath%\OnlineStore.Domain.Tests
dotnet test
cd %outputBasePath%\OnlineStoreWithDataAnnotations.Core.Tests
dotnet test
cd %outputBasePath%\Northwind.Core.Tests
dotnet test
cd %outputBasePath%\AdventureWorks.Core.Tests
dotnet test
cd %srcPath%
dotnet pack
cd %initialPath%
pause
