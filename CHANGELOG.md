# Release Info:
## Version 1.0.10:
### Features:
- Automatisierung von Download für AUTOSAR XSD-Dateien im Setup-Schritt (Dateien sind nicht mehr im Repository)
- Generierung von XML-Input-Dateien über BaseX 12 und XQuery
- Automatisches Update für `Resources.resx` durch XQuery-Skript
- Umstellung auf relative Pfade in Zsh-Skripten (`publish.zsh`, `setup.zsh`) für portable Ausführung
- Beispiele mit Adaptive Datentypen zugefügt (`sample05.arxml` und `sample06.arxml`)
- Beispiel, das nicht Schema-valide ist zugefügt (`sample05.arxml`)
- .vscode in das Repository Root Verzeichnis verschoben und angepasst
- README.md überarbeitet
- CHANGELOG.md hinzugefügt
- LICENSE hinzugefügt
- LICENSE_EXAMPLES und NOTICE für sample01 bis 04 hinzugefügt
## Version 1.0.9.1:
### Features:
- New configuration for build and debugging in VS Code.
## Version 1.0.9.0:
### Features:
- Ready for SDK and Runtime .Net8
- With **Option -?, -h, --help**: show help.
- With **Option --version**: show the tools version.
## Version 1.0.8.0:
### Features:
- With **Option V**: show the tools version.
- With **Option c**: specifiy a config file.
- With **Option v**: see `XML` Schema validation errors and warnings. The following `XSD`s are supported:
    * AUTOSAR_4_0_3.xsd
    * AUTOSAR_4_1_3.xsd
    * AUTOSAR_4_2_2.xsd
    * AUTOSAR_00042.xsd
    * AUTOSAR_00043.xsd
    * AUTOSAR_00045.xsd
    * AUTOSAR_00044.xsd
    * AUTOSAR_00046.xsd
    * AUTOSAR_00047.xsd
    * AUTOSAR_00048.xsd
    * AUTOSAR_00049.xsd
- With **Option p**: list `XPath` or _XPath similar_ expressions that let identify easily the element nodes in the `ARXML` file where the ComplexTypes are used.
- With **Option s**: set the XSD explicitly and use it instead of the value in `schemaLocation` in the `ar:AUTOSAR` element node in the `ARXML` file.
- In the config file: configuration of the assignment of different spellings of the XSD format in the `ARXML` files to one of the supported `XSD` formats as an alias, see [ARXCheckConfig.xml](bin/configschema/ARXCheckConfig.xml).
- List all ComplexTypes used in the `ARXML` file.
- List and identify what AUTOSAR Adaptive only ComplexTypes are used in the `ARXML` file.