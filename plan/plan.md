# Plan: mongodb-clear-cache

## Steps
- [~] 1. Add `IManagedCacheMonitor` to MongoDB constructor and subscribe to `RequestEvictEvent`
- [ ] 2. Write test verifying eviction
- [ ] 3. Run full test suite, commit
