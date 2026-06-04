# Changelog

## 1.0 - EA36 compatibility

- Updated RavenM's compatibility gate from Ravenfield EA29 to EA36.
- Added an EA36 compatibility layer for the renamed/reworked Instant Action menu API.
- Reworked lobby synchronization to use EA36's `InstantActionConfigMenu`, `TeamInfo`, `WeaponCollectionInfo`, and vehicle/turret slot APIs.
- Updated game mode detection for EA36, including `GameModeType.Battle`.
- Updated destructible synchronization for EA36's `Destructible.Shatter(Projectile)` signature.
- Updated Discord Rich Presence game mode labels for the EA36 menu model.
- Removed stale RavenScript custom weapon registration against the removed `WeaponManager.weapons` list.
- Updated default vehicle network prefab registration for EA36.
- Fixed the root `NuGet.Config` package source layout.
- Updated the project file so local builds prefer the installed Ravenfield EA36 managed assemblies, with the bundled `libs` folder as fallback.

Known limitations:

- This release has been verified to compile against Ravenfield EA36 and to include RavenM's embedded assets.
- Full in-game multiplayer testing was not performed in this workspace, so lobby/menu synchronization should be treated as experimental.
