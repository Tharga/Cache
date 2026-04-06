# Feature: mongodb-default-config

## Goal
Fix MongoDB setup when not using "default" config by giving `ConfigurationName` a sensible default.

## Scope
- `MongoDBCacheOptions.ConfigurationName` — set default to `"Default"`

## Acceptance Criteria
- `AddMongoDBOptions()` without explicit ConfigurationName uses `"Default"`
- Existing tests still pass

## Done Condition
User confirms the fix is satisfactory.

## Originating Branch
develop
