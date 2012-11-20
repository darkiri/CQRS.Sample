properties {
   $sln = "CQRS.Sample.sln"
   $config = "Release"
   $mspec = "packages\Machine.Specifications.0.5.10\tools\mspec-x86-clr4.exe"
}

task default -depends Test

task Test -depends Compile, Clean {
   Exec { &$mspec "Tests\bin\$config\CQRS.Sample.Tests.dll" } 
}

task Compile -depends Clean { 
   Get-ChildItem -i packages.config -rec | %{ .\tools\NuGet\NuGet.exe install $_ -OutputDirectory packages }

   Exec { msbuild /verbosity:m /t:Build /p:Configuration=$config $sln }   
}

task Clean { 
   Exec { msbuild /verbosity:m /t:Clean $sln }   
}

task ? -Description "Helper to display task info" {
   Write-Documentation
}
