rem Package the library for Nuget
del lib\*.dll /s /q
copy ..\MaxFactry.Provider.CoreProvider-NF-2.0\bin\Release\MaxFactry.Provider.Core*.dll lib\net20\
copy ..\MaxFactry.Provider.CoreProvider-NF-4.5.2\bin\Release\MaxFactry.Provider.Core*.dll lib\net452\
copy ..\MaxFactry.Provider.CoreProvider-NF-4.7.2\bin\Release\MaxFactry.Provider.Core*.dll lib\net472\
copy ..\MaxFactry.Provider.CoreProvider-NF-4.8\bin\Release\MaxFactry.Provider.Core*.dll lib\net48\
copy ..\MaxFactry.Provider.CoreProvider-NC-3.1\bin\Release\netcoreapp3.1\MaxFactry.Provider.Core*.dll lib\netcoreapp3.1\
copy ..\MaxFactry.Provider.CoreProvider-NC-6.0\bin\Release\net6.0\MaxFactry.Provider.Core*.dll lib\net6.0\
copy ..\MaxFactry.Provider.CoreProvider-NC-8.0\bin\Release\net8.0\MaxFactry.Provider.Core*.dll lib\net8.0\

c:\install\nuget\nuget.exe pack MaxFactry.Provider.Core.nuspec -OutputDirectory "packages" -IncludeReferencedProjects -properties Configuration=Release 