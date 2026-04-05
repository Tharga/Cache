# Plan: idempotent-addcache

## Steps
- [~] 1. Write tests reproducing the bug
- [ ] 2. Fix `AppendPreviousRegistrations` — skip duplicates instead of throwing
- [ ] 3. Fix `CacheOptions.AddType` — skip duplicates instead of throwing
- [ ] 4. Guard DI registrations with `TryAdd*` methods
- [ ] 5. Run full test suite, commit
