# How Disfigure Handles Weapons — Reverse-Engineering Findings

Date: 2026-08-17
Source: Il2CppDumper output (`/home/jammingend/Documents/DisfigureModding/dump.cs`, `stringliteral.json`), BepInEx interop DLLs decompiled with ilspycmd.

> **Limitation**: `dump.cs` and the interop `Assembly-CSharp.dll` only contain method
> signatures/stubs. IL2CPP strips method bodies into native code, so the exact sequencing
> below is reconstructed from field layouts, method names, class relationships, and the
> FBPP string keys — not from readable IL.

---

## 1. Persistence layer: `FBPP` (FileBasedPrefs)

- Class: `public static class FBPP` — `dump.cs:679149`, TypeDefIndex 13037, Image 72 = **`FileBasedPrefs.dll`** (NOT Assembly-CSharp).
- Must call `FBPP.Start(FBPPConfig)` once before any get/set (`dump.cs:679164`).
- Relevant API (all public static, confirmed in interop):
  - `SetString(string key, string value = "")` / `GetString(string key, string defaultValue = "")`
  - `SetBool(string key, bool value = False)` / `GetBool(string key, bool defaultValue = False)`
  - `SetInt` / `GetInt`, `SetFloat` / `GetFloat`
  - `HasKey(key)`, `HasKeyForString/Int/Float/Bool`, `DeleteKey(key)`, `DeleteAll()`

## 2. Home screen scene (StartMenu)

### Weapon buttons = `weaponselect`
- Class: `public class weaponselect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler` — `dump.cs:197142`, TypeDefIndex 2375.
- One component per weapon button. Key fields (`dump.cs:197145-197170`):
  - `string weaponname` (0x18) — the weapon's identifier, set per-button in the scene
  - `GameObject startButton` (0x28), `GameObject purchaseButton` (0x38)
  - `Image image` (0x40), `GameObject selectionBorder` (0x48), `Image backgroundImage` (0x50)
  - `displayimagehandler dIH` (0x58) — the display/preview panel
  - `Color selectedColor` (0x78), `Color hoveringColor` (0x88), `bool isHovering` (0x98)
  - `string unlockedString` (0xA8) — the FBPP key used to check unlock state
  - `bool weaponIsUnlocked` (0xB0)
- Key methods (`dump.cs:197181-197215`):
  - `void selectWeapon()` — invoked when the button is clicked; persists selection + updates UI
  - `void showStartButton()`
  - `void ApplySelectionFromMenu()` / `void ApplySelectionFromMenuRestore()`
  - `void OnPointerEnter/OnPointerExit`
  - `static Transform GetStartPanelTransform()`
  - `void Start()`, `void Update()`, `void OnDisable()`
  - property `bool IsPistolLegacyRow`

### Weapon display/preview panel = `displayimagehandler`
- Class: `dump.cs:174672`, TypeDefIndex 1933.
- Fields:
  - `weaponselect selectedButton` (0x18)
  - `List<GameObject> weaponDisplays` (0x20) — display prefabs/clones per weapon
  - `GameObject currentDisplay` (0x28)
- Methods:
  - `void showChosenWeapon(string weaponname)` — called with the selected weapon's name
  - `void showNothing()`

### Weapon stat sliders = `weaponstatsliders`
- Class: `dump.cs:197202`, TypeDefIndex 2376.
- Fields: `bool useManualStats`, `Image gunImage`, `Image gunImageBG`, `Slider slider1..4`, `string unlockedString` (0x60).

### The menu controller = `StartMenu`
- Class: `dump.cs:192542`, TypeDefIndex 2277.
- Fields: `Image black` (0x18), `Animator anim` (0x20), `GameObject creditstext` (0x28), `GameObject selectionBorder` (0x30), `GameObject purchaseButton` (0x38), `GameObject startGameButton` (0x40).
- Methods:
  - `Start()`, `OnEnable()`, `OnDisable()`
  - `EnsurePurchaseButtonRef()`, `EnsureStartGameButtonVisible()`
  - `static weaponselect FindBaseWeaponRow()`
  - `IEnumerator ApplyDefaultWeaponSelectionAfterInit()` (coroutine, `d__12`)
  - `IEnumerator RestoreWeaponSelectionAfterInit(string weaponId)` (coroutine, `d__13`)
  - `ChooseMap(int num)`, `PlayGame(int difficulty)`, `IEnumerator Fading(int sceneNum)`

### Unlock state (vanilla)
- The game stores per-weapon unlock state as an **FBPP bool keyed `"<weaponname>Unlocked"`**.
- Confirmed keys in `stringliteral.json`: `pistolUnlocked`, `shotgunUnlocked`, `sniperUnlocked`, `greatswordUnlocked`, `knifeUnlocked`, `katanasUnlocked`, `handcannonUnlocked`, `repeaterUnlocked`, `minigunUnlocked`, `sawlauncherUnlocked`, `boomerangUnlocked`, `burstrifleUnlocked`, and the pattern `'_unlocked'`, `'Unlocked'`.
- `weaponstatsliders.unlockedString` and `weaponselect.unlockedString` hold that key; `weaponselect.weaponIsUnlocked` mirrors `FBPP.GetBool(unlockedString)`.

## 3. Persisted selection (the "travel" between scenes)

- The active weapon is saved as an **FBPP string key `"selectedWeapon"`** whose value is the weapon identifier (matches `weaponselect.weaponname`).
- Confirmed in `stringliteral.json`: `'selectedWeapon'`, `'weapon_use'`, `'profile_pref_weapon_use_'`, `'profile_pref_weapon_use_pistol'`, `'profile_pref_display_weapon_use'`.
- Also present: `'no weapon selected, defaulting to pistol'` — the fallback default is the pistol.

## 4. Match scene (ObjectPool → WeaponManager)

### `ObjectPool` (singleton)
- Class: `dump.cs:182888`, TypeDefIndex 2127, `public static ObjectPool instance`.
- Holds refs to the runtime singletons: `PlayerStats pS` (0x90), `PlayerMove pM` (0x98), `GameManager gM` (0xA8), `WeaponManager wM` (0xB0).
- Weapon-related fields:
  - `List<Projectile> weaponsList` (0x6E8) — projectile data per weapon
  - `List<GameObject> weaponsModelList` (0x6F8) — **held-weapon model prefabs** (the visible weapon in the player's hands), `[SerializeField]`
  - `GameObject selectedWeaponModel` (0xBD0) — the currently equipped model, `[HideInInspector] public`

### `WeaponManager`
- Class: `dump.cs:196145`, TypeDefIndex 2373.
- Fields:
  - `int currentWeapon` (0x2C8) — index of the active weapon
  - `GameObject weaponModels` (0x248) — parent transform the held models are parented to (`[SerializeField] private`, but exposed in interop)
  - `List<GameObject> weaponsList` (0x7A0)
  - `string weaponName` (0x7D8) — name of the active weapon
  - `bool isMelee` (0xB8), `bool isSaw` (0xB9)
- Method: `int GetCurrentWeapon()` (`dump.cs:197035`).

### How the held weapon appears
- The equipped model is instantiated from `ObjectPool.weaponsModelList[index]`, parented under `WeaponManager.weaponModels`, and assigned to `ObjectPool.selectedWeaponModel`.
- `WeaponManager.currentWeapon` is the index used to select the active weapon logic (fire patterns, projectile data from `ObjectPool.weaponsList`).
- `PlayerMove`/`PlayerStats` read the persisted choice (`selectedWeapon` FBPP key → weapon identifier string) at scene start.

## 5. Full flow summary

```
HOME SCREEN (StartMenu scene)
  weaponselect button (weaponname = "pistol" / "shotgun" / ...)
     |  StartMenu.ApplyDefaultWeaponSelectionAfterInit / RestoreWeaponSelectionAfterInit(weaponId)
     |  weaponIsUnlocked = FBPP.GetBool(weaponname + "Unlocked")
     v
  selectWeapon()  (button click)
     |  displayimagehandler.showChosenWeapon(weaponname)   -> shows display, enables startButton
     |  FBPP.SetString("selectedWeapon", weaponname)       -> persists across scenes
     v
  StartMenu.PlayGame(difficulty) -> StartMenu.Fading(sceneNum) -> load match scene

MATCH SCENE
  ObjectPool.Awake/Start  (instance = this)
     |  read selectedWeapon FBPP key -> weapon identifier
     |  selectedWeaponModel = Instantiate(weaponsModelList[index]) under WeaponManager.weaponModels
     v
  WeaponManager.currentWeapon = index; weaponName = identifier
  (fire logic reads ObjectPool.weaponsList[currentWeapon] Projectile data)
```

## 6. Key takeaways for the mod API

1. **A weapon's identity is its string `weaponname`** (e.g. `"pistol"`), used as:
   - the `weaponselect.weaponname` value on its button,
   - the suffix of the unlock key `"<name>Unlocked"`,
   - the value of the `"selectedWeapon"` FBPP key.
2. **Unlock** = `FBPP.SetBool(weaponname + "Unlocked", true)` (already-unlocked default `false`).
3. **Select** = `FBPP.SetString("selectedWeapon", weaponname)`.
4. **Read the current selection** = `FBPP.GetString("selectedWeapon")`.
5. **Held model** = `ObjectPool.instance.weaponsModelList[index]` (index ↔ `WeaponManager.currentWeapon` ↔ the weapon identity list order).
6. The home screen assigns weapons to buttons via `weaponselect.weaponname`; buttons that are not assigned a weapon show a locked/`"COMING SOON"` state and call `showStartButton()` only when a weapon is selected.
7. FBPP lives in **`FileBasedPrefs.dll`**, so the API needs a reference to that interop assembly to call `FBPP` directly.
