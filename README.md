Apache cTakes parser
====================

## Prerequiresites
- cTakes binaries (http://apache.ip-connect.vn.ua//ctakes/ctakes-4.0.0/apache-ctakes-4.0.0-bin.zip)
- cTakes dictionaries (http://sourceforge.net/projects/ctakesresources/files/ctakes-resources-4.0-bin.zip/download) or (https://github.com/acstevens/ctakes-standalone-application/tree/master/src/main/resources/org/apache/ctakes/dictionary/lookup) These files should be enough.

- Unpack `apache-ctakes-4.0.0-bin.zip` to `%CTAKES_HOME%`
- Unpack `ctakes-resources-4.0-bin.zip` to `%CTAKES_HOME%` (exact same location as `apache-ctakes-4.0.0-bin.zip`)

- Copy file `data\Indications.xls` to location specified in the `indications_file` configuration
- Copy folder `data\mednotes\ to `%CTAKES_HOME%` location.
- Copy folder `data\run\runctakesCPE_CLI.bat` to `%CTAKES_HOME%\bin` location.
- Open `%CTAKES_HOME%\bin\run\runctakesCPE_CLI.bat` and change user name and password for your UMLS license.
