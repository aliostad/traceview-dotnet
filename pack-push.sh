rm -rf publish/
dotnet pack TraceView.Listener.sln -o ./publish -c Release
dotnet nuget push "publish/*.nupkg" -s nuget.org -k $NUGET_API_KEY