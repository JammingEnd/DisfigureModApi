# "More >>" Clear-Buttons Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a cloned "More >>" button to the home-screen weapon selection that clears all active vanilla `GunButton (x)` slots to free slots and assigns registered modded weapons to them.

**Architecture:** Extend the existing `StartMenu.OnEnable` postfix (`UIinteractorStart`) to (1) create a clone of the `Canvas/Start/back` button, re-labeled "More >>" and re-wired to a new `ClearGunButtons()` method, and (2) continue assigning modded weapons via an extracted, shared `AssignWeaponsToFreeSlots(Transform)` method. The old hardcoded slot whitelist (`IsAvaibleButton` 27/28/31/32) is replaced by a `FindGunButtons()` helper that matches the `GunButton` name prefix + active state.

**Tech Stack:** C# / .NET 6, BepInEx Unity IL2CPP modding, Harmony 2 patches, Il2CppInterop, Unity Engine UI. Reference interop DLLs are in `mnt/seagate/SteamLibraryD/steamsapps/common/Disfigure/BepInEx`.

**Testing note:** This is a game mod with no unit-test framework. Verification is `dotnet build DisfigureModApi.sln` (compile) per task plus a manual in-game runtime checklist (Task 6). Do not introduce a test framework.

**Spec:** `docs/superpowers/specs/2026-08-17-more-button-clear-design.md`

---

## File Structure

- Modify: `Modules/Util/Extentions.cs` — remove `IsAvaibleButton()`, add `FindGunButtons(this Transform)`.
- Modify: `Modules/UIinteractor.cs` — move slot-assignment bookkeeping to the outer `UIinteractor` class, extract `AssignWeaponsToFreeSlots(Transform)`, add MoreButton clone creation + `ClearGunButtons()`.

No new files.

---

### Task 1: Replace `IsAvaibleButton` with `FindGunButtons`

**Files:**
- Modify: `Modules/Util/Extentions.cs:58-71`

- [ ] **Step 1: Remove `IsAvaibleButton` and add `FindGunButtons`**

In `Modules/Util/Extentions.cs`, replace the `IsAvaibleButton` method (lines 58-71) with:

```csharp
/// <summary>
/// Returns the active child GameObjects of <paramref name="parent"/> whose name starts
/// with "GunButton" (e.g. "GunButton (27)"). Disabled/legacy buttons are excluded.
/// </summary>
public static List<GameObject> FindGunButtons(this Transform parent)
{
    List<GameObject> buttons = new();
    foreach (Transform child in parent)
    {
        if (child.gameObject.name.StartsWith("GunButton") && child.gameObject.activeInHierarchy)
        {
            buttons.Add(child.gameObject);
        }
    }
    return buttons;
}
```

`System.Collections.Generic` is already imported at the top of the file.

- [ ] **Step 2: Build the API project**

Run: `dotnet build DisfigureModApi.csproj`
Expected: **Build fails to compile** — `UIinteractor.cs` still calls the removed `IsAvaibleButton()`, which is fixed in Task 2. A failure here is expected and acceptable; the goal of this step is only to confirm `FindGunButtons` itself has no syntax errors. If the only errors are the `IsAvaibleButton` references, proceed.

- [ ] **Step 3: Commit**

```bash
git add Modules/Util/Extentions.cs
git commit -m "feat(ui): add FindGunButtons helper for weapon slots"
```

---

### Task 2: Extract shared slot-assignment logic

**Files:**
- Modify: `Modules/UIinteractor.cs:28-108`

Goal: the `OnEnable` assignment loop becomes a reusable method that both `OnEnable` and the future "More >>" click can call. Move the `assignedWeapons` bookkeeping into the outer `UIinteractor` class so both paths share it. The assign step must also re-apply the per-weapon visual state that `WeaponSelectPatchStart` normally stamps at `Start` (which won't re-fire for reassigned buttons), i.e. `selectedColor = Color.red`.

- [ ] **Step 1: Move bookkeeping to the outer class**

In `Modules/UIinteractor.cs`, inside `public class UIinteractor`, add after the `currentButtonName` field:

```csharp
private static readonly List<NewWeapon> assignedWeapons = new();

public static void ResetAssignments()
{
    assignedWeapons.Clear();
}

private static bool IsAssigned(NewWeapon weapon)
{
    return assignedWeapons.Contains(weapon);
}
```

- [ ] **Step 2: Extract `AssignWeaponsToFreeSlots`**

Add this method to the outer `UIinteractor` class:

```csharp
/// <summary>
/// Assigns unassigned registered modded weapons to free gun-button slots under
/// <paramref name="panel"/>. A slot is free when it has a <see cref="weaponselect"/>
/// that is not yet a modded weapon and whose label is "COMING SOON".
/// </summary>
public static void AssignWeaponsToFreeSlots(Transform panel)
{
    if (NewWeaponInitiator.newWeapons.Count == 0)
    {
        return;
    }

    foreach (GameObject button in panel.FindGunButtons())
    {
        weaponselect wpS = button.GetComponent<weaponselect>();
        if (wpS == null)
        {
            continue;
        }

        if (NewWeaponInitiator.GetWeapon(wpS.weaponname) != null)
        {
            // Already assigned to a modded weapon; keep it.
            continue;
        }

        Text textComp = button.transform.GetChild(0).GetComponent<Text>();
        if (textComp == null || textComp.text != "COMING SOON")
        {
            continue;
        }

        NewWeapon weapon = NewWeaponInitiator.newWeapons.Find(w => !IsAssigned(w));
        if (weapon == null)
        {
            return;
        }

        ModApi.Log.LogMessage("Assigning weapon: " + weapon.weaponName + " to slot " + button.name);
        textComp.text = weapon.weaponName;
        wpS.weaponname = weapon.weaponReference;
        wpS.unlockedString = weapon.UnlockKey;
        wpS.weaponIsUnlocked = weapon.IsUnlocked;
        wpS.selectedColor = Color.red;
        wpS.enabled = true;
        button.GetComponent<Button>().enabled = true;
        assignedWeapons.Add(weapon);
    }
}
```

- [ ] **Step 3: Rewrite `UIinteractorStart.Postfix` to use the shared method**

Replace the body of `UIinteractorStart.Postfix` (currently lines 37-85, the whole `foreach (Transform child in ...)` loop) with:

```csharp
public static void Postfix(StartMenu __instance)
{
    UIinteractor.AssignWeaponsToFreeSlots(__instance.gameObject.transform);
}
```

Delete the now-unused `assignedWeapons`, `ResetAssignments`, and `IsAssigned` members from `UIinteractorStart`.

- [ ] **Step 4: Update `UIinteractorStartDisable.Postfix`**

Replace its body with:

```csharp
public static void Postfix(StartMenu __instance)
{
    // Reset assignment bookkeeping so the menu can be rebuilt next time.
    UIinteractor.ResetAssignments();
}
```

- [ ] **Step 5: Build the API project**

Run: `dotnet build DisfigureModApi.csproj`
Expected: Build succeeded, 0 errors. The `IsAvaibleButton` reference is now gone (Task 1 removed the method), so this must compile.

- [ ] **Step 6: Commit**

```bash
git add Modules/UIinteractor.cs
git commit -m "refactor(ui): extract shared weapon slot assignment logic"
```

---

### Task 3: Create the "More >>" clone

**Files:**
- Modify: `Modules/UIinteractor.cs`

Goal: on `StartMenu.OnEnable`, create the "More >>" clone from `Canvas/Start/back` (if the reference is null / destroyed), before the assignment runs. Guard: the clone must be created even when there are zero registered modded weapons (the assignment's own guard handles that separately).

- [ ] **Step 1: Add the clone field and creation method to the outer class**

In `UIinteractor`, add after the `currentButtonName` field:

```csharp
private static GameObject moreButtonClone;

private static readonly Vector3 MoreButtonPosition = new Vector3(-59.9823f, -65, -53.6812f);
```

Then add the creation method:

```csharp
/// <summary>
/// Creates the "More >>" clone of the back button under Canvas/Start on first use.
/// No-op if the clone already exists or no active back button is found.
/// </summary>
public static void EnsureMoreButtonCreated()
{
    if (moreButtonClone != null)
    {
        return;
    }

    BackButton source = null;
    foreach (BackButton bb in UnityEngine.Object.FindObjectsOfType<BackButton>())
    {
        if (bb.gameObject.name == "back" && bb.gameObject.activeInHierarchy)
        {
            source = bb;
            break;
        }
    }
    if (source == null)
    {
        ModApi.Log.LogMessage("MoreButton: no active 'back' button found to clone.");
        return;
    }

    moreButtonClone = GameObject.Instantiate(source.gameObject, source.transform.parent);
    moreButtonClone.name = "MoreButton";
    moreButtonClone.transform.position = MoreButtonPosition;

    BackButton cloneBack = moreButtonClone.GetComponent<BackButton>();
    if (cloneBack != null)
    {
        cloneBack.enabled = false;
    }
    ButtonControl cloneControl = moreButtonClone.GetComponent<ButtonControl>();
    if (cloneControl != null)
    {
        cloneControl.enabled = false;
    }

    SetMoreButtonLabel(moreButtonClone);

    Button cloneButton = moreButtonClone.GetComponent<Button>();
    if (cloneButton != null)
    {
        cloneButton.onClick.RemoveAllListeners();
        cloneButton.onClick.AddListener(ClearGunButtons);
    }
    ModApi.Log.LogMessage("MoreButton created at " + moreButtonClone.transform.position);
}
```

> Note: `ButtonControl` (`dump.cs:173226`, TypeDefIndex 1888) is the game's own `MonoBehaviour` that the back button carries alongside `BackButton`; the user confirmed the button has it. If it is ever missing from the interop build, drop that block — it is defensive only.

- [ ] **Step 2: Identify the label Text child at runtime**

The spec warns the back button has multiple Text children, so do NOT hardcode a child index blindly. First add a one-off debug dump inside `EnsureMoreButtonCreated` (right after the clone is created) and run the game to see the hierarchy:

```csharp
// TEMPORARY: log every Text child of the clone to identify the label index.
foreach (Text text in moreButtonClone.GetComponentsInChildren<Text>(true))
{
    ModApi.Log.LogMessage($"MoreButton child '{text.gameObject.name}' text='{text.text}'");
}
```

Build, run the game, open weapon selection, and read the console. Identify the child that shows the back button's label (e.g. text "back" or "BACK"). Record its **hierarchy index** (`transform.GetSiblingIndex()`) or name.

- [ ] **Step 3: Write `SetMoreButtonLabel` using the identified child**

Replace the temporary dump with the final method, targeting the child found in Step 2:

```csharp
/// <summary>
/// Sets the back button's label child to "More >>". The target child was identified
/// at runtime (Task 3 Step 2); adjust LABEL_CHILD_INDEX if the hierarchy differs.
/// </summary>
private static void SetMoreButtonLabel(GameObject clone)
{
    Text label = clone.transform.GetChild(LABEL_CHILD_INDEX).GetComponent<Text>();
    if (label == null)
    {
        // Fall back: first non-empty Text child.
        foreach (Text text in clone.GetComponentsInChildren<Text>(true))
        {
            if (!string.IsNullOrEmpty(text.text))
            {
                label = text;
                break;
            }
        }
    }
    if (label != null)
    {
        label.text = "More >>";
    }
    else
    {
        ModApi.Log.LogMessage("MoreButton: no Text child found to label.");
    }
}
```

Where `LABEL_CHILD_INDEX` is a `private const int` set to the index found in Step 2. Keep the fallback so the button is still labeled even if the child index differs.

- [ ] **Step 4: Add `ResetMoreButton`**

```csharp
/// <summary>Destroys the clone and drops its reference so it is recreated fresh next time.</summary>
public static void ResetMoreButton()
{
    if (moreButtonClone != null)
    {
        GameObject.Destroy(moreButtonClone);
    }
    moreButtonClone = null;
}
```

- [ ] **Step 5: Call `EnsureMoreButtonCreated` from `UIinteractorStart.Postfix`**

```csharp
public static void Postfix(StartMenu __instance)
{
    UIinteractor.EnsureMoreButtonCreated();
    UIinteractor.AssignWeaponsToFreeSlots(__instance.gameObject.transform);
}
```

- [ ] **Step 6: Call `ResetMoreButton` from `UIinteractorStartDisable.Postfix`**

```csharp
public static void Postfix(StartMenu __instance)
{
    // Reset assignment bookkeeping so the menu can be rebuilt next time.
    UIinteractor.ResetAssignments();
    UIinteractor.ResetMoreButton();
}
```

> Rationale: after a match the menu scene unloads and Il2CppInterop's destroyed-object null check is unreliable, so the `OnDisable` hook explicitly destroys the clone and drops the reference.

- [ ] **Step 7: Build the API project**

Run: `dotnet build DisfigureModApi.csproj`
Expected: Build succeeded, 0 errors. (`ClearGunButtons` is not yet defined — Task 4 adds it. If you want a green build here, add a stub `ClearGunButtons()` that logs and returns, then replace it in Task 4.)

- [ ] **Step 8: Commit**

```bash
git add Modules/UIinteractor.cs
git commit -m "feat(ui): create 'More >>' clone of back button on menu show"
```

---

### Task 4: Implement `ClearGunButtons`

**Files:**
- Modify: `Modules/UIinteractor.cs`

Goal: the "More >>" click clears all active GunButton slots to a free state, then reassigns registered modded weapons. No-op when there are zero registered modded weapons (so the vanilla UI is never destroyed).

- [ ] **Step 1: Add `SnapshotDefaultButtonColor`**

```csharp
/// <summary>
/// Captures the selectedColor of the first GunButton that is not holding a modded
/// weapon — the "vanilla" default — so cleared slots can be restored to it
/// (weaponselect.Start won't re-fire). Skips slots already stamped by the
/// assignment logic (those carry the modded red color).
/// </summary>
private static Color SnapshotDefaultButtonColor(Transform panel)
{
    foreach (GameObject button in panel.FindGunButtons())
    {
        weaponselect wpS = button.GetComponent<weaponselect>();
        if (wpS == null)
        {
            continue;
        }
        if (NewWeaponInitiator.GetWeapon(wpS.weaponname) != null)
        {
            // Holds a modded weapon; its color is the modded red, not vanilla.
            continue;
        }
        return wpS.selectedColor;
    }
    return Color.white;
}
```

- [ ] **Step 2: Add `ClearGunButtons`**

```csharp
/// <summary>
/// Clears every active GunButton under the More button's parent panel back to a free
/// slot ("COMING SOON", no weapon), then reassigns registered modded weapons.
/// No-op when no modded weapons are registered.
/// </summary>
public static void ClearGunButtons()
{
    if (NewWeaponInitiator.newWeapons.Count == 0 || moreButtonClone == null)
    {
        return;
    }

    Transform panel = moreButtonClone.transform.parent;
    if (panel == null)
    {
        return;
    }

    Color defaultColor = SnapshotDefaultButtonColor(panel);

    foreach (GameObject button in panel.FindGunButtons())
    {
        weaponselect wpS = button.GetComponent<weaponselect>();
        if (wpS == null)
        {
            continue;
        }

        Text textComp = button.transform.GetChild(0).GetComponent<Text>();
        if (textComp != null)
        {
            textComp.text = "COMING SOON";
        }

        wpS.weaponname = "";
        wpS.unlockedString = "";
        wpS.weaponIsUnlocked = false;
        wpS.selectedColor = defaultColor;
        wpS.enabled = true;
        button.GetComponent<Button>().enabled = true;
    }

    ModApi.Log.LogMessage("MoreButton: cleared all gun buttons.");
    ResetAssignments();
    AssignWeaponsToFreeSlots(panel);
}
```

- [ ] **Step 3: Build the API project**

Run: `dotnet build DisfigureModApi.csproj`
Expected: Build succeeded, 0 errors.

- [ ] **Step 4: Commit**

```bash
git add Modules/UIinteractor.cs
git commit -m "feat(ui): add clear-gun-buttons action for More button"
```

---

### Task 5: Build and deploy both plugins

**Files:**
- Build: `DisfigureModApi.sln`

- [ ] **Step 1: Build the solution**

Run: `dotnet build DisfigureModApi.sln`
Expected: Build succeeded, 0 errors. The post-build targets copy the DLLs to `.../BepInEx/plugins/ModApi` and `.../BepInEx/plugins/TestMod`.

- [ ] **Step 2: Confirm both DLLs were deployed**

```bash
ls -la ../../../../mnt/seagate/SteamLibraryD/steamapps/common/Disfigure/BepInEx/plugins/ModApi
ls -la ../../../../mnt/seagate/SteamLibraryD/steamapps/common/Disfigure/BepInEx/plugins/TestMod
```

Expected: `DisfigureModApi.dll` and `TestMod.dll` present with fresh timestamps.

- [ ] **Step 3: Commit any remaining changes**

```bash
git add -A
git commit -m "chore: build artifacts after More button feature"
```

---

### Task 6: Runtime verification checklist

No file changes — manual testing in the game.

- [ ] **Step 1: Launch the game with BepInEx console**

Open the game. In the BepInEx console, confirm no exceptions from the API on load.

- [ ] **Step 2: Verify clone creation**

Press Play in the main menu to show the weapon selection. Expected log lines:
- `MoreButton created at (-59.98, -65.00, -53.68)`
- `Assigning weapon: Test Weapon to slot GunButton (27)` (and any other free slots)

Expected visual: a button labeled `More >>` at the back button's row position `(-59.9823, -65, -53.6812)`.

- [ ] **Step 3: Verify the clone does not break the back button**

The original `back` button still returns to the previous menu when clicked; the "More >>" clone does nothing when pressed outside its assigned behavior (its `BackButton`/`ButtonControl` are disabled and its `onClick` was rewired).

- [ ] **Step 4: Verify the clear action**

Click `More >>`. Expected log line: `MoreButton: cleared all gun buttons.` Expected visual:
- All active `GunButton (x)` labels reset to `COMING SOON`.
- Registered modded weapons (e.g. `Test Weapon`) reassigned to the first free slots, label + red selection color applied.

- [ ] **Step 5: Verify disabled/legacy buttons are untouched**

Buttons named `GunButton (x)` that are not active in hierarchy remain disabled and unchanged.

- [ ] **Step 6: Verify zero-weapon safety**

Temporarily comment out `NewWeaponInitiator.AddWeapon(...)` in `TestMod/TestPlugin.cs`, rebuild/deploy, and relaunch. Clicking `More >>` must leave the UI unchanged (no-op) and produce no `cleared all gun buttons` log. Re-add the weapon registration afterward.

- [ ] **Step 7: Verify scene-switch recreation**

Start a match (weapon in hand appears), return to the main menu, open weapon selection. The `More >>` button must be recreated (`MoreButton created at ...` in the log) because `StartMenu.OnDisable` destroyed the old clone.

- [ ] **Step 8: Verify within-scene reopen (no duplicate clones)**

From weapon selection, go back to the previous menu, then reopen weapon selection. Expected: exactly one `MoreButton` exists — `ResetMoreButton()` destroyed the previous clone on disable, and `EnsureMoreButtonCreated` created exactly one fresh one. If two "More >>" buttons overlap, the reset path is broken.

- [ ] **Step 9: Report results**

If any step fails, capture the BepInEx console output and report back with the log before proceeding.
