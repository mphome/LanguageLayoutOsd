
## **Problem Statement**

Modern Windows systems support **per-application keyboard layouts**, allowing each application to maintain its own input language. While this is powerful, it creates a usability issue specifically for users who **regularly work with multiple languages** (e.g., English, Ukrainian) and frequently switch between applications.

For such users, the active keyboard layout can change implicitly when switching focus (e.g., via Alt+Tab), depending on the last used language in each application. However, **Windows does not provide an immediate and clearly visible indication of the current input language at the moment of switching**.

The built-in language indicator in the taskbar:

* is **easy to miss during active work**
* requires **intentional visual checking**
* does not provide **instant feedback when context changes**

As a result, multilingual users often:

* start typing in the wrong language
* need to correct and retype text
* experience frequent micro-disruptions in their workflow

---

## **Core Problem**

> For users who actively use multiple input languages, the system lacks an immediate, attention-level feedback mechanism indicating the current keyboard layout during context switches and language changes.

---

## **Impact**

* Increased friction in bilingual/multilingual workflows
* Higher cognitive load due to constant context verification
* Frequent input errors and corrections
* Reduced overall typing efficiency

---

## **Desired Outcome**

Provide a **clear, transient, and non-intrusive visual indicator (OSD)** that:

* instantly shows the current input language
* is especially helpful for **multilingual users**
* removes the need to check the taskbar
* integrates seamlessly into fast switching workflows

---

<example>

A bilingual user switches between email (UK) and IDE (EN):

Without solution:
→ switches → types → realizes wrong language → fixes

With solution:
→ switches → instantly sees “EN” → continues typing correctly

</example>
