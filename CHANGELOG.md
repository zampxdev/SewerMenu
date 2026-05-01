# Changelog

## 2.0.0-beta.1 - 2026-05-01

This is the first pushed 2.0 beta branch build for Schedule I `0.4.5f2`.

### Added

- Modern 2.0 menu shell with animated tab transitions, smoother toggles, favorites, quick actions, and feature inspector panel.
- Reworked Ctrl+K command palette that can open from gameplay, toggle features, inspect matches, and spawn matching items.
- Animation quality setting with Auto/Balanced/Low behavior.
- Menu input lock to stop game hotbar scrolling while the menu is open.
- Full item spawner fuzzy search with category filtering and cached search entries.
- Item spawner quantity commands: trailing numbers, `x5`, `*5`, `qty:5`, `max`, and `stack`.
- Enter-to-spawn behavior in the full item spawner search field.
- Toast notifications for spawn success/failure and other important UI actions.
- Infinite ammo feature support.

### Changed

- Reworked ESP update behavior to reduce stutter.
- Reworked vehicle tab data access with caching to reduce tab-open lag.
- Improved full item spawner layout, text spacing, item rows, and footer status handling.
- Improved config save timing and UI timers by using unscaled time where appropriate.
- Reduced unnecessary scans in high-frequency feature updates.
- Updated local verification so it does not overwrite the game Mods folder with a Debug DLL.

### Fixed

- Fixed item spawner IMGUI layout/repaint crashes caused by changing layout control counts mid-event.
- Fixed hidden IMGUI button surfaces causing gray rectangles inside custom pill controls.
- Fixed item spawner text clipping in rows and footer status text.
- Fixed inventory failure feedback so full item spawning reports when items cannot fit.
- Fixed menu resize stability issues from expensive resize-time UI behavior.

### Release Blockers

- Final in-game acceptance test still required before public 2.0 release.
- Public release should not be tagged, uploaded, or published until explicitly approved.
