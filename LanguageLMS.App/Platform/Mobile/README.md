# Mobile Head

This folder is reserved for the mobile platform host.

The shared UI, view models, navigation, and API clients live in the root `LanguageLMS.App` project folders. When Android/iOS workloads are enabled, add the thin mobile bootstrap here and keep the same rule: the app talks to `LanguageLMS.Api` over HTTP only.
