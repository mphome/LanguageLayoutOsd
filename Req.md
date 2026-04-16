---

# **Language Layout OSD Utility — Requirements**

## **1. Overview**

A lightweight Windows utility that displays a temporary on-screen indicator (OSD) showing the current keyboard input language (e.g., `EN`, `RU`) when it changes.

The application is designed for users who use **per-application keyboard layouts** and frequently switch between windows.

---

## **2. Target Environment**

* OS: Windows 11 (primary), Windows 10 (compatible)
* Runtime: .NET Framework 4.5
* UI framework: WinForms
* Deployment:

  * Single EXE
  * No installer required
  * No admin privileges required

---

## **3. Functional Requirements**

### **3.1 Language Detection**

The application must:

* Detect current keyboard layout of the **foreground window**

* Use Win32 APIs:

  * `GetForegroundWindow`
  * `GetWindowThreadProcessId`
  * `GetKeyboardLayout`

* Extract language ID:

  * `0x0409` → EN
  * `0x0419` → RU
  * fallback: hex code

---

### **3.2 Change Detection**

* Poll current layout every **100–200 ms**
* Compare with last known layout
* Trigger OSD only when layout **changes**

Must detect:

* manual switching (CapsLock / Alt+Shift)
* switching between applications (per-app layout)

---

### **3.3 OSD Display**

Display a temporary overlay when layout changes.

#### Requirements:

* Text: `EN`, `RU`, etc.
* Duration: configurable (default: 500 ms)
* Position: center of screen (primary monitor)
* Always on top

#### Visual:

* Borderless window
* Semi-transparent background
* Large font (e.g., 32–48 pt)
* High contrast text

---

### **3.4 Window Behavior**

The OSD window must:

* NOT receive focus
* NOT appear in Alt+Tab
* NOT appear in taskbar
* NOT block input
* Close automatically after timeout

---

## **4. Non-Functional Requirements**

### **4.1 Performance**

* CPU usage: negligible (<1%)
* Memory: <30 MB
* No UI thread blocking

---

### **4.2 Stability**

* Must not crash if:

  * foreground window changes rapidly
  * layout cannot be resolved
* Must handle null/invalid handles safely

---

### **4.3 UX Constraints**

* No flickering
* No multiple overlapping OSD windows
* If layout changes rapidly → restart timer instead of stacking windows

---

## **5. Technical Requirements**

### **5.1 Win32 Interop**

Required P/Invoke:

* `GetForegroundWindow`
* `GetWindowThreadProcessId`
* `GetKeyboardLayout`

Optional:

* multi-monitor support (`Screen.AllScreens`)

---

### **5.2 WinForms Window Configuration**

The OSD form must use:

* `FormBorderStyle = None`
* `TopMost = true`
* `ShowInTaskbar = false`
* `StartPosition = Manual`
* `Opacity` or layered window

Extended styles (via override):

* `WS_EX_TOOLWINDOW`
* `WS_EX_TOPMOST`
* `WS_EX_NOACTIVATE`

---

### **5.3 Threading Model**

* Main loop:

  * background thread OR timer
* UI:

  * must run on UI thread
* Use `Invoke` for UI updates

---

## **6. Configuration (optional but recommended)**

Provide simple config:

* display duration (ms)
* font size
* colors:

  * EN → neutral (gray/white)
  * RU → accent (orange/red)

Can be:

* hardcoded
* or simple JSON рядом с EXE

---

## **7. Extensibility (optional)**

Future features:

* per-language color mapping
* position near caret
* fade in/out animation
* tray icon with enable/disable
* startup with Windows

---

## **8. Out of Scope**

* No WPF
* No UWP / Windows App SDK
* No installer
* No system-level hooks
* No registry modification
* No cursor manipulation

---

## **9. Acceptance Criteria**

The application is considered complete if:

1. Switching language triggers OSD within ≤200 ms
2. Switching between apps shows correct language
3. No focus stealing occurs
4. No visible flicker
5. Works reliably for continuous use

---

## **10. Minimal Architecture**

```
Program.cs
 ├── LayoutMonitor (polling loop)
 ├── LayoutService (Win32 calls)
 └── OsdForm (WinForms overlay)
```

---

## **11. Example Flow**

1. User presses CapsLock → layout changes
2. Poll detects new HKL
3. Layout differs from previous
4. OSD form shown (`RU`)
5. Timer closes form after 500 ms

---

## **Final Note**

This solution intentionally avoids:

* WPF
* Windows notifications
* system hacks

and uses the **only reliable Windows-native approach**:

> lightweight topmost WinForms overlay + layout polling

