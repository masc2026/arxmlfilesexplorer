#!/bin/zsh

zmodload zsh/zutil

MYDIR=${0:A:h}

setopt EXTENDED_GLOB
setopt NULL_GLOB
unsetopt NOMATCH

URLS=(
 "https://www.autosar.org/fileadmin/standards/R17-03_R1.1.0/AP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R17-10_R1.2.0/AP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R4.3.1/CP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R19-11/CP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R19-03/AP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R18-10_R4.4.0_R1.5.0/AP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R18-03_R1.4.0/AP/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R24-11/FO/AUTOSAR_FO_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R23-11/FO/AUTOSAR_FO_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R22-11/FO/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R21-11/FO/AUTOSAR_MMOD_XMLSchema.zip"
 "https://www.autosar.org/fileadmin/standards/R20-11/FO/AUTOSAR_MMOD_XMLSchema.zip"
)

typeset -U URLS

REPO_DIR="$MYDIR/../"
REPO_DIR=${REPO_DIR:A}
TARGET_DIR="$MYDIR/config/xsd"
TMP_DIR="$MYDIR/tmp/$(date +%s)"
ARXML_OUT_DIR="$REPO_DIR/src/ARXCheck/ARXCheck/input/"
RESX_PATH="$REPO_DIR/src/ARXCheck/ARXCheck/Resources.resx"

# mkdir -p "$TARGET_DIR"
mkdir -p "$TMP_DIR"
mkdir -p "$ARXML_OUT_DIR"

typeset -i i=1 max=${#URLS[*]}

while (( i <= max )) do
  url=${URLS[$i]}
  echo "Verarbeite: $url"
  
  zip_file="$TMP_DIR/schema${(l:3::0:)i}.zip"
  
  http_code=$(curl -s -L -w "%{http_code}" -o "$zip_file" "$url")
  wget --quiet --output-document="$zip_file" "$url"
  
  if [[ $? -ne 0 ]]; then
    echo "  Fehler: Download von $url fehlgeschlagen (wget Exit-Code: $?)."
    continue
  fi

  if ! file "$zip_file" | grep -q "Zip archive"; then
     echo "  Fehler: Die heruntergeladene Datei ist kein gültiges ZIP-Archiv."
     continue
  fi

  extract_dir="$TMP_DIR/extracted${(l:3::0:)i}"
  mkdir -p "$extract_dir"
  
  unzip -q -o -j "$zip_file" "*.xsd" -d "$extract_dir" 2>/dev/null
  
  count=( "$extract_dir"/*.xsd(N) )
  if [[ ${#count[@]} -gt 0 ]]; then
      cp "$extract_dir"/*.xsd "$TARGET_DIR/"
      echo "  Erfolgreich entpackt."
  else
      echo "  Warnung: Keine .xsd-Datei gefunden."
  fi
  i=i+1
done

echo "Erstelle BaseX Datenbank 'XSD' neu ..."
BXS_SCRIPT="$TMP_DIR/build.bxs"
echo "SET CREATEFILTER AUTOSAR*.xsd" > "$BXS_SCRIPT"
echo "CREATE DB XSD $TARGET_DIR" >> "$BXS_SCRIPT"

basex "$BXS_SCRIPT"

echo "Überprüfe Import ..."
FOLDER_COUNT=$(ls -1q "$TARGET_DIR"/AUTOSAR*.xsd 2>/dev/null | wc -l)

DB_COUNT=$(basex -q "if(db:exists('XSD')) then count(db:list('XSD')) else -1")

if [[ "$DB_COUNT" -eq "-1" ]]; then
  echo "Fehler: Datenbank 'XSD' konnte nicht erstellt werden."
elif [[ "$FOLDER_COUNT" -eq "$DB_COUNT" ]]; then
  echo "Erfolg: Die Datenbank ist aktuell und enthält exakt $DB_COUNT Dateien."
  basex -boutdir="$ARXML_OUT_DIR" -bresxFilePath="$RESX_PATH" "$MYDIR"/main.xqy
else
  echo "Warnung: Unterschiedliche Anzahl von Daten in der Datenbank!"
  echo " Dateien im Ordner: $FOLDER_COUNT"
  echo " Dateien in der DB:  $DB_COUNT"
fi