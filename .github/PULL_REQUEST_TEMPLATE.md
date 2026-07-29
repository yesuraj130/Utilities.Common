---
Title: Hardening ClickOnceHelper: async update API, safer watcher handling, robust Process.Start, and proper Dispose pattern

This PR contains a focused refactor and hardening of Utilities.Common/ClickOnceHelper.cs with the following goals:

- Provide a non-blocking, async update API: UpdateClickOnceAsync() that returns (UpdateResult, string)
- Keep a synchronous compatibility wrapper UpdateClickOnce(out string) that runs the async method on a background thread
- Move network and blocking ClickOnce calls (CheckForUpdate, Update) off the UI thread via Task.Run
- Make FileSystemWatcherExt usage safer: copy local reference to avoid races, guard EnableRaisingEvents and Dispose, and avoid exceptions escaping async void handlers
- Use ProcessStartInfo with UseShellExecute = true for more robust Process.Start behavior for both file and URL URIs
- Implement standard Dispose pattern with disposed flag, null guards, and a finalizer for defensive cleanup
- Harden GetProgramsPath with input validation and directory existence checks
- Keep public API backwards-compatible except for the added async API and UpdateResult enum

Notes:
- This change swallows some exceptions in surface methods to avoid crashing callers; if you'd prefer structured logging, we can inject an ILogger.
- ApplicationUpdated event may be raised from a background thread; if UI-thread invocation is required we can capture and use a SynchronizationContext.

Follow-ups:
- Add unit tests for GetProgramsPath, watcher lifecycle, and update scenarios (mocking ApplicationDeployment)
- Optionally inject ILogger and SynchronizationContext to improve diagnostics and thread-affinity of events

