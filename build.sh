command=$1

if [[ "$#" -ne 1 ]]; then
    echo "only one argument 'build' or 'test' should be used"
    exit 1
fi

if [[ "$command" == "build" ]] ; then
    for sln in $(find -name *.sln); do
        dotnet build $sln
    done
elif [[ "$command" == "test" ]] ; then
    for sln in $(find -name *.sln); do
        dotnet test $sln
    done
else
    echo "unknown command '$command'"
    exit 1
fi
