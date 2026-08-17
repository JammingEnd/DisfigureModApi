# "More >>" Clear-Buttons Design

Date: 2026-08-17

## Purpose

Add a duplicated "More >>" button to the home-screen weapon selection that clears all
active vanilla `GunButton (x)` slots back to free slots and then assigns registered
modded weapons to them.

## Background (found in dump.cs)

- The weapon-selection screen is the `Start` GameObject under `Canvas` in the home scene
  (path `Canvas/Start/`). All weapon UI lives under it.
- Weapon buttons are `Canvas/Start/GunButton (x)`; some are disabled (legacy content) and
  must be skipped — only `activeInHierarchy` ones are affected.
- The back button is `Canvas/Start/back` carrying a `Button` and a `BackButton` script
  (`dump.cs:172525`, TypeDefIndex 1866). `BackButton` fields: `makeActive`,
  `makeNotActive`, `playerControls`, `texts[]`; methods `Start`, `Update`,
  `clickFunctionality()`.
- The weapon-selection panel is shown when `StartMenu` is enabled
  (`StartMenu.OnEnable`, `dump.cs:192542`). This is the reliable hook for "panel shown".

## Design

### 1. Create/recreate the "More >>" clone (`StartMenu.OnEnable` postfix)

In `Modules/UIinteractor.cs`, extend the existing `UIinteractorStart` postfix
(which already hooks `StartMenu.OnEnable` for slot assignment):

- **Clone creation runs before the `newWeapons.Count == 0` early-return guard**, so the
  "More >>" button is always created when the panel is shown regardless of how many
  modded weapons are registered. (The existing guard only gates the slot-assignment
  loop, not clone creation.)
- If `moreButtonClone == null` (Unity destroyed-reference check, which handles scene
  reloads after playing a match):
  - Find the back button: `Object.FindObjectsOfType<BackButton>()`, filter
    `gameObject.name == "back"` and `activeInHierarchy` (it lives at `Canvas/Start/back`).
  - Clone it: `GameObject.Instantiate(backButton.gameObject)` (lands as a sibling under
    `Canvas/Start`). Rename the clone (e.g., to `MoreButton`) so the `BackButton`
    name-filter can't accidentally match it on a later re-create.
  - Disable the clone's `BackButton` component (`enabled = false`) so its `Update` /
    `clickFunctionality` don't run and it doesn't do ESC-close logic.
  - Set `transform.position = new Vector3(-59.9823f, -65, -53.6812f)`.
  - Set the label to `"More >>"`. Resolve the label by a specific child/index rather
    than `GetComponentInChildren<Text>()` first-match (the back button has multiple
    Text children). The implementer should identify the correct Text child at runtime.
  - Clear `Button.onClick` (`RemoveAllListeners`) and add our clear handler.

Because the hook is `StartMenu.OnEnable`, the clone is created every time the weapon
panel is shown, including after returning from a match (the destroyed reference check
recreates it). This satisfies "reuse, but recreate when the scene switches".

### 2. Clear action (the "More >>" button click)

`ClearGunButtons()`:

- **Guard: if `NewWeaponInitiator.newWeapons.Count == 0`, no-op.** Without this, a click
  with no registered modded weapons would clear every active vanilla `GunButton*` to
  `"COMING SOON"` with `weaponname=""` and assign nothing, breaking the vanilla
  weapon-selection UI until reload.
- Walk the `Canvas/Start` hierarchy for GameObjects whose name starts with `GunButton` and
  that are `activeInHierarchy`.
- For each, reset to a free slot:
  - `weaponselect.weaponname = ""`
  - label text = `"COMING SOON"`
  - `Button.enabled = true`
  - `weaponselect.enabled = true`
  - also reset `unlockedString = ""`, `weaponIsUnlocked = false`, and `selectedColor`.
    For `selectedColor`, snapshot the vanilla value from a fresh `GunButton*` before
    clearing rather than guessing a constant, so per-weapon state from
    `WeaponSelectPatchStart` does not linger on a cleared button within the same session.
- Reset the shared `assignedWeapons` bookkeeping so previously-assigned modded weapons can
  be re-assigned to the freed slots.
- Then call `AssignWeaponsToFreeSlots()` to fill the freed slots with registered modded
  weapons. The assign step must re-apply the same per-weapon state that
  `WeaponSelectPatchStart` applies (`unlockedString`, `weaponIsUnlocked`,
  `selectedColor = red`), so reassigned modded buttons look identical to directly-
  registered ones (`Start` won't re-fire on a scene object).

### 3. Shared slot logic

- Extract the current `StartMenu.OnEnable` assignment loop (in `UIinteractorStart`) into a
  reusable method `AssignWeaponsToFreeSlots(StartMenu)` that both the `OnEnable` patch and
  the "More >>" click call.
- The shared method iterates the **same collection** the clear action targets: the set of
  active `GunButton*` GameObjects under `Canvas/Start`. The `GunButton (x)` buttons are
  children of the `Start` GameObject (the `StartMenu` component host), so both paths use
  the single consistent helper `FindGunButtons()` to walk `Canvas/Start`, guaranteeing the
  two sets always match.
- Replace `IsAvaibleButton()` (hardcoded names 27/28/31/32) with a `FindGunButtons()`
  helper that matches the `GunButton` name prefix + active state, so all live gun buttons
  become usable slots. This is an intentional behavior change: previously only slots named
  27/28/31/32 were usable; now every active `GunButton*` is a candidate slot.

## Files

- `Modules/UIinteractor.cs` — `UIinteractorStart` postfix extension (clone creation),
  `ClearGunButtons()`, extracted `AssignWeaponsToFreeSlots()`.
- `Modules/Util/Extentions.cs` — replace `IsAvaibleButton` with `FindGunButtons` helper.

## Data flow

```
Weapon panel shown -> StartMenu.OnEnable -> clone "More >>" created/reused (if destroyed)
"More >>" click -> ClearGunButtons()
   -> find active GunButton (x) under Canvas/Start
   -> reset each to free slot (weaponname="", "COMING SOON", enabled, cleared state)
   -> reset assignedWeapons bookkeeping
   -> AssignWeaponsToFreeSlots() -> assign registered modded weapons
```

## Success Criteria

- "More >>" button appears when the weapon-selection screen is shown, positioned at
  `(-59.9823, -65, -53.6812)`.
- Clicking it resets active vanilla gun buttons to free slots.
- Registered modded weapons are assigned to the freed slots immediately, showing the same
  unlock/selection state (`selectedColor = red`) as directly-registered modded weapons.
- Disabled/legacy gun buttons are unaffected.
- Re-entering the menu after a match recreates the clone (destroyed-reference check).
- Clicking "More >>" with zero registered modded weapons leaves the UI unchanged.

## Note

- The position `(-59.9823, -65, -53.6812)` came from a dumped scene transform; the
  implementer should re-verify the exact placement at runtime.