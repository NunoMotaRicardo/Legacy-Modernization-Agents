# COBOL Dependency Diagram

```mermaid
flowchart LR
  %% Program
  ap825[/"ap825_67.cbl"/]

  %% Calls
  subgraph Calls
    direction TB
    getdata[GETDATA.cbl]
    eracsf[ERACSF.cbl]
  end

  %% Files / File operations
  subgraph Files
    direction TB
    fpedoc[FPEDOC]
    rpedoc[RPEDOC]
  end

  %% SQL objects / EXEC SQL targets
  subgraph "EXEC SQL / DB Objects"
    direction TB
    pnscnp[PNSCNP]
    parcelas[PARCELAS]
    vrs037[VRS037]
    tabvalfixo[TABVALFIXO]
    endexec["END-EXEC"]
    parcelasvp[PARCELASVP]
    tabcvp[TABCVP]
    pnsid[PNSID]
    stcp[STCP]
    benguela[BENGUELA]
    histalt[HISTALT]
    conviid[CONVIID]
    vrs017[VRS017]
    pnscga[PNSCGA]
    parcelacga[PARCELACGA]
    parcelacgavp[PARCELACGAVP]
    tabcregime[TABCREGIME]
  end

  %% Copybooks
  subgraph "COPYBOOKS (COPY)"
    direction TB
    cb_SELECT_FDPERMISSOES["SELECT-FDPERMISSOES.cpy"]
    cb_SELECT_FPEDOC["SELECT-FPEDOC.cpy"]
    cb_RPERMISSOES["RPERMISSOES.cpy"]
    cb_RPEDOC["RPEDOC.cpy"]
    cb_TXIDEF_UCOB["TXIDEF-UCOB.cpy"]
    cb_MCBDEF_UCOB["MCBDEF-UCOB.cpy"]
    cb_DD_DMSPROTN["DD-DMSPROTN.cpy"]
    cb_MSG_ERR["MSG-ERR.cpy"]
    cb_MSG_807["MSG-807.cpy"]
    cb_DD_DATA["DD-DATA.cpy"]
    cb_DD_TABERROS["DD-TABERROS.cpy"]
    cb_DD_DATA_HORA["DD-DATA-HORA.cpy"]
    cb_DD_ERRO_RDMS["DD-ERRO-RDMS.cpy"]
    cb_DD_DESCR_HA["DD-DESCR-HA.cpy"]
    cb_DD_DIFDATAS["DD-DIFDATAS.cpy"]
    cb_DD_DATAPROC["DD-DATAPROC.cpy"]
    cb_DD_INCTIME["DD-INCTIME.cpy"]
    cb_DD_REGIME["DD-REGIME.cpy"]
    cb_DD_FDPERMISSOES["DD-FDPERMISSOES.cpy"]
    cb_DD_FDPERMISSOES4["DD-FDPERMISSOES4.cpy"]
    cb_DD_FDPERMISSOES1["DD-FDPERMISSOES1.cpy"]
    cb_DD_FILE_STATUS_SFS["DD-FILE-STATUS-SFS.cpy"]
    cb_DD_TABCUTIL["DD-TABCUTIL.cpy"]
    cb_DD_ERRO_SCRATCH["DD-ERRO-SCRATCH.cpy"]
    cb_DD_FPEDOC["DD-FPEDOC.cpy"]
    cb_DD_TABCVP["DD-TABCVP.cpy"]
    cb_DD_PNSCNPVP["DD-PNSCNPVP.cpy"]
    cb_DD_PNSCNP["DD-PNSCNP.cpy"]
    cb_DD_CONVIID["DD-CONVIID.cpy"]
    cb_DD_PNSCGA["DD-PNSCGA.cpy"]
    cb_DD_PARCELACGA["DD-PARCELACGA.cpy"]
    cb_DD_PNSID["DD-PNSID.cpy"]
    cb_DD_STCP["DD-STCP.cpy"]
    cb_DD_BENGUELA["DD-BENGUELA.cpy"]
    cb_DD_HISTALT["DD-HISTALT.cpy"]
    cb_DD_TABVALFIXO["DD-TABVALFIXO.cpy"]
    cb_DD_VAR_ERROS["DD-VAR-ERROS.cpy"]
    cb_PD_ERRO_SCRATCH["PD-ERRO-SCRATCH.cpy"]
    cb_PD_UTIL["PD-UTIL.cpy"]
    cb_PD_REGIME["PD-REGIME.cpy"]
    cb_PD_VER_OCORR_PERMISSOES["PD-VER-OCORR-PERMISSOES.cpy"]
    cb_PD_FDPERMISSOES["PD-FDPERMISSOES.cpy"]
    cb_PD_DATAPROC["PD-DATAPROC.cpy"]
    cb_PD_DIFDATAS["PD-DIFDATAS.cpy"]
    cb_PD_DESCR_HA["PD-DESCR-HA.cpy"]
    cb_PD_TABERROS["PD-TABERROS.cpy"]
    cb_PD_DATA_HORA["PD-DATA-HORA.cpy"]
    cb_PD_INCTIME["PD-INCTIME.cpy"]
    cb_PD_ERRO_RDMS_DMS["PD-ERRO-RDMS-DMS.cpy"]
    cb_PD_ERRO_ERACSF_DMS["PD-ERRO-ERACSF-DMS.cpy"]
    cb_PD_ERRO_SFS_DMS["PD-ERRO-SFS-DMS.cpy"]
    cb_API_MSG_ERR["API-MSG-ERR.cpy"]
    cb_API_ERRO["API-ERRO.cpy"]
    cb_PD_VALID_DAT["PD-VALID-DAT.cpy"]
    cb_API_MSG_ERR_EPORTAL["API-MSG-ERR-EPORTAL.cpy"]
    cb_API_ERRO_EPORTAL["API-ERRO-EPORTAL.cpy"]
    cb_API_PLATFORM_EXIT["API-PLATFORM-EXIT.cpy"]
    cb_PD_DMSPROTN["PD-DMSPROTN.cpy"]
    cb_PD_DMSPROTN4["PD-DMSPROTN4.cpy"]
    cb_PD_DMSPROTN1["PD-DMSPROTN1.cpy"]
    cb_PD_LIMPA_VAR_ERROS["PD-LIMPA-VAR-ERROS.cpy"]
  end

  %% Edges: Calls
  ap825 -->|CALL| getdata
  ap825 -->|CALL| eracsf

  %% Edges: File operations
  ap825 -->|OPEN file| fpedoc
  ap825 -->|WRITE file| rpedoc
  ap825 -->|CLOSE file| fpedoc

  %% Edges: EXEC SQL (deduplicated)
  ap825 -->|EXEC SQL| pnscnp
  ap825 -->|EXEC SQL| parcelas
  ap825 -->|EXEC SQL| vrs037
  ap825 -->|EXEC SQL| tabvalfixo
  ap825 -->|EXEC SQL| endexec
  ap825 -->|EXEC SQL| parcelasvp
  ap825 -->|EXEC SQL| tabcvp
  ap825 -->|EXEC SQL| pnsid
  ap825 -->|EXEC SQL| stcp
  ap825 -->|EXEC SQL| benguela
  ap825 -->|EXEC SQL| histalt
  ap825 -->|EXEC SQL| conviid
  ap825 -->|EXEC SQL| vrs017
  ap825 -->|EXEC SQL| pnscga
  ap825 -->|EXEC SQL| parcelacga
  ap825 -->|EXEC SQL| parcelacgavp
  ap825 -->|EXEC SQL| tabcregime

  %% Edges: COPYBOOKS
  ap825 -->|COPY| cb_SELECT_FDPERMISSOES
  ap825 -->|COPY| cb_SELECT_FPEDOC
  ap825 -->|COPY| cb_RPERMISSOES
  ap825 -->|COPY| cb_RPEDOC
  ap825 -->|COPY| cb_TXIDEF_UCOB
  ap825 -->|COPY| cb_MCBDEF_UCOB
  ap825 -->|COPY| cb_DD_DMSPROTN
  ap825 -->|COPY| cb_MSG_ERR
  ap825 -->|COPY| cb_MSG_807
  ap825 -->|COPY| cb_DD_DATA
  ap825 -->|COPY| cb_DD_TABERROS
  ap825 -->|COPY| cb_DD_DATA_HORA
  ap825 -->|COPY| cb_DD_ERRO_RDMS
  ap825 -->|COPY| cb_DD_DESCR_HA
  ap825 -->|COPY| cb_DD_DIFDATAS
  ap825 -->|COPY| cb_DD_DATAPROC
  ap825 -->|COPY| cb_DD_INCTIME
  ap825 -->|COPY| cb_DD_REGIME
  ap825 -->|COPY| cb_DD_FDPERMISSOES
  ap825 -->|COPY| cb_DD_FDPERMISSOES4
  ap825 -->|COPY| cb_DD_FDPERMISSOES1
  ap825 -->|COPY| cb_DD_FILE_STATUS_SFS
  ap825 -->|COPY| cb_DD_TABCUTIL
  ap825 -->|COPY| cb_DD_ERRO_SCRATCH
  ap825 -->|COPY| cb_DD_FPEDOC
  ap825 -->|COPY| cb_DD_TABCVP
  ap825 -->|COPY| cb_DD_PNSCNPVP
  ap825 -->|COPY| cb_DD_PNSCNP
  ap825 -->|COPY| cb_DD_CONVIID
  ap825 -->|COPY| cb_DD_PNSCGA
  ap825 -->|COPY| cb_DD_PARCELACGA
  ap825 -->|COPY| cb_DD_PNSID
  ap825 -->|COPY| cb_DD_STCP
  ap825 -->|COPY| cb_DD_BENGUELA
  ap825 -->|COPY| cb_DD_HISTALT
  ap825 -->|COPY| cb_DD_TABVALFIXO
  ap825 -->|COPY| cb_DD_VAR_ERROS
  ap825 -->|COPY| cb_PD_ERRO_SCRATCH
  ap825 -->|COPY| cb_PD_UTIL
  ap825 -->|COPY| cb_PD_REGIME
  ap825 -->|COPY| cb_PD_VER_OCORR_PERMISSOES
  ap825 -->|COPY| cb_PD_FDPERMISSOES
  ap825 -->|COPY| cb_PD_DATAPROC
  ap825 -->|COPY| cb_PD_DIFDATAS
  ap825 -->|COPY| cb_PD_DESCR_HA
  ap825 -->|COPY| cb_PD_TABERROS
  ap825 -->|COPY| cb_PD_DATA_HORA
  ap825 -->|COPY| cb_PD_INCTIME
  ap825 -->|COPY| cb_PD_ERRO_RDMS_DMS
  ap825 -->|COPY| cb_PD_ERRO_ERACSF_DMS
  ap825 -->|COPY| cb_PD_ERRO_SFS_DMS
  ap825 -->|COPY| cb_API_MSG_ERR
  ap825 -->|COPY| cb_API_ERRO
  ap825 -->|COPY| cb_PD_VALID_DAT
  ap825 -->|COPY| cb_API_MSG_ERR_EPORTAL
  ap825 -->|COPY| cb_API_ERRO_EPORTAL
  ap825 -->|COPY| cb_API_PLATFORM_EXIT
  ap825 -->|COPY| cb_PD_DMSPROTN
  ap825 -->|COPY| cb_PD_DMSPROTN4
  ap825 -->|COPY| cb_PD_DMSPROTN1
  ap825 -->|COPY| cb_PD_LIMPA_VAR_ERROS

  %% Styling
  classDef program fill:#f0f9ff,stroke:#036,stroke-width:2px;
  classDef call fill:#e6f7ff,stroke:#0a6,stroke-width:1px;
  classDef file fill:#fff4e6,stroke:#b35b00,stroke-width:1px;
  classDef sql fill:#fff7f0,stroke:#b33a6f,stroke-width:1px;
  classDef copy fill:#f4f6f8,stroke:#666,stroke-dasharray: 3 3;

  class ap825 program;
  class getdata,eracsf call;
  class fpedoc,rpedoc file;
  class pnscnp,parcelas,vrs037,tabvalfixo,endexec,parcelasvp,tabcvp,pnsid,stcp,benguela,histalt,conviid,vrs017,pnscga,parcelacga,parcelacgavp,tabcregime sql;
  class cb_SELECT_FDPERMISSOES,cb_SELECT_FPEDOC,cb_RPERMISSOES,cb_RPEDOC,cb_TXIDEF_UCOB,cb_MCBDEF_UCOB,cb_DD_DMSPROTN,cb_MSG_ERR,cb_MSG_807,cb_DD_DATA,cb_DD_TABERROS,cb_DD_DATA_HORA,cb_DD_ERRO_RDMS,cb_DD_DESCR_HA,cb_DD_DIFDATAS,cb_DD_DATAPROC,cb_DD_INCTIME,cb_DD_REGIME,cb_DD_FDPERMISSOES,cb_DD_FDPERMISSOES4,cb_DD_FDPERMISSOES1,cb_DD_FILE_STATUS_SFS,cb_DD_TABCUTIL,cb_DD_ERRO_SCRATCH,cb_DD_FPEDOC,cb_DD_TABCVP,cb_DD_PNSCNPVP,cb_DD_PNSCNP,cb_DD_CONVIID,cb_DD_PNSCGA,cb_DD_PARCELACGA,cb_DD_PNSID,cb_DD_STCP,cb_DD_BENGUELA,cb_DD_HISTALT,cb_DD_TABVALFIXO,cb_DD_VAR_ERROS,cb_PD_ERRO_SCRATCH,cb_PD_UTIL,cb_PD_REGIME,cb_PD_VER_OCORR_PERMISSOES,cb_PD_FDPERMISSOES,cb_PD_DATAPROC,cb_PD_DIFDATAS,cb_PD_DESCR_HA,cb_PD_TABERROS,cb_PD_DATA_HORA,cb_PD_INCTIME,cb_PD_ERRO_RDMS_DMS,cb_PD_ERRO_ERACSF_DMS,cb_PD_ERRO_SFS_DMS,cb_API_MSG_ERR,cb_API_ERRO,cb_PD_VALID_DAT,cb_API_MSG_ERR_EPORTAL,cb_API_ERRO_EPORTAL,cb_API_PLATFORM_EXIT,cb_PD_DMSPROTN,cb_PD_DMSPROTN4,cb_PD_DMSPROTN1,cb_PD_LIMPA_VAR_ERROS copy;
```