# Feature: idempotent-addcache

## Goal
Make `AddCache()` safe to call multiple times, merging type registrations instead of throwing.

## Scope
- `CacheRegistrationExtensions.AddCache` — guard against duplicate DI registrations
- `CacheRegistrationExtensions.AppendPreviousRegistrations` — skip duplicate types instead of throwing
- `CacheOptions.AddType` — skip duplicate types instead of throwing
- Tests verifying idempotent behavior

## Acceptance Criteria
- Calling `AddCache()` twice with the same type registration does not throw
- Calling `AddCache()` twice with different types merges them
- First registration wins when same type is registered with different options
- All existing tests still pass

## Done Condition
User confirms the fix is satisfactory.

## Originating Branch
develop
