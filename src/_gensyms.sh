#!/bin/bash
basepath=$(dirname $(readlink -f "$0"))
basepath_dir=$(dirname "${basepath}")

get_arch=`arch`

if [[ $get_arch =~ "x86_64" ]];then
    echo "dump_syms $1 > $2"
    chmod +x $basepath/dump_analyzer/x86_64/tools/dump_syms
    $basepath/dump_analyzer/x86_64/tools/dump_syms $1 > $2
elif [[ $get_arch =~ "aarch64" ]];then
    echo "dump_syms $1 > $2"
    chmod +x $basepath/dump_analyzer/aarch64/tools/dump_syms
    $basepath/dump_analyzer/aarch64/tools/dump_syms $1 > $2
elif [[ $get_arch =~ "mips64" ]];then
    echo "this is mips64"
else
    echo "unknown!!"
fi