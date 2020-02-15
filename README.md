## Building
Install the [DotNet Core SDK](https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-centos7#install-the-net-core-sdk) and use the [MSBuild](https://github.com/microsoft/msbuild/blob/master/documentation/wiki/Building-Testing-and-Debugging-on-.Net-Core-MSBuild.md#getting-net-core-msbuild-binaries-without-building-the-code) to build the c# projects.  
  
You can use the following command to build the c# projects.  
  
```  
cd /*root of this git repository*/
dotnet build ./DotNet/Microsoft.Build.Utilities.Extension/Microsoft.Build.Utilities.Extension.csproj  -property:Configuration=Debug -property:Platform=AnyCPU
dotnet build ./DotNet/Microsoft.Build.CPPTasks.Common/Microsoft.Build.CPPTasks.Common.csproj  -property:Configuration=Debug -property:Platform=AnyCPU
dotnet build ./DotNet/Microsoft.Build.CPPTasks.Android/Microsoft.Build.CPPTasks.Android.csproj  -property:Configuration=Debug -property:Platform=AnyCPU

```  
  
Use the [Apache NetBeans](https://github.com/YuqiaoZhang/EL7-RPMS/tree/master/netbeans) to build the C++ projects.  
  
You may use the **Apache NetBeans** to open the CPP/Tracker folder and then press **the build button in the UI** to build  
You may use the [LLVM Tool Chain](https://github.com/YuqiaoZhang/EL7-RPMS/tree/master/llvmtoolchain) prebuilt by me.  
  
## Debugging  
You may use the [Visual Studio Code with the C# extension](https://code.visualstudio.com/docs/languages/dotnet) to debug and I have prepared the **json files**(in the DotNet/.vscode) for you.  
The breakpoints may not be hit if you use the **MSBuild in DotNet Core SDK**(in the /usr/share/dotnet/sdk/2.1.803/MSBuild.dll) for debugging.  
You may need to build another [MSBuild](https://github.com/microsoft/msbuild/blob/master/documentation/wiki/Building-Testing-and-Debugging-on-.Net-Core-MSBuild.md#build-1) with the **Debug configuration** for debugging.  
You may need to install the [libssl1.0.0](https://github.com/YuqiaoZhang/EL7-RPMS/tree/master/openssl) before build the **MSBuild** if you use the **CentOS 7** platform.   
