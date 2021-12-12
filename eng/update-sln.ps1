dotnet new sln --force -n axis
dotnet sln axis.sln add (ls -r ./refs/core/src/**/*.csproj)
dotnet sln axis.sln add (ls -r ./refs/core/test/**/*.csproj)
dotnet sln axis.sln add (ls -r ./src/**/*.csproj)
dotnet sln axis.sln add (ls -r ./test/**/*.csproj)
