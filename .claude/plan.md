# Plan: eplicta-cache-requests

## Steps

### Request 1: Deprecate IMemoryWithRedis
- [x] 1. Mark `IMemoryWithRedis` and `MemoryWithRedis` as `[Obsolete]` with descriptive message
- [x] 2. Write test verifying obsolete attribute is present
- [x] 3. Run tests, commit

### Request 2: ReturnDefaultOnFirstLoad
- [x] 4. Add `ReturnDefaultOnFirstLoad` property to `CacheTypeOptions`
- [x] 5. Add `ReturnDefaultOnFirstLoad` to `CacheTypeInfo`
- [x] 6. Modify `GetCoreAsync<T>()` in `CacheBase.cs` to return `default(T)` and trigger background load when option is enabled and no cache exists
- [x] 7. Write tests covering all 4 combinations of SWR × ReturnDefaultOnFirstLoad
- [x] 8. Run full test suite, commit

### Finalize
- [x] 9. Run full test suite, summarize for review
