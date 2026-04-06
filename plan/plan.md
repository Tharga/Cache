# Plan: monitor-track-persisted

## Steps
- [~] 1. Add `Track<T>` to `IManagedCacheMonitor` and implement in `CacheMonitor`
- [ ] 2. Call `Track<T>` from `CacheBase.GetCoreAsync` on fresh persistence hits
- [ ] 3. Write tests
- [ ] 4. Build, run full test suite, commit
