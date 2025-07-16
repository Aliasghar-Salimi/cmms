#!/bin/bash

# Version Management Script for CMMS Identity Service
# Usage: ./version.sh [get|set|bump|info]

PROJECT_FILE="IdentityService.csproj"
CURRENT_VERSION=""

get_current_version() {
    CURRENT_VERSION=$(grep -o '<Version>.*</Version>' $PROJECT_FILE | sed 's/<Version>\(.*\)<\/Version>/\1/')
    echo $CURRENT_VERSION
}

set_version() {
    local new_version=$1
    if [[ -z "$new_version" ]]; then
        echo "Error: Version number required"
        echo "Usage: ./version.sh set <version>"
        exit 1
    fi
    
    # Update project file
    sed -i "s/<Version>.*<\/Version>/<Version>$new_version<\/Version>/" $PROJECT_FILE
    sed -i "s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>$new_version.0<\/AssemblyVersion>/" $PROJECT_FILE
    sed -i "s/<FileVersion>.*<\/FileVersion>/<FileVersion>$new_version.0<\/FileVersion>/" $PROJECT_FILE
    sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>$new_version<\/InformationalVersion>/" $PROJECT_FILE
    
    echo "✅ Version updated to $new_version"
}

bump_version() {
    local bump_type=$1
    local current=$(get_current_version)
    local major=$(echo $current | cut -d. -f1)
    local minor=$(echo $current | cut -d. -f2)
    local patch=$(echo $current | cut -d. -f3)
    
    case $bump_type in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            ;;
        patch)
            patch=$((patch + 1))
            ;;
        *)
            echo "Error: Invalid bump type. Use: major, minor, or patch"
            exit 1
            ;;
    esac
    
    local new_version="$major.$minor.$patch"
    set_version $new_version
}

show_info() {
    echo "📋 Version Information:"
    echo "========================"
    echo "Current Version: $(get_current_version)"
    echo "Assembly Version: $(grep -o '<AssemblyVersion>.*</AssemblyVersion>' $PROJECT_FILE | sed 's/<AssemblyVersion>\(.*\)<\/AssemblyVersion>/\1/')"
    echo "File Version: $(grep -o '<FileVersion>.*</FileVersion>' $PROJECT_FILE | sed 's/<FileVersion>\(.*\)<\/FileVersion>/\1/')"
    echo "Informational Version: $(grep -o '<InformationalVersion>.*</InformationalVersion>' $PROJECT_FILE | sed 's/<InformationalVersion>\(.*\)<\/InformationalVersion>/\1/')"
    echo ""
    echo "📦 Project Details:"
    echo "==================="
    echo "Company: $(grep -o '<Company>.*</Company>' $PROJECT_FILE | sed 's/<Company>\(.*\)<\/Company>/\1/')"
    echo "Product: $(grep -o '<Product>.*</Product>' $PROJECT_FILE | sed 's/<Product>\(.*\)<\/Product>/\1/')"
    echo "Description: $(grep -o '<Description>.*</Description>' $PROJECT_FILE | sed 's/<Description>\(.*\)<\/Description>/\1/')"
    echo ""
    echo "🔧 Build Commands:"
    echo "=================="
    echo "Build: dotnet build"
    echo "Run: dotnet run"
    echo "Test: dotnet test"
    echo "Pack: dotnet pack"
}

case "$1" in
    get)
        get_current_version
        ;;
    set)
        set_version $2
        ;;
    bump)
        bump_version $2
        ;;
    info)
        show_info
        ;;
    *)
        echo "Version Management Script for CMMS Identity Service"
        echo ""
        echo "Usage: $0 {get|set|bump|info}"
        echo ""
        echo "Commands:"
        echo "  get                    - Get current version"
        echo "  set <version>          - Set specific version (e.g., 1.2.3)"
        echo "  bump {major|minor|patch} - Bump version by type"
        echo "  info                   - Show detailed version information"
        echo ""
        echo "Examples:"
        echo "  $0 get                 # Get current version"
        echo "  $0 set 1.2.3          # Set version to 1.2.3"
        echo "  $0 bump minor          # Bump minor version"
        echo "  $0 info                # Show all version info"
        exit 1
        ;;
esac 