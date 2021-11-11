#!/bin/sh

file_list=(
    "slua_OSX_32"
    "slua_OSX_64"
    "sluac_OSX_32"
    "sluac_OSX_64"
    "xxtea"
)

files_processed=()

for file in ${file_list[@]}
do
    if !(test -f $file)
    then
        echo '文件'${file}'不存在'
        exit -1
    fi

    if !(test -x $file)
    then
        chmod +x $file
        files_processed+=($file)
    fi
done

echo ${files_processed[@]}
