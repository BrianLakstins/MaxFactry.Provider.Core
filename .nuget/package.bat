rem Package the library for Nuget
copy ..\MaxFactry.Provider.CoreProvider-NF-2.0\bin\Release\MaxFactry.Provider.Core*.dll lib\net20\
copy ..\MaxFactry.Provider.CoreProvider-NF-4.5.2\bin\Release\MaxFactry.Provider.Core*.dll lib\net452\

c:\install\nuget\nuget.exe pack MaxFactry.Provider.Core.nuspec -OutputDirectory "packages" -IncludeReferencedProjects -properties Configuration=Release 