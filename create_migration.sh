# Example usage: ./create_migration.sh 'some new migration name'
# Example output (date will be different for you): <path>/20230720135309_some_new_migration_name.sql

set -e

# must be run WITHIN the repository folder structure, but can be run ANYWHERE within it
PATH_TO_SCRIPTS="$(git rev-parse --show-toplevel)/WookiepediaStatusArticleData/Database/Migrate"
# uses UTC (-u) as the timezone so it doesn't matter which timezone you call this from,
# it will be the same for everyone
VERSION_NUMBER=$(date -u "+%Y%m%d%H%M%S")
# collapse whitespace and replace it with a single underscore (_)
FILE_NAME=$(echo $1 | tr -s ' ' '_')

if [ "$FILE_NAME" = "" ]; then
    echo "First argument must be a valid name that isn't empty or just spaces"
    exit 1
fi

FULL_PATH="$PATH_TO_SCRIPTS"/"$VERSION_NUMBER"_"$FILE_NAME.sql"

touch $FULL_PATH
echo "$FULL_PATH"