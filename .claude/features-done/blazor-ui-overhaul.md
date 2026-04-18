# Feature: blazor-ui-overhaul

## Goal
Overhaul the Blazor monitoring UI: slim ListView columns, move details to a dialog showing all metadata + JSON content, show persist type per cache type, and make Clear Cache refresh the ListView.

## Scope
- `CacheTypeInfo` — add `PersistType` field
- `CacheMonitor.Set/Track` — populate `PersistType`
- `ListView.razor` — slim columns to Key, Expires, Size, Load Time, Stale; add persist type to the type-level grid; add chevron button per row; subscribe to monitor events for live refresh
- `SummaryView.razor` — subscribe to monitor events for live refresh
- New `DetailView.razor` — dialog component with all metadata + JSON content via `PeekAsync` + reflection + `CopyButton`

## Acceptance Criteria
- ListView shows slim columns with chevron to open detail dialog
- Detail dialog shows all metadata (Created, Updated, Fresh Span, Last Accessed, Access Count) plus cached data as JSON
- Copy JSON button works
- Persist type column appears on the type-level grid
- Clear Cache button refreshes the ListView without reload
- All tests still pass

## Done Condition
User confirms the UI works as expected.

## Originating Branch
develop
