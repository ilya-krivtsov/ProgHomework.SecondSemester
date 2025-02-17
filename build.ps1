if ($args.Count -ne 1) {
    Write-Host "only one argument 'build' or 'test' should be used"
    exit 1
}

$command = $args[0]
if ($command -eq "build") {
    foreach ($sln in Get-ChildItem "*.sln" -Recurse) {
        dotnet build $sln
    }
}
elseif ($command -eq "test") {
    foreach ($sln in Get-ChildItem "*.sln" -Recurse) {
        dotnet test $sln
    }
}
else {
    Write-Host "unknown command '$command'"
    exit 1
}
