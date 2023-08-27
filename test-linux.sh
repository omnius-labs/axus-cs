export StableTest="true"

dotnet format --verify-no-changes
dotnet test --no-restore --filter "FullyQualifiedName~Omnius.Axus"
