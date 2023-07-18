#!/bin/bash
set -eo pipefail

bin_dir=/usr/local/bin
project_file_path=($(find . -type f -name '*.csproj' -ipath './src/*' \! -ipath '*/tests/*' \! -ipath '*.tests.csproj'))
publish_dir=publish
target_os=osx

if [[ "$project_file_path" == "" ]]; then
    echo "No project file found"
    exit 1
elif [ ${#project_file_path[@]} -gt 1 ]; then
    echo "Multiple project files found: ${project_file_path[@]}"
    exit 1
fi

function project_property() {
    set -eo pipefail
    local name="$1"
    echo "$(cat "$project_file_path" | sed -En "s/.+<$name>([^<]+)<.+/\1/p")"
}

target_framework="$(project_property "TargetFramework")"
project_dir=$(dirname $project_file_path)
project_file=$(basename $project_file_path)

project_target="$(project_property "AssemblyName")"
[[ "$project_target" == "" ]] && project_target="${project_file%.*}"

function dotnet_build() {
    set -eo pipefail
    dotnet build "$project_file_path" >/dev/null
    app_built=1
}

function dotnet_clean() {
    set -eo pipefail
    dotnet clean "$project_file_path" >/dev/null
    app_cleaned=1
    dotnet_build
}

function bold_text() {
    echo -e "\033[0;1m$@\033[0m"
}

function dotnet_publish() {
    set -eo pipefail
    local arg=$1
    local do_copy=0

    echo -n "Publish $project_file to $publish_dir/"
    if [ ! -z $bin_dir ] && [ -d $bin_dir ]; then
        echo " and copy $project_target to $bin_dir/"
        do_copy=1
    else echo; fi
    echo "Target OS: $target_os (override with argument)"
    [[ ! "$arg" =~ clean ]] && echo "(Use ':publish-clean' instead to clean before publish)"
    read -p "Continue? [y/N] " -n 1
    [[ $REPLY == "" ]] && echo -en "\033[1A"
    echo
    [[ $REPLY =~ ^[Yy]$ ]] || exit 0

    if [[ "$arg" =~ clean ]]; then
        bold_text "Cleaning..."
        dotnet_clean
    fi

    bold_text "Publishing..."
    dotnet publish "$project_file_path" -c Release --os $target_os --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o $publish_dir

    if [ $do_copy -eq 1 ] && [ -f $publish_dir/$project_target ]; then
        bold_text "Copying $project_target to $bin_dir..."
        cp $publish_dir/$project_target $bin_dir/
    fi

    exit 0
}

function dotnet_run() {
    set -eo pipefail
    local project_assembly_path="$project_dir/bin/Debug/$target_framework/$project_target"

    if [ -f "$project_assembly_path" ]; then
        "$project_assembly_path" "${app_args[@]}"
    else
        if [ -f "$project_assembly_path.dll" ]; then
            dotnet "$project_assembly_path.dll" "${app_args[@]}"
        elif [ $app_built -eq 0 ]; then
            dotnet_build
            dotnet_run
        else
            echo "No target found"
            exit 1
        fi
    fi
}

app_built=0
app_cleaned=0

if [[ "$1" == ":build" ]]; then
    shift
    dotnet_build
elif [[ "$1" == ":clean" ]]; then
    shift
    dotnet_clean
elif [[ "$1" =~ ^:publish(-clean)?$ ]]; then
    target_os=${2:-$target_os}
    dotnet_publish "$1"
fi

app_args=("$@")

dotnet_run
