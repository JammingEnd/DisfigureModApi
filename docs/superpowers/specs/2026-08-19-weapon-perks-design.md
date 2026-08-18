# Weapon Perks / Upgrade Trees for Modded Weapons

Date: 2026-08-19

## Purpose

Give each registered modded weapon its own weapon-perk/upgrade tree on the in-match
level-up screen, replacing the current behavior where a modded weapon silently shows
the pistol's upgrade tree (because vanilla can't resolve a non-vanilla `selectedWeapon`
name).

## Background (from Ghidra decompiles)

### How vanilla resolves a weapon's perk tree

`weaponupgradescreen$$Awake` (RVA `0x4E51E0`):

```
selectedWeapon = PlayerPrefs.GetString("selectedWeapon")   // StringLiteral_7505
switch (selectedWeapon):  // hardcoded name → index
  "pistol"         -> 0
  "shotgun"        -> 1
  "knife"          -> 2
  "sniper"         -> 3
  "boomerang"      -> 4
  "greatsword"     -> 5
  "sawlauncher"    -> ...
  "handcannon"     -> ...
  ...
temp = weaponUpgradesList[index]    // field 0x30 = the perk tree GameObject
```

- An **unknown weapon name** falls through every case and hits
  `Debug.Log("WEAPON UPGRADE NOT ASSIGNED PROPERLY. OPEN THIS SCRIPT AND CREATE NEW CASE FOR NEW WEAPON.")`
  (`StringLiteral_12286`) with `temp` left unset. `weaponUpgradesList` is a
  `List<GameObject>`; index 0 happens to be the pistol tree, which is why a modded
  weapon currently inherits pistol perks.

### How a perk is applied when clicked

`weaponupgrade$$clickFunction` (RVA `0x4E4E60`):

- Skips if `isMainMenu` (`param_1 + 0x198`) — menu clones from `MenuWeaponPerksHandler`
  are decorative.
- Calls `PlayerStats$$chooseWeaponUpgrade(pS, this)`.
- Adds `this.upgradeName` (`param_1 + 0x118`) to `PlayerStats.unlockedWeaponPerksList`
  (`pS + 0x140`), and calls `SideUpgradesDisplayHandling$$addSideUpgrade` so the chosen
  perk shows on the side panel.

`PlayerStats$$chooseWeaponUpgrade` (RVA `0x4606C0`):

- Plays the upgrade sound, closes the level-up UI, then calls
  `weaponupgrade$$weaponUpgradeStat` (applies `statName/change` fields) and increments
  the Steam stat `profile_total_weapon_perks_gained`.

## Current mod state (in the repo)

- `Modules/Patchers.cs`:
  - `WeaponUpgradeScreenAwake` postfix on `weaponupgradescreen.Awake` calls
    `AddNewWeaponUpgradeTreesToPlayer(true)` — clones `weaponUpgradesList[0]` (pistol
    tree) into `weaponUpgradesList`, **keeping** pistol upgrades.
  - `WeaponUpgradeScreenOnUpgrade` postfix on `OnEnable` grabs the **last** list entry
    and copies its first 8 children into `chosenList`.
- `Modules/Content/NewWeaponUpgrade.cs`:
  - `NewWeaponUpgradeRegistry` holds `NewWeaponUpgrades` and `NewTreeDef`
    (`refname → keepPistolUpgrades`).
  - `NewWeaponUpgrade : weaponupgrade` with `ownerWeaponReference`.
  - `AddNewWeaponUpgradeTreesToPlayer(instance, keepPistolUpgrades)` clones the pistol
    tree, renames it `<ref>WeaponUpgrades`, and overwrites its children with registered
    upgrades that match the active weapon reference.

## Problems with the current approach

1. **`Awake` still picks the pistol tree** for a modded `selectedWeapon` — the hardcoded
   switch runs first; the postfix appends a clone but `temp`/`weaponUpgradesList[index]`
   selection in vanilla `Awake` already chose index 0.
2. **`OnEnable` uses "last list entry"** — fragile; depends on our `Awake` postfix having
   appended, and can break when the vanilla list layout changes.
3. **`keepPistolUpgrades=true`** leaves pistol perks in the tree, which is the "pistol
   perks equipped" symptom. Modded weapons should get only their own perks (unless the
   mod author opts in to sharing).

## Design

### 1. Store the modded tree on the screen instance when Awake runs

`weaponupgradescreen` has a `temp` field (`dump.cs` field 0x30, public property `temp`)
that vanilla `Awake` sets to the resolved tree. When the active weapon is a registered
modded weapon, we set `temp` (and our own tracked clone) ourselves.

### 2. Patch `weaponupgradescreen.Awake` (prefix or postfix)

Replace the current `WeaponUpgradeScreenAwake` logic:

- If `NewWeaponInitiator.GetActiveWeapon()` is null (vanilla weapon), do nothing —
  vanilla resolves normally.
- Otherwise (modded weapon active):
  - Build/reuse the tree for that reference. If a tree for
    `<ref>WeaponUpgrades` isn't already in `weaponUpgradesList`, create it via
    `AddNewWeaponUpgradeTreesToPlayer(false)` (do **not** keep pistol upgrades by
    default).
  - Set `instance.temp = <that tree>` so vanilla `OnEnable`/`spawnUpgrades` uses our
    tree instead of the pistol's.

This should be a **prefix returning false** that fully overrides vanilla `Awake` for
modded weapons (skip the hardcoded switch + the "NOT ASSIGNED" log), and a normal
postfix for vanilla weapons (no-op). If a prefix that skips vanilla breaks
initialization (e.g. other fields set in `Awake`), fall back to a postfix that
overwrites `temp` after vanilla runs.

### 3. Rework `WeaponUpgradeScreenOnUpgrade` (OnEnable postfix)

Drop the "last list entry" heuristic. Use the tree that `Awake` selected:

- `GameObject tree = NewWeaponUpgradeUtils.GetTreeForWeapon(__instance, activeRef)` —
  returns the clone we stored, or falls back to `__instance.temp`.
- Copy its children into `chosenList` as today, but guard against null/missing children
  and skip the `"WEAPON UPGRADE NOT ASSIGNED"` path entirely for modded weapons.

### 4. API surface (`NewWeaponUpgrade.cs`)

- `NewWeaponUpgradeRegistry`:
  - Keep `RegisterNewWeaponUpgrade(NewWeaponUpgrade)`.
  - Change `RegisterNewWeaponUpgradeTree(string refname, bool keepPistolUpgrades = false)`
    semantics so `NewTreeDef[refname]` decides whether the clone inherits pistol children
    (default `false` = clean modded tree).
- `AddNewWeaponUpgradeTreesToPlayer(instance, keepPistolUpgrades)`:
  - Guard when `weaponUpgradesList` is empty.
  - Name the clone `<ref>WeaponUpgrades`.
  - When `keepPistolUpgrades == false`, **clear the clone's children** that hold vanilla
    perks before (or while) populating from `NewWeaponUpgradeRegistry.NewWeaponUpgrades`.
  - Keep the 8-slot cap (`currentIndex < 8`) and only assign upgrades whose
    `ownerWeaponReference == activeRef`.
- Add `GetTreeForWeapon(weaponupgradescreen, string weaponRef)` returning the
  `<ref>WeaponUpgrades` entry in `weaponUpgradesList`, or null.

### 5. Persistence of unlocked perks

Perk *application* is vanilla (`unlockedWeaponPerksList` + stat changes); we do not
replicate it. Modded perk stat fields (`statName`, `change`, etc.) flow through
`weaponupgrade` fields already copied in `AddNewWeaponUpgradeTreesToPlayer`. No new
persistence is added in this change.

## Verification

- Build passes (`dotnet build DisfigureModApi.sln`, 0 errors).
- In-game with `TestMod`:
  1. Select the modded weapon, start a run, level up.
  2. Weapon-upgrade screen shows **only** TestWeapon's registered upgrades (no pistol
     perks) when no `RegisterNewWeaponUpgradeTree` opt-in is set.
  3. Choosing one applies its stat and adds its name to the side display.
  4. Vanilla weapons still show their normal trees.

## Files touched

- `docs/superpowers/specs/2026-08-19-weapon-perks-design.md` (this doc)
- `Modules/Patchers.cs` — rework `WeaponUpgradeScreenAwake` + `WeaponUpgradeScreenOnUpgrade`
- `Modules/Content/NewWeaponUpgrade.cs` — tree lookup + clean-tree population + registry
  semantics