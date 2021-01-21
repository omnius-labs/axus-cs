dotnet new sln --force
dotnet sln xeus.sln add (ls -r ./refs/core/src/**/*.csproj)
dotnet sln xeus.sln add (ls -r ./refs/core/test/**/*.csproj)
dotnet sln xeus.sln add (ls -r ./src/**/*.csproj)
dotnet sln xeus.sln add (ls -r ./test/**/*.csproj)
