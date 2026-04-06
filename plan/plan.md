# Plan: cache-load-time

## Steps
- [~] 1. Add `LoadDuration` to `CacheItem` and `CacheItemBuilder`
- [ ] 2. Measure fetch time in `FetchQueue` with `Stopwatch`
- [ ] 3. Add `LoadDuration` to `CacheItemInfo`, update `CacheMonitor.Set/Track`
- [ ] 4. Update `CacheBase` to pass `LoadDuration` through
- [ ] 5. Add "Load Time" column to `ListView.razor`
- [ ] 6. Write tests
- [ ] 7. Build, run full test suite, commit
