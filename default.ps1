properties {
   $sln = "CQRS.Sample.sln"
}

task default -depends Test

task Test -depends Compile, Clean { 

}

task Compile -depends Clean { 
   Get-ChildItem -i packages.config -rec | %{ .\tools\NuGet\NuGet.exe install $_ -OutputDirectory packages }

   Exec{ msbuild /verbosity:m /t:Build /p:Configuration=Release $sln }   
}

task Clean { 
   Exec{ msbuild /verbosity:m /t:Clean $sln }   
}

task ? -Description "Helper to display task info" {
   Write-Documentation
}
