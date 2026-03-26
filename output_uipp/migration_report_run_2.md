# COBOL Migration Report - Run 2

**Generated:** 2026-03-20 16:01:36

---

## 📊 Migration Summary

- **Total COBOL Files:** 1
- **Programs (.cbl):** 1
- **Copybooks (.cpy):** 0

- **Total Dependencies:** 131
  - CALL: 2
  - COPY: 61
  - PERFORM: 0
  - EXEC: 0
  - READ: 0
  - WRITE: 1
  - OPEN: 1
  - CLOSE: 2

---

## 📁 File Inventory

|  File Name   |        Path         | Is Copybook |
|--------------|---------------------|-------------|
| ap825_67.cbl | source/ap825_67.cbl | 0           |

---

## 🔗 Dependency Relationships

|    Source    |           Target            |   Type   | Line |         Context          |
|--------------|-----------------------------|----------|------|--------------------------|
| ap825_67.cbl | ERACSF.cbl                  | CALL     | 4547 | Line 4547: CALL 'ERACSF' |
| ap825_67.cbl | GETDATA.cbl                 | CALL     | 557  | Line 557: CALL 'GETDATA' |
| ap825_67.cbl | FPEDOC                      | CLOSE    | 4619 | File Close               |
| ap825_67.cbl | FPEDOC                      | CLOSE    | 5371 | File Close               |
| ap825_67.cbl | API-ERRO-EPORTAL.cpy        | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | API-ERRO.cpy                | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | API-MSG-ERR-EPORTAL.cpy     | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | API-MSG-ERR.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | API-PLATFORM-EXIT.cpy       | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-BENGUELA.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-CONVIID.cpy              | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DATA-HORA.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DATA.cpy                 | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DATAPROC.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DESCR-HA.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DIFDATAS.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-DMSPROTN.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-ERRO-RDMS.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-ERRO-SCRATCH.cpy         | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-FDPERMISSOES.cpy         | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-FDPERMISSOES1.cpy        | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-FDPERMISSOES4.cpy        | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-FILE-STATUS-SFS.cpy      | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-FPEDOC.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-HISTALT.cpy              | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-INCTIME.cpy              | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-PARCELACGA.cpy           | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-PNSCGA.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-PNSCNP.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-PNSCNPVP.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-PNSID.cpy                | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-REGIME.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-STCP.cpy                 | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-TABCUTIL.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-TABCVP.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-TABERROS.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-TABVALFIXO.cpy           | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | DD-VAR-ERROS.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | MCBDEF-UCOB.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | MSG-807.cpy                 | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | MSG-ERR.cpy                 | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DATA-HORA.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DATAPROC.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DESCR-HA.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DIFDATAS.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DMSPROTN.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DMSPROTN1.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-DMSPROTN4.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-ERRO-ERACSF-DMS.cpy      | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-ERRO-RDMS-DMS.cpy        | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-ERRO-SCRATCH.cpy         | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-ERRO-SFS-DMS.cpy         | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-FDPERMISSOES.cpy         | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-INCTIME.cpy              | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-LIMPA-VAR-ERROS.cpy      | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-REGIME.cpy               | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-TABERROS.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-UTIL.cpy                 | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-VALID-DAT.cpy            | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | PD-VER-OCORR-PERMISSOES.cpy | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | RPEDOC.cpy                  | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | RPERMISSOES.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | SELECT-FDPERMISSOES.cpy     | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | SELECT-FPEDOC.cpy           | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | TXIDEF-UCOB.cpy             | COPY     |      | Copybook inclusion       |
| ap825_67.cbl | BENGUELA                    | EXEC SQL | 4438 | FROM BENGUELA            |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 4633 | FROM CONVIID             |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 4673 | INTO CONVIID             |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 4701 | UPDATE CONVIID           |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 6572 | FROM CONVIID             |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 6612 | INTO CONVIID             |
| ap825_67.cbl | CONVIID                     | EXEC SQL | 6640 | UPDATE CONVIID           |
| ap825_67.cbl | END-EXEC                    | EXEC SQL | 3691 | UPDATE END-EXEC          |
| ap825_67.cbl | END-EXEC                    | EXEC SQL | 6093 | UPDATE END-EXEC          |
| ap825_67.cbl | HISTALT                     | EXEC SQL | 4521 | INTO HISTALT             |
| ap825_67.cbl | HISTALT                     | EXEC SQL | 6468 | INTO HISTALT             |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 5894 | FROM PARCELACGA          |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6093 | TABLE PARCELACGA         |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6187 | FROM PARCELACGA          |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6314 | FROM PARCELACGA          |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6336 | FROM PARCELACGA          |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6349 | FROM PARCELACGA          |
| ap825_67.cbl | PARCELACGA                  | EXEC SQL | 6423 | INTO PARCELACGA          |
| ap825_67.cbl | PARCELACGAVP                | EXEC SQL | 5968 | FROM PARCELACGAVP        |
| ap825_67.cbl | PARCELACGAVP                | EXEC SQL | 6147 | FROM PARCELACGAVP        |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 1253 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 1618 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 1641 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 1699 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 3615 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 4272 | FROM PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 4366 | INTO PARCELAS            |
| ap825_67.cbl | PARCELAS                    | EXEC SQL | 6031 | FROM PARCELAS            |
| ap825_67.cbl | PARCELASVP                  | EXEC SQL | 3727 | FROM PARCELASVP          |
| ap825_67.cbl | PNSCGA                      | EXEC SQL | 4786 | FROM PNSCGA              |
| ap825_67.cbl | PNSCGA                      | EXEC SQL | 5491 | FROM PNSCGA              |
| ap825_67.cbl | PNSCGA                      | EXEC SQL | 5845 | UPDATE PNSCGA            |
| ap825_67.cbl | PNSCGA                      | EXEC SQL | 6726 | FROM PNSCGA              |
| ap825_67.cbl | PNSCGA                      | EXEC SQL | 6978 | FROM PNSCGA              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 696  | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 2504 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 3691 | TABLE PNSCNP             |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 3919 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 4158 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 4213 | UPDATE PNSCNP            |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 4458 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 4730 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 5232 | FROM PNSCNP              |
| ap825_67.cbl | PNSCNP                      | EXEC SQL | 6670 | FROM PNSCNP              |
| ap825_67.cbl | PNSID                       | EXEC SQL | 3840 | FROM PNSID               |
| ap825_67.cbl | PNSID                       | EXEC SQL | 4178 | FROM PNSID               |
| ap825_67.cbl | PNSID                       | EXEC SQL | 4974 | UPDATE PNSID             |
| ap825_67.cbl | PNSID                       | EXEC SQL | 5171 | UPDATE PNSID             |
| ap825_67.cbl | PNSID                       | EXEC SQL | 5247 | FROM PNSID               |
| ap825_67.cbl | PNSID                       | EXEC SQL | 5533 | FROM PNSID               |
| ap825_67.cbl | PNSID                       | EXEC SQL | 6869 | UPDATE PNSID             |
| ap825_67.cbl | PNSID                       | EXEC SQL | 6998 | FROM PNSID               |
| ap825_67.cbl | STCP                        | EXEC SQL | 4394 | FROM STCP                |
| ap825_67.cbl | TABCREGIME                  | EXEC SQL | 6534 | FROM TABCREGIME          |
| ap825_67.cbl | TABCVP                      | EXEC SQL | 3763 | FROM TABCVP              |
| ap825_67.cbl | TABCVP                      | EXEC SQL | 6108 | FROM TABCVP              |
| ap825_67.cbl | TABVALFIXO                  | EXEC SQL | 3684 | TABLE TABVALFIXO         |
| ap825_67.cbl | TABVALFIXO                  | EXEC SQL | 3858 | FROM TABVALFIXO          |
| ap825_67.cbl | VRS017                      | EXEC SQL | 4755 | INTO VRS017              |
| ap825_67.cbl | VRS017                      | EXEC SQL | 4813 | INTO VRS017              |
| ap825_67.cbl | VRS017                      | EXEC SQL | 6695 | INTO VRS017              |
| ap825_67.cbl | VRS017                      | EXEC SQL | 6753 | INTO VRS017              |
| ap825_67.cbl | VRS037                      | EXEC SQL | 3637 | INTO VRS037              |
| ap825_67.cbl | VRS037                      | EXEC SQL | 5916 | INTO VRS037              |
| ap825_67.cbl | FPEDOC                      | OPEN     | 4553 | I-O Mode                 |
| ap825_67.cbl | RPEDOC                      | WRITE    | 4600 | File Write               |

---

*Report generated by COBOL Migration Tool*
