# Reverse Engineering Details

**Generated**: 2026-03-20 15:53:28
**Total Files Analyzed**: 1 (1 programs, 0 copybooks)
**Total User Stories**: 7
**Total Features**: 7
**Total Business Rules**: 0

---

## Business Logic

## ap825_67.cbl

### Business Purpose
This program authorizes or deletes changes to a pension record (including its instalments/parcelas and selected pension fields), applies validation rules, updates the pension database, writes history records, and optionally generates document requests. It enforces business rules to prevent invalid or inconsistent pension updates.

### Feature Descriptions

#### US-1: Use Case 1: Authorize changes to a pension (Update Pension)

**Trigger:** Incoming authorization message containing new pension data (M807) for a pension number (NPENS) and action not equal to 'E'.
**Description:** Validate incoming pension data against current database values and business rules; if valid, update pension master record(s), parcel/instalment tables, convention/beneficiary links and write a history (audit) record. May also create FPEDOC (document) requests when needed.

**Business Rules:**
- Authenticate/authorize request (check NPENS/session, user type CNP/CRSS and permission table FDPERMISSOES).
- Lock relevant tables and read current pension master (PNSCNP or PNSCGA), parcels and supporting tables (PARCELAS, TABCVP, TABVALFIXO, etc.).
- Validate incoming fields and parcels against numerous business rules (amount caps, parcel combinations, date/age constraints, event/type consistency).
- Identify differences and build a descriptive change (history) entry.
- Update pension master record (PNSCNP or PNSCGA) and insert/delete/replace parcels (PARCELAS, PARCELACGA, PARCELASVP, PARCELACGAVP) as required.
- Insert HISTALT history/audit record describing changes.
- If required, insert FPEDOC request records (document requests).
- Commit; on error roll back and report business error code/message.

*Source: ap825_67.cbl*

#### US-2: Use Case 2: Eliminate pension (Delete Pension)

**Trigger:** Incoming action M807-ACCAO = 'E'.
**Description:** Delete related temporary authorization records (TABCVP) and any provisional parcels (PARCELASVP / PARCELACGAVP) for the indicated pension. Log an outcome code.

**Business Rules:**
- Validate request and lock tables.
- Delete TABCVP rows with matching NPENS/origin/CMODPAG and any PARCELASVP or PARCELACGAVP rows for the NPENS/CMODPAG.
- Return deletion result (error/success code).

*Source: ap825_67.cbl*

#### US-3: Use Case 3: Read current pension and instalment data (Read / Fetch)

**Trigger:** Needed during authorization processing or validation (internal).
**Description:** Read pension master (PNSCNP / PNSCGA), identity (PNSID), existing parcels (PARCELAS, PARCELACGA, PARCELASVP, PARCELACGAVP), TABCVP and other lookup tables (TABVALFIXO, STCP, BENGUELA, CONVIID) to compare with incoming data.

**Business Rules:**
- Open cursor(s) / select from RDMS tables using NPENS and CMODPAG where applicable.
- Fetch up to configured numbers of parcel rows (typically up to 17 or 15).
- Normalize returned NULL indicators into usable values.

*Source: ap825_67.cbl*

#### US-4: Use Case 4: Validate business rules before apply (Validate)

**Trigger:** Before any update/insert/delete operation.
**Description:** Execute comprehensive validation rules for fields, parcel combinations, amounts, dates and dependent records; if any rule fails, stop processing and return a business error code.

**Business Rules:**
- Validate identity/session and input NPENS matches caller session.
- Validate parcel composition, amounts against fixed thresholds (TABVALFIXO), dates, event codes and other domain constraints.
- Check dependent pensions/other pensions for incompatible parcel combinations.
- If validation fails, build error description and abort (rollback).

*Source: ap825_67.cbl*

#### US-5: Use Case 5: Maintain conventions/beneficiary references (Create/Update/Delete CONVIID)

**Trigger:** Changes in convention/beneficiary fields in incoming data or differences detected.
**Description:** Read, insert, update or delete CONVIID table rows when the pension’s convention references change (three possible convention slots). Key Steps: 1. Compare incoming convention fields vs stored values. 2. If change requires deletion and/or re-insertion, delete existing CONVIID rows for NPENS and insert or update new ones. 3. Mark that convention history was changed.

*Source: ap825_67.cbl*

#### US-6: Use Case 6: Audit/history insertion (Create HISTALT)

**Trigger:** Any detected change to fields or parcels that must be recorded.
**Description:** Build a human-readable description of changes and insert a HISTALT record (with timestamp, origin, operator, description).

**Business Rules:**
- Assemble DD-DESCRICAO from change codes/values.
- Insert HISTALT row (timestamped) into the versioned history table.

*Source: ap825_67.cbl*

#### US-7: Use Case 7: Produce FPEDOC document request (Create/Delete)

**Trigger:** Business rules indicate a document request should be created or removed (e.g., special cases, certain pension change types or when existing FPEDOC 15/16 found).
**Description:** Write or delete FPEDOC records (external file) for subsequent manual processing.

**Business Rules:**
- Open FPEDOC file (SFS).
- Search for existing requests of types 15/16 and delete if present, or create a new request record (type 8, 15 or 16 depending on conditions).
- Close FPEDOC.
- NPENS (M807-NPENS) must match session/stored pension reference (SCR-NPENS) and SCR-NEMP must be non-zero - Error: 695 (authorization/pension mismatch).
- Database read operations (PNSCNP/PNSCGA/TABCVP) must return rows; missing required rows produce specific errors:
- No TABCVP for expected origin -> Error: 657 (missing TABCVP).
- No PNSCNP -> Error: 5 (pension master missing).
- No PNSID -> Error: 242 (identity record missing).
- No PNSCGA when expected -> Error: 6 (CGA master missing).
- Parcel arrays: maximum occurrences read/handled are limited (17 for most parcel sets, 15 for producer parcels). If incoming list lengths exceed capacity, business logic treats only up to limits.
- Date fields (DTINI, DTINIOR, DTINIOP, DT1DES, DTUDES, DTVELHI, DTTERM, DTFAL) must be valid and consistent with format YYYYMMDD or YYYYMM for comparisons; empty/null are normalized.
- Numeric fields (amounts VPAR, VOUTR, VOUTP, V13MES, VREMREF) must be numeric; NULL flags are translated to zeroes.
- For social pension (PENSAO-S) specific fields:
- If CPESP indicates certain categories (C, E, G, H, J) then S-ANMESES must be > 0, else error 130; conversely if CPESP not in those and S-ANMESES > 0 -> error 526.
- When CSP = 'N' and source is not a valid basic record, a derived CSP value may be computed — but PNSCGA/PNSCNP records must exist to allow CSP derivation; otherwise errors 386/230 apply.
- Access and permission:
- The program checks FDPERMISSOES for user roles (CNP or CRSS). Certain rules/limits depend on AGU-UTILIZADOR (CNP vs CRSS) and on SCR-UTIL region (special allowances for ACORES/MADEIRA).
- Parcel composition and incompatibilities:
- Certain parcel type codes (CPAR) are tracked (examples: '01','10','22','42','2I','4I','33','37','28','12','34','29','82','65','66','49','72','ES','39','MM','59','67','87','2I','4I'). Business logic sets flags for presence and amounts per CPAR code.
- Some parcel types cannot co-exist or create conflicts. For instance:
- If either the incoming pension or any existing related pension has parcel codes in the set {'22','2I','42','4I'} while the other has other conflicting parcels, the combination is forbidden -> Error: 950 (dependency/conflict).
- Presence of parcel '10' on both sides can produce conflict - Error: 950.
- Value caps and thresholds (TABVALFIXO lookups):
- Total of selected parcel values (sum of W-VPAR for included CPARs plus optionally VOUTP) must not exceed a threshold value (W-VALOR-TVF from TABVALFIXO for code '15'). If total > threshold -> Error: 84.
- Individual parcel types have specific caps depending on user and parcel code:
- CPAR '37','12','28' checks against TVF (code 'PS'): if W-VPAR for those > W-VALOR-TVF -> Error: 83.
- CPAR '33' allowed up to 60% of TVF for typical cases -> If W-VPAR-33 > 60% of TVF -> Error: 79.
- Special case: When CPAR '39' ORFANDADE and NPENS in an allowed list, different percentage applies (80% vs 40%); otherwise 60% used.
- CPAR '34' limited to 60% of TVF -> Error: 80.
- CPAR '22' and '2I' limited by TVF with code 'SG' -> Error: 81.
- CPAR '42' and '4I' limited by TVF with code 'CD' -> Error: 81.
- CPAR '29' and '82' with code 'PR' -> Error: 82.
- Parcel type 'ES' (special) must equal TABVALFIXO value for code 'CE' or 'CS' depending on computed age threshold -> Error: 600.
- For CRSS users or specific regional users, some caps use different TVF codes (e.g., PS vs 15 vs SG vs CD) and additional constraints (e.g., CNT-PARCELAS-ECR counts).
- Parcel conversion and automatic mapping:
- Under several conditions the program may attempt to convert a parcel type when threshold rules permit (e.g., convert '12'->'37' or '28'->'72') only if computed sums do not exceed TVF; if not possible -> Error: 554.
- Date/age constraints:
- If pension record’s saved recording date (W-DTGRV) is before specific cut-offs, certain CPESP values are not allowed -> Error: 861 (or variations).
- For some parcel types ('37' or '72') there is a minimum birth date requirement for the beneficiary (M807-DTNAS must be >= 19530101) -> Error: 68 (age/date restriction).
- If incoming DTVELHI (some “old age” date) is non-zero and not greater than processing date year-month -> Error: 133.
- Additional historical cut-offs exist for event/regime combinations (error 1009 used for policy dates).
- Event vs special codes consistency:
- Certain CSESP codes (like 'D','12','13','15','20','25','29','30','31','32','37') require CEVENTO = '2'; otherwise -> Error: 106 (invalid CEVENTO for CSESP).
- CSESP values '43' or '44' must be consistent between incoming and DB (mismatch -> Error: 962).
- Suspensions and employer restrictions:
- If a pension is suspended (CSUSP > 0) then many changes are blocked unless the operator’s employer is in a whitelist; if blocked -> Error: 621.
- Specific changes flagged (event, convention, special codes, parcel changes, start-date changes) are not allowed when CSUSP prevents changes except for certain social-investment exceptions.
- Social pension (PENSAO-S) rules:
- Pension social parcel additions are controlled: the number of new social parcel types cannot exceed existing allowed count -> Error: 043.
- If S-CSP derivation required and missing producer data, returns errors 135 or 230.
- For PENSAO-S, some numeric programme fields (PROGCC) are validated and may be required or prohibited depending on CPESP values.
- Cross-pension dependency rules:
- When there are other pensions for the same person (PNSCNP/PNSCGA queries), combinations of parcel types across pensions can create blocking conditions (e.g., orphanage pensions and parent relationship checks) -> errors 951/979 etc for inconsistent parent/pension type combinations.
- Parcel existence and counts:
- The number of parcels in the incoming message must match the stored parcel count unless an intentional replacement is being done; mismatch may cause elimination/insertion processing or error (CNT comparisons).
- Database concurrency:
- Database deadlock on DML operations is treated as specific error 506 and returned as an application error.
- Final result codes:
- On completion the program sets a result code depending on action:
- Deletion success -> WS-TERRCOD = 351
- Authorization success -> WS-TERRCOD = 336 (or other codes for special FPEDOC handling, e.g., 614/940/411 depending on FPEDOC conditions)
- : NPENS/session mismatch or missing session data (authorization invalid).
- : TABCVP not found for expected origins (missing authorization context).
- : Employer number in TABCVP equals caller employer — unauthorized attempt (security mismatch).
- : Invalid pension type (CPESP) for older records (pre-2007 rule).
- : DTVELHI date invalid relative to current processing date (old-age date constraint).
- : Total of selected parcel values exceeds allowed fixed threshold (TABVALFIXO).
- : Individual parcel exceeds allowed threshold (TABVALFIXO).
- : CPAR-33 exceeds allowed percentage threshold (60% default, 40/80% special cases).
- : CPAR-34 exceeds allowed threshold (60% of TVF).
- : CPAR-22 / 2I / 42 / 4I exceeds allowed threshold (TABVALFIXO 'SG'/'CD').
- : CPAR-29 / 82 exceeds allowed threshold (TABVALFIXO 'PR').
- : CPAR 'ES' value mismatch with TVF-derived expected value.
- : Exact equality to TVF forbidden case (specific to some CPAR+ES combinations).
- : Forbidden combination of parcel types and counts (e.g., certain combos not allowed).
- : Insufficient sum against threshold when VOUTP = 0 (e.g., minimum sum rule).
- : Unable to convert parcels (12->37 or 28->72) under rules (conversion not allowed).
- : Conflicting parcel dependency across pensions (e.g., presence of 22/42/2I/4I conflicting with other parcel flags).
- x, 106: CSESP requires CEVENTO=2 -> Error 106 for mismatch.
- : Missing PNSID (identity) record.
- : Missing PNSCNP record (pension master).
- / 230: Missing producer/producer identity for social pension CSP derivation.
- : Social pension parcel phase constraints violated (too many new social parcels) .
- : Changes not permitted due to CSUSP and employer not whitelisted.
- : Parcel amount changes not permitted due to CSUSP.
- : Database deadlock encountered (DMS/RDMS deadlock).
- : Final code when deletion action executed (delete outcome).
- / 614 / 940 / 411: Final codes for various successful authorization outcomes depending on additional FPEDOC handling or data conditions.
- : Regime/event/date combination not allowed per regulatory cut-off.
- / 960: Parcel codes not permitted for changes with start date constraints (e.g., start-date >= 2014 disallowed for certain parcel types).
- : CSESP=43/44 mismatch between incoming and DB.
- : Other pensions exist that block an operation (e.g., orphanage pension conflict).
- : CE special code 'CE' inconsistency.
- : Specific TVF threshold violation for certain cases (e.g., CRSS flows).
- : Number-of-names change note (historical logging code).
- /183/184 etc: Codes used when writing descriptive history lines for parcel details.
- User types and origin of request (AGU-UTILIZADOR = 'CNP ' or 'CRSS') change which validation thresholds apply and whether some fields are allowed to change (regional exceptions for ACORES/MADEIRA are present).
- The program enforces strict parity between incoming parcel definitions and stored parcel sets; if count/values differ it will delete and re-insert parcels to match the incoming profile after validation.
- Many business decisions depend on lookup values held in TABVALFIXO (value caps), TABCVP (current temporary authorization data), and other auxiliary tables; these lookups determine allowed amounts, percentages or allowed codes.
- All database changes are performed within transactional semantics; any validation failure or DB error triggers a rollback.
- All changes that are applied are logged in an audit/history table (HISTALT) with a textual description assembled during processing.
- produce a tabular mapping of CPAR codes → business meaning / rules inferred from the code,
- extract a consolidated list of all error codes with the exact code locations and contexts in the source,
- or convert the validations into a concise executable checklist for a QA team. Which would help you most?

*Source: ap825_67.cbl*

### Features

#### F-1: Authorize changes to a pension (Update Pension)

*Source: ap825_67.cbl*

#### F-2: Eliminate pension (Delete Pension)

*Source: ap825_67.cbl*

#### F-3: Read current pension and instalment data (Read / Fetch)

*Source: ap825_67.cbl*

#### F-4: Validate business rules before apply (Validate)

*Source: ap825_67.cbl*

#### F-5: Maintain conventions/beneficiary references (Create/Update/Delete CONVIID)

*Source: ap825_67.cbl*

#### F-6: Audit/history insertion (Create HISTALT)

*Source: ap825_67.cbl*

#### F-7: Produce FPEDOC document request (Create/Delete)

**Processing Steps:**
1. NPENS (M807-NPENS) must match session/stored pension reference (SCR-NPENS) and SCR-NEMP must be non-zero - Error: 695 (authorization/pension mismatch).
2. Database read operations (PNSCNP/PNSCGA/TABCVP) must return rows; missing required rows produce specific errors:
3. No TABCVP for expected origin -> Error: 657 (missing TABCVP).
4. No PNSCNP -> Error: 5 (pension master missing).
5. No PNSID -> Error: 242 (identity record missing).
6. No PNSCGA when expected -> Error: 6 (CGA master missing).
7. Parcel arrays: maximum occurrences read/handled are limited (17 for most parcel sets, 15 for producer parcels). If incoming list lengths exceed capacity, business logic treats only up to limits.
8. Date fields (DTINI, DTINIOR, DTINIOP, DT1DES, DTUDES, DTVELHI, DTTERM, DTFAL) must be valid and consistent with format YYYYMMDD or YYYYMM for comparisons; empty/null are normalized.
9. Numeric fields (amounts VPAR, VOUTR, VOUTP, V13MES, VREMREF) must be numeric; NULL flags are translated to zeroes.
10. For social pension (PENSAO-S) specific fields:
11. If CPESP indicates certain categories (C, E, G, H, J) then S-ANMESES must be > 0, else error 130; conversely if CPESP not in those and S-ANMESES > 0 -> error 526.
12. When CSP = 'N' and source is not a valid basic record, a derived CSP value may be computed — but PNSCGA/PNSCNP records must exist to allow CSP derivation; otherwise errors 386/230 apply.
13. Access and permission:
14. The program checks FDPERMISSOES for user roles (CNP or CRSS). Certain rules/limits depend on AGU-UTILIZADOR (CNP vs CRSS) and on SCR-UTIL region (special allowances for ACORES/MADEIRA).
15. Parcel composition and incompatibilities:
16. Certain parcel type codes (CPAR) are tracked (examples: '01','10','22','42','2I','4I','33','37','28','12','34','29','82','65','66','49','72','ES','39','MM','59','67','87','2I','4I'). Business logic sets flags for presence and amounts per CPAR code.
17. Some parcel types cannot co-exist or create conflicts. For instance:
18. If either the incoming pension or any existing related pension has parcel codes in the set {'22','2I','42','4I'} while the other has other conflicting parcels, the combination is forbidden -> Error: 950 (dependency/conflict).
19. Presence of parcel '10' on both sides can produce conflict - Error: 950.
20. Value caps and thresholds (TABVALFIXO lookups):
21. Total of selected parcel values (sum of W-VPAR for included CPARs plus optionally VOUTP) must not exceed a threshold value (W-VALOR-TVF from TABVALFIXO for code '15'). If total > threshold -> Error: 84.
22. Individual parcel types have specific caps depending on user and parcel code:
23. CPAR '37','12','28' checks against TVF (code 'PS'): if W-VPAR for those > W-VALOR-TVF -> Error: 83.
24. CPAR '33' allowed up to 60% of TVF for typical cases -> If W-VPAR-33 > 60% of TVF -> Error: 79.
25. Special case: When CPAR '39' ORFANDADE and NPENS in an allowed list, different percentage applies (80% vs 40%); otherwise 60% used.
26. CPAR '34' limited to 60% of TVF -> Error: 80.
27. CPAR '22' and '2I' limited by TVF with code 'SG' -> Error: 81.
28. CPAR '42' and '4I' limited by TVF with code 'CD' -> Error: 81.
29. CPAR '29' and '82' with code 'PR' -> Error: 82.
30. Parcel type 'ES' (special) must equal TABVALFIXO value for code 'CE' or 'CS' depending on computed age threshold -> Error: 600.
31. For CRSS users or specific regional users, some caps use different TVF codes (e.g., PS vs 15 vs SG vs CD) and additional constraints (e.g., CNT-PARCELAS-ECR counts).
32. Parcel conversion and automatic mapping:
33. Under several conditions the program may attempt to convert a parcel type when threshold rules permit (e.g., convert '12'->'37' or '28'->'72') only if computed sums do not exceed TVF; if not possible -> Error: 554.
34. Date/age constraints:
35. If pension record’s saved recording date (W-DTGRV) is before specific cut-offs, certain CPESP values are not allowed -> Error: 861 (or variations).
36. For some parcel types ('37' or '72') there is a minimum birth date requirement for the beneficiary (M807-DTNAS must be >= 19530101) -> Error: 68 (age/date restriction).
37. If incoming DTVELHI (some “old age” date) is non-zero and not greater than processing date year-month -> Error: 133.
38. Additional historical cut-offs exist for event/regime combinations (error 1009 used for policy dates).
39. Event vs special codes consistency:
40. Certain CSESP codes (like 'D','12','13','15','20','25','29','30','31','32','37') require CEVENTO = '2'; otherwise -> Error: 106 (invalid CEVENTO for CSESP).
41. CSESP values '43' or '44' must be consistent between incoming and DB (mismatch -> Error: 962).
42. Suspensions and employer restrictions:
43. If a pension is suspended (CSUSP > 0) then many changes are blocked unless the operator’s employer is in a whitelist; if blocked -> Error: 621.
44. Specific changes flagged (event, convention, special codes, parcel changes, start-date changes) are not allowed when CSUSP prevents changes except for certain social-investment exceptions.
45. Social pension (PENSAO-S) rules:
46. Pension social parcel additions are controlled: the number of new social parcel types cannot exceed existing allowed count -> Error: 043.
47. If S-CSP derivation required and missing producer data, returns errors 135 or 230.
48. For PENSAO-S, some numeric programme fields (PROGCC) are validated and may be required or prohibited depending on CPESP values.
49. Cross-pension dependency rules:
50. When there are other pensions for the same person (PNSCNP/PNSCGA queries), combinations of parcel types across pensions can create blocking conditions (e.g., orphanage pensions and parent relationship checks) -> errors 951/979 etc for inconsistent parent/pension type combinations.
51. Parcel existence and counts:
52. The number of parcels in the incoming message must match the stored parcel count unless an intentional replacement is being done; mismatch may cause elimination/insertion processing or error (CNT comparisons).
53. Database concurrency:
54. Database deadlock on DML operations is treated as specific error 506 and returned as an application error.
55. Final result codes:
56. On completion the program sets a result code depending on action:
57. Deletion success -> WS-TERRCOD = 351
58. Authorization success -> WS-TERRCOD = 336 (or other codes for special FPEDOC handling, e.g., 614/940/411 depending on FPEDOC conditions)
59. 695: NPENS/session mismatch or missing session data (authorization invalid).
60. 657: TABCVP not found for expected origins (missing authorization context).
61. 679: Employer number in TABCVP equals caller employer — unauthorized attempt (security mismatch).
62. 861: Invalid pension type (CPESP) for older records (pre-2007 rule).
63. 133: DTVELHI date invalid relative to current processing date (old-age date constraint).
64. 84: Total of selected parcel values exceeds allowed fixed threshold (TABVALFIXO).
65. 83: Individual parcel exceeds allowed threshold (TABVALFIXO).
66. 79: CPAR-33 exceeds allowed percentage threshold (60% default, 40/80% special cases).
67. 80: CPAR-34 exceeds allowed threshold (60% of TVF).
68. 81: CPAR-22 / 2I / 42 / 4I exceeds allowed threshold (TABVALFIXO 'SG'/'CD').
69. 82: CPAR-29 / 82 exceeds allowed threshold (TABVALFIXO 'PR').
70. 600: CPAR 'ES' value mismatch with TVF-derived expected value.
71. 598: Exact equality to TVF forbidden case (specific to some CPAR+ES combinations).
72. 599: Forbidden combination of parcel types and counts (e.g., certain combos not allowed).
73. 372: Insufficient sum against threshold when VOUTP = 0 (e.g., minimum sum rule).
74. 554: Unable to convert parcels (12->37 or 28->72) under rules (conversion not allowed).
75. 950: Conflicting parcel dependency across pensions (e.g., presence of 22/42/2I/4I conflicting with other parcel flags).
76. 10x, 106: CSESP requires CEVENTO=2 -> Error 106 for mismatch.
77. 242: Missing PNSID (identity) record.
78. 5: Missing PNSCNP record (pension master).
79. 135 / 230: Missing producer/producer identity for social pension CSP derivation.
80. 043: Social pension parcel phase constraints violated (too many new social parcels) .
81. 621: Changes not permitted due to CSUSP and employer not whitelisted.
82. 620: Parcel amount changes not permitted due to CSUSP.
83. 506: Database deadlock encountered (DMS/RDMS deadlock).
84. 351: Final code when deletion action executed (delete outcome).
85. 336 / 614 / 940 / 411: Final codes for various successful authorization outcomes depending on additional FPEDOC handling or data conditions.
86. 1009: Regime/event/date combination not allowed per regulatory cut-off.
87. 961 / 960: Parcel codes not permitted for changes with start date constraints (e.g., start-date >= 2014 disallowed for certain parcel types).
88. 962: CSESP=43/44 mismatch between incoming and DB.
89. 979: Other pensions exist that block an operation (e.g., orphanage pension conflict).
90. 833: CE special code 'CE' inconsistency.
91. 98: Specific TVF threshold violation for certain cases (e.g., CRSS flows).
92. 69: Number-of-names change note (historical logging code).
93. 182/183/184 etc: Codes used when writing descriptive history lines for parcel details.
94. User types and origin of request (AGU-UTILIZADOR = 'CNP ' or 'CRSS') change which validation thresholds apply and whether some fields are allowed to change (regional exceptions for ACORES/MADEIRA are present).
95. The program enforces strict parity between incoming parcel definitions and stored parcel sets; if count/values differ it will delete and re-insert parcels to match the incoming profile after validation.
96. Many business decisions depend on lookup values held in TABVALFIXO (value caps), TABCVP (current temporary authorization data), and other auxiliary tables; these lookups determine allowed amounts, percentages or allowed codes.
97. All database changes are performed within transactional semantics; any validation failure or DB error triggers a rollback.
98. All changes that are applied are logged in an audit/history table (HISTALT) with a textual description assembled during processing.
99. produce a tabular mapping of CPAR codes → business meaning / rules inferred from the code,
100. extract a consolidated list of all error codes with the exact code locations and contexts in the source,
101. or convert the validations into a concise executable checklist for a QA team. Which would help you most?

*Source: ap825_67.cbl*

---

---

## Technical Analysis

### ap825_67.cbl

**Program Description:** Extracted from AI analysis

Summary (program AP825)
- Program-ID: AP825
- Purpose: Authorization/elimination of pension (pensao) changes — handles validation, comparison and update/delete of pension records and related parcels, writes history records, and optionally writes requests to an FPEDOC file. Interacts extensively with RDMS tables via embedded SQL (DB2-style).
- Language: COBOL with embedded SQL (EXEC SQL ... END-EXEC) and system calls (ERACSF, MCB$ENT), uses many COPY copybooks.

1) Overall program description
- AP825 processes an incoming message (M807 structure from an external interface) to authorize or eliminate modifications to pension records. It:
  - Initializes environment, opens RDMS threads and DMS if needed.
  - Determines whether the record is for CNP (national) or CGA (contributory) pension (based on M807-CMODPAG-CGA).
  - Reads existing pension data from RDMS tables (PNSCNP, PNSCGA, PARCELAS, TABCVP, etc.).
  - Validates incoming changes against business rules (value thresholds, date checks, parcel compatibility, special-case logic).
  - If differences exist, it composes descriptive messages (DD-DESCRICAO) and inserts HISTALT (history) rows.
  - Updates PNSCNP/PNSCGA/other RDMS tables (UPDATE, INSERT, DELETE) or deletes TABCVP/PARCELASVP where needed.
  - Optionally writes a request record to FPEDOC (file) depending on conditions.
  - Handles and logs application and DB errors, including rollback, error decoding and notification to an error mechanism (MERR structures).
  - Supports both authorization and elimination flows for pensions and has separate sections for CNP and CGA processing.

2) Data divisions and their purpose
- SUBSCHEMA SECTION
  - Contains INVOKE SUBSCHEMA statements (TESTAR/FORLAR/REALAR variants) to declare sub-schemas for RDMS access (not active code but annotation/config).
- FILE SECTION
  - Uses COPY RPERMISSOES and COPY RPEDOC (and copies declared at top of program) — file definitions (FPEDOC etc.) are in copybooks.
- WORKING-STORAGE SECTION
  - Many COPY statements (TXIDEF-UCOB, MCBDEF-UCOB, DD-*, MSG-*, PD-* etc.) bring in shared data definitions (error handling, date/time, DMS/RDMS utility fields).
  - Explicit local variables and groups declared within the visible WS section:
    - Single-level items: AGU-UTILIZADOR (X(04)), many 01-level numeric counters and flags (IND-PAR, IND-ECR, CNT-PARCELAS, AGU-FIC, AGU-DIF etc.).
    - 77-levels: AGU-NCGA-ALT etc. for flags.
    - Redefinitions: W-REG-VAL and its REDEFINES W-RVALINT/W-RVALDEC; W-NPENS-PROD group and redefines; W-VALOR redefines W-VALOR1/2 etc.
    - OCCURS arrays: W-TAB-PARCELAS / W-TABELA-PARCELAS / W-TAB-PARCELAS-PRODUTOR (occurs 17/15) with elements: W-DTINI, W-CPAR, W-VPAR .
    - Date/time groups: W-DATA21, W-DATA10, W-DATA8, W-DATA6, W-DT-PROCESS, DT-DATA21 etc. with components for year/month/day/time parts and redefines to numeric forms.
- COPYBOOKS are used heavily for file descriptions, standard structures, status variables and many RDMS variable definitions (DD-TABCVP, DD-PNSCNPVP, DD-PNSCNP etc.). These supply most table-related working-storage fields (W- prefixed fields used with SQL).

3) Procedure divisions and their purpose
Top-level sections/paragraphs (main flow):
- INICIAL: startup — initializes communication with EPORTAL (MCB$ENT), clears RDMS error variables, begins thread, obtains schema, sets DMS flags and calls SRDP..SRDPX (external).
- PRINCIPAL: route to PROGRAMA-CNP or PROGRAMA-CGA depending on M807-CMODPAG-CGA.
- PROGRAMA-CNP SECTION:
  - INICIO: Entry for CNP flow — opens permission file, loads parcel arrays, locks tables, accesses TABCVP and decides pension type (IV vs S), then branches.
  - TRATA-PENSAO-IV / TRATA-PENSAO-S: Handle validation rules for pension IV (invalidity?) and S (social?), many checks on parcels and fields; call TRATAMENTO when validations completed.
  - TRATAMENTO: Main processing for CNP — accesses PNSCNP, PNSID, parcel tables, perform comparisons, call ACTUALIZAR-PNSCNP, manage HISTALT, FPEDOC and deletes/updates.
  - Many subroutines to access RDMS: ACEDER-TABCVP, ACEDER-PARCELASVP, ACEDER-PARCELAS, ACEDER-PNSCNP, ACEDER-PNSID, DECLARA-TABVALFIXO, ACEDER-TABVALFIXO, ACEDER-PNSCNP-PRODUTOR etc.
  - Special validators: VERIFICA-PARCELAS-ECRAN, VAL-COMPL-DEPENDENCIA-INV/SOB, PROCURA-OUTRAS-PNSCNP.
  - File operations and history: ESCREVE-DESCRICAO-E-TESTA, INSERIR-HISTALT, TRATA-FPEDOC, TRATA-FPEDOC-PEDIDO-8.
  - Error handling: ERRO-APLICACAO (does ROLLBACK, decode error, send message), ERRO-BD (DB error path).
  - ELIMINAR-TABCVP, ELIMINAR-PARCELASVP: delete operations.
- PROGRAMA-CGA SECTION:
  - MAIN (starts CGA flow) -> TRATAMENTO -> TRATA-ELIMINACAO or TRATA-AUTORIZACAO
  - TRATA-AUTORIZACAO: similar to CNP but different table names (PNSCGA, PARCELACGA), includes parcel handling (TRATA-PARCELAS), ACEDER-PARCELACGAVP, ACEDER-PARCELACGA-PROD, updates PNSCGA, inserts HISTALT, etc.
  - TRATA-ELIMINACAO: deletes TABCVP and PARCELACGAVP.
  - Many subroutines mirror CNP ones but with CGA-specific variables/tables.
- ACESSO-FCPROTN SECTION: entry for access to prot. files; copies PD-DMSPROTN/variants.
- LIMPA-VAR-ERROS SECTION: uses COPY PD-LIMPA-VAR-ERROS to clear error vars.

4) Variables (selected, main ones visible in source; many more come from COPYs)
Note: many RDMS-related working-storage fields are defined via COPY DD-* copybooks. Below are variables explicitly declared in shown WS and representative RDMS fields used by SQL INTO clauses.

- AGU-UTILIZADOR
  - level: 01
  - type: PIC X(04)
  - purpose: user identifier; 88-levels: AGU-UTILIZADOR-CNP ('CNP '), AGU-UTILIZADOR-CRSS ('CRSS').

- IND-PAR
  - level: 01
  - type: PIC 9(02)
  - initial: 1
  - purpose: loop index over parcels (1..17)

- IND-ECR, IND-PER, CNT-PARCELAS-ECR, CNT-PARCELAS
  - types: numeric PIC 9(n)
  - used as counters and indices

- AGU-FIC, AGU-FPEDOC, AGU-TRATA-FPEDOC
  - flags: PIC 9(01)

- AGU-DIF
  - PIC 9(01) with 88-values mapping to different difference codes (AGU-HA-DIF=1, AGU-EVENTO-DIF=2,...)

- W-CPAR (array)
  - group W-TABELA-PARCELAS or arrays declared:
    - 01 W-TABELA-PARCELAS.
      - 02 W-TAB-PARCELAS OCCURS 17 TIMES.
        - 03 W-DTINI PIC X(10)
        - 03 W-CPAR PIC X(02)
        - 03 W-VPAR PIC 9(05)V9(02)
  - Purpose: hold read parcel data for comparisons/insert.

- W-VPAR (array)
  - PIC 9(05)V9(02) per occurs element (see above).

- W-REG-VAL
  - 01 W-REG-VAL PIC 9(09)V99
  - redefines: W-REG-VAL-RED with 03 W-RVALINT PIC 9(09), 03 W-RVALDEC PIC 9(02)
  - Purpose: used to format numeric values for DD-VARIAVEL (history).

- W-NPENS groups
  - Several NPENS fields for pension numbers:
    - W-NPENS (02 subfields W-NPENS-N, W-NPENS-S and redefines)
    - W-NPENS-RED etc. used to pass to DD-NBEN-LE when performing SQL.

- Date groups:
  - W-DATA21, W-DATA10, W-DATA8, W-DT-PROCESS and corresponding components (ANO/MES/DIA/HOR/MIN/SEG). Reused to build CAST strings for SQL date casts.

- CAST strings (text):
  - W-DATA10-CAST, W-DATAHORA-CAST, W-DTINIOP-CAST etc used to build SQL CAST parameter strings.

- Arrays for RDMS table rows:
  - Many W- prefixed variables used in SQL INTO and as update/insert source: W-NID-CNP, W-CCAIXA-CNP, W-CCONVI-CNP, W-V13MES-CNP etc. Most are defined via copybooks (COPY DD-PNSCNP and similar).

- Flags & counters introduced in patches (VRS*):
  - FLAG-PARVP-22-OK, FLAG-PARVP-42-OK, FLAG-PARVP-2I-OK, etc.

- W- fields used for update (UPD suffix):
  - W-CCAIXA-UPD, W-CEVENTO-UPD, W-CSESP-UPD, W-VOUTR-UPD, W-DTINIOR-UPD, many with corresponding NULO fields (S9(01)) used when building UPDATE columns (DB2 style with nullable indicators).

- DD-* and MERR/WS-TERR* variables
  - Brought via copybooks: DD-COMANDO-LE, DD-TABELA-LE, DD-NBEN-LE, DD-ERRO-FICHEIRO, DD-ERRO-FILE-STATUS, MERR-*, WS-TERRCOD, WS-TERRDESCR etc used for error handling and auditing.

Note: This program relies on many COPYs that define hundreds of fields (DD-TABCVP, DD-PNSCNP, DD-PARCELACGA, etc.). A conversion tool should expand these COPYs to get exact full variable map.

5) Paragraphs/sections (name, description, main logic, variables used, paragraphs called)
I list major paragraphs/sections and key subroutines called or used variables. (Many subroutines called repeated times; I include the principal ones.)

- INICIAL (Procedure Division, PROGRAMA SECTION)
  - Description: startup initialization and thread/DMS setup; calls to external MCB$ENT and SRDP..SRDPX.
  - Logic:
    - Initialize MCB packet, call MCB$ENT to fetch message (MSG-807).
    - BEGIN THREAD via EXEC SQL BEGIN THREAD and set FLAG-THREAD.
    - Configure schema (USE DEFAULT SCHEMA #RDMSCH# via EXEC SQL).
    - Move '#DMSKEY#' to AC-KEY and set FLAG-DMS etc; reset errors and call SRDP..SRDPX.
  - Variables used: P-MCB-PACKET, MSG-807, P-STATBIT, FLAG-THREAD, AC-KEY, FLAG-DMS, ERRO-PROT, RPID-PROT, IGRV-PROT.
  - Calls: LIMPA-VAR-ERROS-RDMS, SRDP THRU SRDPX, PERFORM to PRINCIPAL.

- PRINCIPAL
  - Description: dispatch to CNP or CGA processing depending on M807-CMODPAG-CGA.
  - Calls: PROGRAMA-CNP or PROGRAMA-CGA.

- PROGRAMA-CNP SECTION -> INICIO
  - Description: prepare for processing CNP pensions, open permission files, prepare W-TAB-PARCELAS, lock tables, access TABCVP, declare cursor for PNSCNP, branch to processing per pension type.
  - Key variables: M807-NPENS, SCR-NPENS, DD-AGU-PERMISSOES, CTRANS, AGU-UTILIZADOR, W-DT-PROCESS, W-DATA21, W-ORIGEM-TCVP, W-NPENS-RED, W-NEMP-TCVP, W-DTGRV-N8, PENSAO-IV/ PENSAO-S flags (AGU-TIPO-PENSAO)
  - Calls: LIMPA-VAR-ERROS-ERACSF, ABRE-FDPERMISSOES, LER-FDPERMISSOES, FECHA-FDPERMISSOES, LOCK-TABELAS, ACEDER-TABCVP, then GO TRATA-PENSAO-IV or TRATA-PENSAO-S depending on type.

- ACEDER-TABCVP
  - Type: SQL SELECT INTO
  - Purpose: read TABCVP row for pension to determine NEMP_TCVP, DESCR_TCVP, DTGRV_TCVP. Sets W-DTGRV-X10 and W-DTGRV-N8 for date comparisons later.
  - Variables: W-NPENS-RED, W-NEMP-TCVP, W-DESCR-TCVP, W-DTGRV-TCVP, W-ORIGEM-TCVP
  - Called by: PROGRAMA-CNP.INICIO, PROGRAMA-CGA.TRATAMENTO etc.

- TRATA-PENSAO-IV / TRATA-PENSAO-S
  - Description: Validate incoming (IV) or social (S) pension modifications against existing data and business rules.
  - Logic: validations include:
    - Action 'E' (elimination) triggers ELIMINAR-TABCVP and ELIMINAR-PARCELASVP and exit.
    - Cross-checks NEMP, DTGRV thresholds, CPESP business rules, value thresholds using TABVALFIXO, parcel rules (presence of 22/42/2I/4I/10/37/72), special treatment for orphans codes.
    - Use ACEDER-PARCELASVP to fetch incoming parcel set, VERIFICA-PARCELAS-ECRAN to set AGU-CPAR-* flags and sums, ACEDER-TABVALFIXO to load value thresholds.
    - If validations passed, GO TRATAMENTO.
  - Key variables: IV-*/S-* fields (from input message), W-CPAR( ), W-VPAR( ), W-VALOR-TVF, W-VALOR, W-COD-TVF, flags AGU-CPAR-xx, CNT-PARCELAS-ECR, AGU-UTILIZADOR, SCR-FLAG-CSUSP, etc.
  - Calls: ELIMINAR-TABCVP, ELIMINAR-PARCELASVP, ACEDER-PARCELASVP, VERIFICA-PARCELAS-ECRAN, ACEDER-TABVALFIXO, PROCURA-OUTRAS-PNSCNP, TRATA-FPEDOC, TRATAMENTO.

- VERIFICA-PARCELAS-ECRAN
  - Purpose: Parse W-CPAR/W-VPAR arrays loaded from PARCELASVP (incoming), set AGU-CPAR-xx flags and copy values into W-VPAR-xx variables for quick reference. Counts CNT-PARCELAS-ECR (how many incoming parcels).
  - Variables used: W-CPAR(IND-PAR), W-VPAR(IND-PAR), AGU-CPAR-xx, FLAG-PARVP-xx, CNT-PARCELAS-ECR.

- TRATAMENTO (CNP flow)
  - Description: Main processing after validation for CNP pensions. Reads existing PNSCNP, PNSID, compares incoming vs existing fields, applies patch logic (e.g. special parcel substitution 12->37, 28->72), updates/ inserts/ deletes parcels and TABCVP and writes HISTALT entries if changes.
  - Key actions:
    - ACEDER-PNSCNP (SELECT INTO PNSCNP fields)
    - ACEDER-PNSID
    - Set W-CPAREN-UPD/W-*UPD variables, check differences and set DD-CODIGO/DD-VARIAVEL to build DD-DESCRICAO (history)
    - ACTUALIZAR-PNSCNP: UPDATE PNSCNP with W-*-UPD values and null indicators
    - Manage parcel deletion/insert: ELIMINAR-PARCELAS, INSERIR-PARCELAS
    - Insert HISTALT via INSERIR-HISTALT
    - Handle FPEDOC via TRATA-FPEDOC and TRATA-FPEDOC-PEDIDO-8 for special requests
  - Variables used: many W- and IV-/S- fields; DD-DESCRICAO, DD-CODIGO, DD-VARIAVEL, M807 fields.

- ACEDER-PNSCNP
  - Type: SELECT INTO
  - Purpose: Load existing CNP pension row (a large list of fields) into W- prefixed variables used for comparisons and updates.
  - Variables used: NPENS_CNP (input), many output fields such as W-NID-CNP, W-CCAIXA-CNP, W-CCONVI-CNP, W-V13MES-CNP, W-... (see SQL SELECT list). Null-indicator variables (NULO) used to detect SQL NULLS and to convert into default values.

- ACEDER-PARCELAS / ACEDER-PARCELASVP / ACEDER-PARCELAS-PROD / ACEDER-PARCELACGAVP / ACEDER-PARCELACGA-PROD
  - Purpose: Declare cursor, OPEN, FETCH bulk rows (FETCH NEXT N) into arrays, CLOSE. Read parcel definitions for both existing DB rows and incoming proposed modifications.
  - Variables: M807-NPENS / W-NPENS-RED, W-DTINI, W-CPAR, W-VPAR arrays, CNT-PARCELAS, AUX-INFO (rowcount).

- VAL-COMPL-DEPENDENCIA-INV and -SOB
  - Purpose: When cursor is declared (PNSCNP_CUR for retention), FETCH successive dependent pensions to ensure dependency rules (e.g., presence of conflicting parcels across pensions) and enforce business restrictions such as mutual exclusivity of parcel types.
  - SQL statements: FETCH NEXT PNSCNP_CUR INTO :WS-NPENS-CNP, :WS-CEVENTO-CNP, :WS-CPAREN-CNP, :WS-CMCSOB-CNP, then SELECT COUNT(*) FROM PARCELAS WHERE NPENS_PAR = :WS-NPENS-CNP AND CPAR_PAR IN (...) etc.

- ESCREVE-DESCRICAO / ESCREVE-DESCRICAO-E-TESTA
  - Purpose: Manage writing/ buffering of descriptive messages (DD-DESCRICAO) and call INSERIR-HISTALT when threshold reached (DD-MAX-HA). Build DD-DESCRICAO entries from DD-CODIGO/DD-VARIAVEL for later historic insertion.
  - Variables: DD-MAX-HA, DD-DESCRICAO, W-DESCR-HA, DD-CODIGO, DD-VARIAVEL, IND-DESC.

- INSERIR-HISTALT
  - Purpose: insert a HISTALT row with the accumulated DD-DESCRICAO, timestamped. Uses CAST for TIMESTAMP.
  - SQL: INSERT INTO HISTALT::W-VERSAO COLUMNS (NBEN_HA, DTHORA_HA=CAST(:W-DATAHORA-CAST AS TIMESTAMP), CTALT_HA, ORIGEM_HA, NEMP_HA, TERM_HA, DTPRC_HA, DESCR_HA, DESCRC_HA)

- TRATA-FPEDOC, TRATA-FPEDOC-PEDIDO-8, ERRO-TRATA-FPEDOC...
  - Purpose: Access SFS file FPEDOC via file operations (OPEN I-O, READ, DELETE, WRITE), do duplicate-key detection and create request records (pedidos) depending on logic (CTPED-PD codes, e.g. 15, 16, 8).
  - Files used: FPEDOC (file with key NPENS-PD, CTPED-PD, NORD-PD) — file layout in COPY RPEDOC/RPERMISSOES.
  - Variables: ERACSF-STATUS flags, AGU-FPEDOC, NPENS-PD, CTPED-PD, NORD-PD, UTIL-PD, PEDOC-TIPO1-PD.
  - Verbs used: OPEN I-O FPEDOC, READ FPEDOC, DELETE FPEDOC, WRITE RPEDOC, CLOSE FPEDOC.

- FECHAR-RDMS-DMS / LOCK-TABELAS / LIMPA-VAR-ERROS-RDMS
  - Purpose: Manage RDMS thread lifecycle (END THREAD), lock necessary RDMS tables in SHARED UPDATE or RETRIEVAL, and clear DB error variables prior to SQL statements (helper routines).

- ERRO-APLICACAO / ERRO-BD
  - Purpose: Unified error handling routines:
    - ERRO-APLICACAO: ROLLBACK via EXEC SQL ROLLBACK, DESCODIFICAR-ERRO, handle specific codes (e.g., 695), call FECHAR-RDMS-DMS and send message to error mechanism (ENVIAR-MSG-ERR-EPORTAL).
    - ERRO-BD: handle database errors, call error-specific handlers depending on DD-FLAG-ERRO flags, close files, decode and send error message.

6) Copybooks referenced (explicit COPY lines from program)
- COPY SELECT-FDPERMISSOES.
- COPY SELECT-FPEDOC.
- COPY RPERMISSOES.
- COPY RPEDOC.
- COPY MCBDEF-UCOB.
- COPY DD-DMSPROTN.
- COPY MSG-ERR.
- COPY MSG-807.
- COPY DD-DATA.
- COPY DD-TABERROS.
- COPY DD-DATA-HORA.
- COPY DD-ERRO-RDMS.
- COPY DD-DESCR-HA.
- COPY DD-DIFDATAS.
- COPY DD-DATAPROC.
- COPY DD-INCTIME.
- COPY DD-REGIME.
- COPY DD-FDPERMISSOES1 (REALAR variant).
- COPY DD-FILE-STATUS-SFS.
- COPY DD-TABCUTIL.
- COPY DD-ERRO-SCRATCH.
- (Many DD-* table copybooks)
  - COPY DD-TABCVP.
  - COPY DD-PNSCNPVP.
  - COPY DD-PNSCNP.
  - COPY DD-CONVIID.
  - COPY DD-PNSCGA.
  - COPY DD-PARCELACGA.
  - COPY DD-PNSID.
  - COPY DD-STCP.
  - COPY DD-BENGUELA.
  - COPY DD-HISTALT.
  - COPY DD-TABVALFIXO.
- COPY DD-VAR-ERROS.
- COPY PD-ERRO-SCRATCH.
- COPY PD-UTIL.
- COPY PD-REGIME.
- COPY PD-VER-OCORR-PERMISSOES.
- COPY PD-FDPERMISSOES.
- COPY PD-DATAPROC.
- COPY PD-DIFDATAS.
- COPY PD-DESCR-HA.
- COPY PD-TABERROS.
- COPY PD-DATA-HORA.
- COPY PD-INCTIME.
- COPY PD-ERRO-RDMS-DMS, PD-ERRO-ERACSF-DMS, PD-ERRO-SFS-DMS.
- COPY PD-LIMPA-VAR-ERROS.
- COPY API-MSG-ERR-EPORTAL, API-ERRO-EPORTAL, API-PLATFORM-EXIT (VRS064 variant).
- Other PD-* copies for CGA path (PD-TABERROS, PD-VALID-DAT etc.)

Note: Many copybooks hide FD definitions (files), DD field definitions and error handling structures (MERR, DD-*, SCR-*, W-* fields used in SQL). A converter must expand these copies.

7) File access (file name, mode, verbs used, status variable, FD linkage)
Files referenced either via copybooks or explicit code:
- FPEDOC
  - File record layout: from COPY RPEDOC (not shown inline).
  - Mode: I-O (opened with OPEN I-O FPEDOC).
  - Verbs used: OPEN I-O, READ FPEDOC, DELETE FPEDOC (DELETE with INVALID KEY handling), WRITE RPEDOC (WRITE with INVALID KEY GO error handler), CLOSE FPEDOC.
  - Status variables: file-status variable checks use FILE-STATUS (from COPY DD-FILE-STATUS-SFS?), DD-ERRO-FILE-STATUS moved on error. Also DD-FLAG-ERRO-SFS.
  - FD linkage: FD for FPEDOC is in COPY RPEDOC (not visible in snippet). All file names and record layouts likely in copybooks (SELECT-FPEDOC, SELECT-FDPERMISSOES).
- FDPERMISSOES (permissions file) — accessed by ABRE-FDPERMISSOES, LER-FDPERMISSOES, FECHA-FDPERMISSOES (actual verbs likely inside copy PD-FDPERMISSOES)
  - Mode: open/read semantics present; status variables: ERACSF-POS, DD-AGU-PERMISSOES, DD-EXISTE-OCOR, DD-ERRO-FILE-STATUS used.
  - FD linkage: defined in copybooks (SELECT-FDPERMISSOES/ DD-FDPERMISSOES).
- Internal database (RDMS) access is via embedded SQL — there are no explicit FD entries for RDMS tables. Instead operations use EXEC SQL, and the program sets DD-COMANDO-LE/DD-TABELA-LE/DD-NBEN-LE typically for logging and diagnostic fields (not DB2 API but conventions).
- Status variables for SQL:
  - SQLCODE-OK, SQLCODE-EOF checks are used. The actual SQLCODE variable names are likely declared in COPY DD-VAR-ERROS or similar.
  - DD-FLAG-ERRO-RDMS, DD-FLAG-ERRO-DMS, DD-FLAG-ERRO-SFS etc. control which error routines run.
- FD linkage for SFS/FPEDOC is in copybooks (not visible). All file definitions (FD, SELECT clause) are not present inline and must be read from COPYs.

8) Embedded SQL / DB2 statements (type, purpose, variables used)
The program uses many embedded SQL blocks. I list types and representative statements, with purposes and variables:

- Thread management
  - EXEC SQL BEGIN THREAD END-EXEC.
    - Purpose: start DB thread.
    - Checked with SQLCODE-OK.

  - EXEC SQL END THREAD END-EXEC.
    - Purpose: end DB thread (in FECHAR-RDMS-DMS).

- SET SCHEMA / USE DEFAULT SCHEMA
  - EXEC SQL USE DEFAULT SCHEMA #RDMSCH# END-EXEC.
    - Purpose: set default DB schema (string substituted at build/runtime).

- LOCK TABLE
  - EXEC SQL LOCK TABLE TABVALFIXO IN RETRIEVAL END-EXEC.
  - EXEC SQL LOCK TABLE PNSCNP, PARCELAS, STCP, BENGUELA, TABCVP, PARCELASVP, PNSID, CONVIID IN SHARED UPDATE END-EXEC.
  - Purpose: acquire locks to guarantee consistency during updates.

- DECLARE CURSOR ... FOR RETENTION / CURSOR declarations
  - Example:
    - EXEC SQL DECLARE PNSCNP_CUR CURSOR FOR RETENTION SELECT NPENS_CNP, CEVENTO_CNP, CPAREN_CNP, CMCSOB_CNP FROM PNSCNP WHERE NID_CNP = :WS-NID-CNP AND NPENS_CNP <> :M807-NPENS AND CSUSP_CNP = 0 END-EXEC.
    - EXEC SQL DECLARE PARCELAS_CUR CURSOR SELECT DTINI_PAR, CPAR_PAR, VPAR_PAR FROM PARCELAS WHERE NPENS_PAR = :W-NPENS-RED END-EXEC.
    - DECLARE TVFIXO_CUR CURSOR SELECT VALOR_TVF FROM TABVALFIXO WHERE COD_TVF = :W-COD-TVF AND DTENT_TVF <= CAST (:W-DATA10-CAST AS DATE) AND DTINI_TVF <= :W-DATA21-ANO END-EXEC.
  - Purpose: read multiple rows; used with OPEN/FETCH/CLOSE.

- OPEN / FETCH / CLOSE
  - Typical pattern:
    - EXEC SQL OPEN <cursor> END-EXEC.
    - EXEC SQL FETCH NEXT N <cursor> INTO :host-variables END-EXEC.
    - EXEC SQL CLOSE <cursor> END-EXEC.
  - Variables used: host variables prefixed with : (e.g., :W-DTINI, :W-CPAR, :W-VPAR, :AUX-INFO, :W-NPENS-RED).

- SELECT ... INTO
  - Many SELECT statements retrieving single row into host variables:
    - SELECT NEMP_TCVP, DESCR_TCVP, DTGRV_TCVP FROM TABCVP WHERE NPENS_TCVP = :W-NPENS-RED AND CMODPAG_TCVP = 0 AND ORIGEM_TCVP = :W-ORIGEM-TCVP AND NSEQ_TCVP = 0 INTO :W-NEMP-TCVP, :W-DESCR-TCVP, :W-DTGRV-TCVP END-EXEC.
    - SELECT COUNT(*) FROM PARCELAS WHERE NPENS_PAR = :WS-NPENS-CNP AND CPAR_PAR IN ('22','2I','42','4I') INTO :CNT-PARCELAS-INV END-EXEC.
    - SELECT CMOR_PNS, NRNOMEP_PNS FROM PNSID WHERE NID_PNS = :W-NID-CNP INTO :W-CMOR-PNS, :W-NRNOMEP-PNS END-EXEC.
    - SELECT VALOR_TVF FROM TABVALFIXO (cursor).
    - SELECT NID_CNP, CCAIXA_CNP, CCONVI_CNP, ... FROM PNSCNP WHERE NPENS_CNP = :W-NPENS-RED INTO many :W- variables END-EXEC.
    - SELECT CODC_STCP, PAUM_STCP FROM STCP WHERE NPENS_STCP = :W-NPENS-RED INTO :W-CODC-STCP, :W-PAUM-STCP END-EXEC.
  - Purpose: load existing record values for comparisons/updates, or count occurrences for validation.

- INSERT
  - Examples:
    - INSERT INTO PARCELAS COLUMNS (NPENS_PAR = :W-NPENS-RED, DTINI_PAR = CAST(:W-DTINI-CAST AS DATE), CPAR_PAR=:W-CPAR(IND-PAR), VPAR_PAR=:W-VPAR(IND-PAR)) END-EXEC.
    - INSERT INTO HISTALT::W-VERSAO COLUMNS (NBEN_HA=:W-NPENS-RED, DTHORA_HA=CAST(:W-DATAHORA-CAST AS TIMESTAMP), CTALT_HA=7729, ORIGEM_HA=:W-ORIGEM-HA, NEMP_HA=:SCR-NEMP, TERM_HA=:DD-TERM, DTPRC_HA=:W-DATA8ANOMES-RED, DESCR_HA=:W-DESCR-HA, DESCRC_HA=NULL) END-EXEC.
    - INSERT INTO CONVIID COLUMNS (...) END-EXEC.
  - Purpose: add parcels, history, or relationship records.

- UPDATE
  - Example:
    - UPDATE PNSCNP COLUMNS (CCAIXA_CNP=:W-CCAIXA-UPD, CCONVI_CNP=:W-CCONVI-UPD, CREGIME_CNP=:W-CREGIME-UPD, ... ) WHERE NPENS_CNP = :W-NPENS-RED END-EXEC.
    - UPDATE PNSCGA COLUMNS(...) WHERE NPENS_CGA=:W-NPENS-RED AND CMODPAG_CGA=:M807-CMODPAG-CGA END-EXEC.
    - UPDATE PNSID COLUMNS (NRNOMEP_PNS=:W-NRNOMEP-PNS :W-NRNOMEP-PNS-NULO) WHERE NID_PNS=:W-NID-CNP END-EXEC.
  - Purpose: apply authorized changes to pension master records.

- DELETE
  - Examples:
    - DELETE TABCVP WHERE NPENS_TCVP = :W-NPENS-RED AND CMODPAG_TCVP = 0 AND ORIGEM_TCVP = :W-ORIGEM-TCVP AND NSEQ_TCVP = 0 END-EXEC.
    - DELETE PARCELASVP WHERE NPENS_PARVP = :W-NPENS-RED END-EXEC.
    - DELETE PARCELAS WHERE NPENS_PAR = :W-NPENS-RED END-EXEC.
    - DELETE PARCELACGA WHERE NPENS_PCGA=:W-NPENS-RED AND CMODPAG_PCGA=:M807-CMODPAG-CGA END-EXEC.
    - DELETE BENGUELA WHERE NPENS_BG = :W-NPENS-RED END-EXEC.
    - DELETE CONVIID WHERE NPENS_CIID = :W-NPENS-RED END-EXEC.
  - Purpose: remove obsolete rows when elimination or full parcel replacement needed.

- ROLLBACK
  - EXEC SQL ROLLBACK END-EXEC used in ERRO-APLICACAO to rollback DB changes on application logic errors.

- SELECT COUNT(*) and scalar SELECTs
  - Purpose: validations and conditions (e.g., count existing parcels with certain CPAR, count other pensions for dependencies).

- FETCH LAST / FETCH NEXT N
  - Example: FETCH LAST TVFIXO_CUR INTO :W-VALOR-TVF (used to obtain latest TABVALFIXO value)
  - Purpose: get last applicable value or fetch a block of rows.

- Use of CAST and DATE/TIMESTAMP literals
  - Program constructs string date/timestamp literal (e.g., "DATE 'YYYY-MM-DD'") and uses CAST(:W-DATA-CAST AS DATE) or CAST(:W-DATAHORA-CAST AS TIMESTAMP) in SQL statements. Host variables for dates are passed as text and cast in SQL.

- Error handling around SQL
  - After each EXEC SQL, the program checks SQLCODE-OK or SQLCODE-EOF and acts accordingly (GO ERRO-BD or application error handlers). It also uses custom macro variables (DEAD-LOCK) to detect deadlocks and produce specific WS-TERRCOD codes.

9) File/DB objects (RDMS tables) used
- PNSCNP, PARCELAS, TABCVP, PARCELASVP, TABVALFIXO, PNSID, STCP, BENGUELA, HISTALT, PNSCGA, PARCELACGA, PARCELACGAVP, CONVIID, PARCELACGA, PARCELACGAVP, PNSID (reused), PNSCNP (cursor), PARCELASVP, PARCELAS_SOC_CUR etc.
- Purpose: these tables hold pension master data, parcel breakdowns, fixed values table, history, special bogy tables (BENGUELA), and relationship tables (CONVIID).

10) Control flow / logic highlights
- Main branching depends on M807-CMODPAG-CGA (0 => CNP flow (PROGRAMA-CNP) else CGA flow (PROGRAMA-CGA)).
- Many VRSxxx conditional compilation or variant flags modify logic for particular releases or patches: e.g., VRS021 changes date thresholds, VRS035 adds cursor declaration for PNSCNP_CUR, many VRS0xx controlling extra checks or new fields.
- Extensive business rule validations enforce constraints by checking sums across existing and incoming parcel sets, using TABVALFIXO thresholds (W-COD-TVF) and special-case pension numbers.
- When differences are detected, the program creates an audit/history entry (HISTALT) using DD-DESCRICAO, and then updates the corresponding PNS table with W-*-UPD fields and null indicators.

11) Error handling approach
- Two primary error categories:
  - ERRO-APLICACAO (application/business rule violation): performs EXEC SQL ROLLBACK, builds human-readable message (via DESCODIFICAR-ERRO), may persist scratch error log (ERRO-SCRATCH), close RDMS thread and DMS, send message to EPORTAL via ENVIAR-MSG-ERR-EPORTAL.
  - ERRO-BD (DB/SQL error): run database-specific error handlers (e.g., ERRO-RDMS), close files, update error structures and send error notification.
- Uses WS-TERRCOD and MERR structures to build error messages and codes.
- Dead-lock handling: when SQL fails and DEAD-LOCK condition is set, specific error code 506 moved to WS-TERRCOD and treated as application error.

12) Missing / truncated parts and notes
- Many structural definitions (FDs, copybook contents) are not included in the provided file because they are imported with COPY statements. These contain FD entries for files (FPEDOC, FDPERMISSOES, RPERMISSOES), many working-storage variables (W-... fields used in SQL), and error handling variables (DD-*, MERR-*, SQLCODE variables).
- The program references numerous COPY PD-* and DD-* copybooks; the full variable map and file definitions require expanding those copies.
- The source appears largely complete for the logical flow, but some lower-level routines are pulled from COPYs (e.g., LIMPA-VAR-ERROS, DESCODIFICAR-ERRO, SRDP routines) and are not visible inline. The file ends with COPY PD-LIMPA-VAR-ERROS; there may be subsequent paragraphs or a trailing END PROGRAM not shown, but Program flow and main sections are present.
- Many conditional VRSxxx blocks indicate variants; conversion must interpret or preserve these conditional compilation flags.

13) Recommendations for conversion / reuse
- Expand all COPYs to obtain complete variable and FD definitions prior to automated conversion.
- Map EXEC SQL statements carefully: they use host variables and null indicators (NULO fields) and CASTed date strings. Conversion should preserve CAST semantics or translate to target DB param binding.
- Files: FPEDOC and FDPERMISSOES are COBOL files (SFS). Identify equivalent file system or DB implementation and handle OPEN I-O, READ/WRITE/DELETE semantics and invalid key handling.
- Error handling and message sending (ENVIAR-MSG-ERR-EPORTAL) use external APIs: MCB$ENT and ERACSF calls and DMS interactions — target platform must implement comparable services.
- Preserve business rules and numeric computations (COMPUTE ROUNDED, many checks) exactly; these are critical.

14) Explicit list of embedded SQL statements (concise)
- BEGIN THREAD / END THREAD
- USE DEFAULT SCHEMA #RDMSCH#
- LOCK TABLE <table-list> IN RETRIEVAL / SHARED UPDATE
- DECLARE <cursor> CURSOR FOR <SELECT ... FROM ... WHERE ...>
- OPEN <cursor>
- FETCH NEXT <N> <cursor> INTO :host-variables
- FETCH NEXT <cursor> INTO :host-variables (looped)
- FETCH LAST TVFIXO_CUR INTO :W-VALOR-TVF
- SELECT ... INTO :host-variables (many single-row selects across PNSCNP, PNSCGA, TABCVP, PNSID, STCP, BENGUELA, TABVALFIXO)
- SELECT COUNT(*) FROM ... INTO :count
- INSERT INTO <table> COLUMNS (...)
- UPDATE <table> COLUMNS (...) WHERE ...
- DELETE FROM <table> WHERE ...
- ROLLBACK
- Notes: Many SELECTs use CAST(:string AS DATE/TIMESTAMP) where program builds date/timestamp textual literals prior to SQL.

If you want:
- I can expand and enumerate every variable and SQL host variable by extracting copybook names and lines where fields are used (requires feeding the program's COPY contents).
- I can output a JSON representation of the structure (sections, paragraphs, SQL statements, variables per paragraph) for automated conversion. Which format would you prefer?

---

