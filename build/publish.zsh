#!/bin/zsh

zmodload zsh/zutil

MYDIR=${0:A:h}

setopt EXTENDED_GLOB
setopt NULL_GLOB
unsetopt NOMATCH

# -- CS project

CS_PRJ=$MYDIR/../src/ARXCheck/ARXCheck/ARXCheck.csproj
CS_PRJ=${CS_PRJ:A}

# -- config dir
CONF_DIR=$MYDIR/config
CONF_DIR=${CONF_DIR:A}

# -- examples dir
EXMPL_DIR=$MYDIR/examples
EXMPL_DIR=${EXMPL_DIR:A}

# -- build output path
BIN_DIR=$MYDIR/../bin
BIN_DIR=${BIN_DIR:A}

# -- build output file name
BIN_FILE_DIR=ARXCheck

# -- build output path windows (self contained exe binary)
WIN_SC_BIN_DIR=$BIN_DIR/windows/SelfContained/$BIN_FILE_DIR

# -- build output path windows (not self contained exe binary)
WIN_NSC_BIN_DIR=$BIN_DIR/windows/NotSelfContained/$BIN_FILE_DIR

# -- build output path linux (self contained exe binary)
LIN_SC_BIN_DIR=$BIN_DIR/linux/SelfContained/$BIN_FILE_DIR

# -- build output path linux (not self contained exe binary)
LIN_NSC_BIN_DIR=$BIN_DIR/linux/NotSelfContained/$BIN_FILE_DIR

# -- build output path macos-x64 (intel) (self contained exe binary)
MAC64_SC_BIN_DIR=$BIN_DIR/macos-x64/SelfContained/$BIN_FILE_DIR

# -- build output path macos-x64 (intel) (not self contained exe binary)
MAC64_NSC_BIN_DIR=$BIN_DIR/macos-x64/NotSelfContained/$BIN_FILE_DIR

# -- build output path macos-arm64 (silicon) (self contained exe binary)
MACARM_SC_BIN_DIR=$BIN_DIR/macos-arm64/SelfContained/$BIN_FILE_DIR

# -- build output path macos-arm64 (silicon) (not self contained exe binary)
MACARM_NSC_BIN_DIR=$BIN_DIR/macos-arm64/NotSelfContained/$BIN_FILE_DIR

# zsh options:
#
# setopt +o nomatch: Unset nomatch option. See https://zsh.sourceforge.io/Doc/Release/Options.html : If a pattern for filename generation has no matches, print an error, instead of leaving it unchanged in the argument list"
setopt +o nomatch
# setopt rm_star_silent: Set rm_star_silent option. See https://zsh.sourceforge.io/Doc/Release/Options.html : "Do not query the user before executing 'rm *' or 'rm path/*'."
setopt rm_star_silent
# rm -fr : See 'man rm': -f "Attempt to remove the files without prompting for confirmation, regardless of the file's permissions.  If the file does not exist, do not display a diagnostic message or modify the exit status to reflect an error."
#                        -r "Attempt to remove the file hierarchy rooted in each file argument. The -R option implies the -d option."

rm -fr $WIN_SC_BIN_DIR
rm -fr $WIN_NSC_BIN_DIR
rm -fr $LIN_SC_BIN_DIR
rm -fr $LIN_NSC_BIN_DIR
rm -fr $MAC64_SC_BIN_DIR
rm -fr $MAC64_NSC_BIN_DIR
rm -fr $MACARM_SC_BIN_DIR
rm -fr $MACARM_NSC_BIN_DIR

mkdir -p $WIN_SC_BIN_DIR
mkdir -p $WIN_NSC_BIN_DIR
mkdir -p $LIN_SC_BIN_DIR
mkdir -p $LIN_NSC_BIN_DIR
mkdir -p $MAC64_SC_BIN_DIR
mkdir -p $MAC64_NSC_BIN_DIR
mkdir -p $MACARM_SC_BIN_DIR
mkdir -p $MACARM_NSC_BIN_DIR

cp -R $CONF_DIR/* $WIN_SC_BIN_DIR
cp -R $CONF_DIR/* $WIN_NSC_BIN_DIR
cp -R $CONF_DIR/* $LIN_SC_BIN_DIR
cp -R $CONF_DIR/* $LIN_NSC_BIN_DIR
cp -R $CONF_DIR/* $MAC64_SC_BIN_DIR
cp -R $CONF_DIR/* $MAC64_NSC_BIN_DIR
cp -R $CONF_DIR/* $MACARM_SC_BIN_DIR
cp -R $CONF_DIR/* $MACARM_NSC_BIN_DIR

cp -R $EXMPL_DIR/* $WIN_SC_BIN_DIR
cp -R $EXMPL_DIR/* $WIN_NSC_BIN_DIR
cp -R $EXMPL_DIR/* $LIN_SC_BIN_DIR
cp -R $EXMPL_DIR/* $LIN_NSC_BIN_DIR
cp -R $EXMPL_DIR/* $MAC64_SC_BIN_DIR
cp -R $EXMPL_DIR/* $MAC64_NSC_BIN_DIR
cp -R $EXMPL_DIR/* $MACARM_SC_BIN_DIR
cp -R $EXMPL_DIR/* $MACARM_NSC_BIN_DIR

unsetopt rm_star_silent
setopt -o nomatch

#break 2> /dev/null

dotnet clean $CS_PRJ

#break 2> /dev/null
dotnet publish $CS_PRJ -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o $WIN_SC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false -o $WIN_NSC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained true -o $LIN_SC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false -o $LIN_NSC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r osx-x64 -p:PublishSingleFile=true --self-contained true -o $MAC64_SC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r osx-x64 -p:PublishSingleFile=true --self-contained false -o $MAC64_NSC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true -o $MACARM_SC_BIN_DIR
dotnet publish $CS_PRJ -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained false -o $MACARM_NSC_BIN_DIR