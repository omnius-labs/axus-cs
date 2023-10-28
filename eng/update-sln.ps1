dotnet new sln --force -n axus
dotnet sln axus.sln add (ls -r ./refs/core-cs/src/**/*.csproj)
dotnet sln axus.sln add (ls -r ./refs/core-cs/test/**/*.csproj)
dotnet sln axus.sln add (ls -r ./src/**/*.csproj)
dotnet sln axus.sln add (ls -r ./test/**/*.csproj)
