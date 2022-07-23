dotnet new sln --force -n axus
dotnet sln axus.sln add (ls -r ./refs/core/src/**/*.csproj)
dotnet sln axus.sln add (ls -r ./refs/core/test/**/*.csproj)
dotnet sln axus.sln add (ls -r ./src/**/*.csproj)
dotnet sln axus.sln add (ls -r ./test/**/*.csproj)
