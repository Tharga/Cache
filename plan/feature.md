# Feature: cache-content-darkmode

## Goal
Fix the cache content JSON panel so it stays readable in dark Radzen themes. Today the `<pre>` block in `CacheContent.razor` uses a hardcoded `background:#f5f5f5`, which combined with the theme's white default text colour renders white-on-white in dark mode.

## Originating request
`Tharga.Cache — Blazor / Dark-mode unreadable: hardcoded light background on cache content <pre> block` from Quilt4Net.Server, priority Low.

## Scope
- `Tharga.Cache.Blazor/CacheContent.razor`, line 19 — replace the hardcoded `background:#f5f5f5` with a Radzen theme variable, and explicitly set the text colour for safety.

## Acceptance criteria
- The JSON panel is readable in both light and dark Radzen themes (verified visually in `Tharga.Cache.BlazorServer`).
- Existing behaviour preserved — same border radius, padding, max-height, overflow.
- Build is clean and all tests still pass.

## Done condition
User confirms the JSON panel renders correctly under at least one dark theme.

## Originating branch
master
