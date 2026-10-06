# Non-admin test suite

Run from an **unelevated Windows terminal**, using the solution's existing project layout and your up-to-date SecurityDescriptor project:

```powershell
dotnet build UnitTests/UnitTests.csproj
dotnet run --project UnitTests/UnitTests.csproj --no-build -- --report-trx
```

The project already enables the MSTest executable runner. Test Explorer also supports these tests and their categories. The default run includes active regression tests. The October 5 validation below records the original failures; the October 6 validation records the subsequent fixes.

To run supported behavior separately while working through the recorded defects:

```powershell
dotnet run --project UnitTests/UnitTests.csproj --no-build -- --filter 'TestCategory!=KnownRegression'
dotnet run --project UnitTests/UnitTests.csproj --no-build -- --filter 'TestCategory=KnownRegression'
```

Excluding KnownRegression is a diagnostic convenience, not evidence that the complete suite passes. Remove a test's KnownRegression category when its production fix lands; retain its assertions.

| Category | Behavior |
| --- | --- |
| Pure | Result and disposal contracts; correlation metadata; model clones/equality; converters and malformed input; artifact hashes and temporary files; enum/time/byte helpers; native buffer lifetime without LSA calls |
| Catalog | Every shipped catalog setting is individually deserialized and round-tripped, plus whole-catalog validation. Native SID/ACL entries are inconclusive off Windows. The source catalog is linked into the output, not duplicated in git. |
| WindowsModel | Built-in SID/SeRight and SDDL/ACL value serialization, snapshot protection flags, and cloning. No policy or ACL is applied. |
| WindowsHKCU | Real RegKey values, path creation/deletion, enumeration, ancestry, clones, and ownership in a private HKCU sandbox. Inconclusive off Windows or in an elevated process. |
| KnownRegression | Correctness contracts violated by the reviewed production code; failures are active, not ignored or disguised as passing characterizations. |

## Isolation and boundaries

Each registry test creates exactly one GUID-named leaf at `HKCU\Software\StigRemediator.UnitTests.<guid>` using the 64-bit registry view. It closes owned handles and deletes only that leaf in cleanup. No test connects to remote registry, edits HKLM/HKU, changes machine ACLs, grants privileges, alters account/lockout policy, opens certificate stores for writes, calls the operational Logger, or invokes real remediation/rollback against the catalog.

Artifact verification writes only into a GUID-named directory under the user's temp directory. It uses rooted artifact paths so the test host's executable directory need not be writable. ArtifactFirstBuild.Construct/Verify are excluded because they use the hard-coded operational network share. Memory tests allocate only their own buffers and never invoke LSA. Unicode regression tests validate lengths before reading the buffer.

Parallel execution uses independent fixtures and private JsonSerializerOptions copies. Recursive-create tests close generated borrowed wrappers explicitly. The only production-project change is `InternalsVisibleTo("UnitTests")`, which permits direct tests of the in-memory rollback hierarchy without reflection or Logger side effects.

This is broad regression coverage, not a replacement for the staged WhatIf/remediation/rollback plan. Remote-host propagation, access-denied recovery, SYSTEM-only keys, SACL/security privileges, native policy mutation, certificate-store remediation, operational logging, UI workflows, and the previously identified remediation disposal/Pattern WhatIf paths still require suitable integration fixtures or manual tests.

## Validation on October 6, 2026

Base STIG commit: `ae64b0b`. Added 16 Pure test cases covering value-interface equality dispatch, registry data content and kind, Overwrite and action identity, pattern-stage boundaries and delimiter collisions, resolved-target equality and absence in both directions, setting/rule content equality and clone equality, rule hash consistency across insertion order, and DisposableList cleanup after an item throws.

The same temporary Linux harness compiled the committed test files: **967 cases: 856 passed, 0 failed, 111 skipped**. All new cases passed without registry access or elevated privileges. Windows-dependent cases remain unexecuted in this environment.

## Validation on October 5, 2026

Base STIG commit: `dfad869d40015c24f1be144111898103db3a9d7c`.

A temporary Linux validation project used the actual Common project and published SecurityDescriptor project, and compiled the exact Remediator Result/Null, log-hierarchy, enum, and time-helper source files alongside these MSTest files. No production behavior was mocked. This harness is not a replacement for the solution project and is not committed.

- Portable supported behavior and catalog run: **819 passed, 66 skipped, 0 failed**. Skipped cases are 62 SeRight settings, 3 file-ACL settings, and the whole catalog test.
- Portable KnownRegression run: **21 failed**, reproducing the intended defects below.
- Final complete harness run: **951 discovered cases: 819 passed, 21 failed, 111 skipped**. Counts include per-catalog-setting and exhaustive single-byte data rows.
- WindowsModel and WindowsHKCU behavior compiled in that harness but could not be executed on Linux. Their runtime results are unknown.
- A full solution build was attempted and stopped on pre-existing API mismatches with published SecurityDescriptor HEAD `ccaea5a34bd32f7f6ee12291e5934f4f7a0223f3`: missing `Sddl.ToDirectorySecurity`, `Trustee.Clone`, and `Sddl.Equals(..., compareActiveOnly: ...)`. The user's newer local dependency may resolve these; the test commit does not alter it.

## Reproduced defects

| Regression | Cases | Consequence |
| --- | ---: | --- |
| Absent registry data serialization | 5 | DWORD/QWORD null becomes zero, Binary null becomes empty bytes, MultiString null emits invalid JSON, and None-kind snapshots cannot serialize. Absence must remain distinguishable from a present value. |
| Absent pattern data serialization | 4 | The corresponding pattern converter has the same null-data defects. |
| Rule equality/hash contract | 1 | Case-insensitive RuleId equality uses a case-sensitive hash, breaking hash-based deduplication. |
| ByteMagic empty/multiple-byte formatting | 2 | Empty input throws; multiple bytes overwrite earlier characters and leave NUL characters in output. Exhaustive single-byte cases pass. |
| LsaUnicodeStringWrapper UTF-16 lengths | 4 | Length/MaximumLength use character counts rather than byte counts, and the copied payload is truncated. Tested with ASCII, Japanese, and a surrogate pair. No LSA API is called. |
| Log hierarchy casing | 1 | Case-insensitive outer dictionaries reach LogRule.Add, which rejects case variants using case-sensitive comparisons. |
| Invalid regex target selection | 1 | A malformed TargetPattern is swallowed and becomes null, potentially broadening selection. |
| Mutable registry snapshot clones | 2 | Binary and MultiString Clone share the source array. |
| Pattern clone's resolved target | 1 | Clone drops ResolvedTarget, losing the resolved rollback identity. |

Three additional WindowsHKCU regression tests encode source-visible contracts that could not be executed here: GetKeyValue's full parent path, equal RegKey hashes, and case-insensitive absolute OpenKey lookup. Treat these as unverified Windows regression cases until run on Windows.
