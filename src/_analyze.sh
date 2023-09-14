#!/bin/bash
basepath=$(dirname $(readlink -f "$0"))
basepath_dir=$(dirname "${basepath}")

get_arch=`arch`

if [[ $get_arch =~ "x86_64" ]];then
    echo "minidump_stackwalk $1 $2 > '$1.txt'"
    chmod +x $basepath/dump_analyzer/x86_64/tools/minidump_stackwalk
    $basepath/dump_analyzer/x86_64/tools/minidump_stackwalk $1 $2 > "$1.txt"
elif [[ $get_arch =~ "aarch64" ]];then
    echo "minidump_stackwalk $1 $2 > '$1.txt'"
    chmod +x $basepath/dump_analyzer/aarch64/tools/minidump_stackwalk
    $basepath/dump_analyzer/aarch64/tools/minidump_stackwalk $1 $2 > "$1.txt"
elif [[ $get_arch =~ "mips64" ]];then
    echo "this is mips64"
else
    echo "unknown!!"
fi