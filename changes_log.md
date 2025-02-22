# Changes Log

## 2025-02-22
- Initialized git repository
- Connected to remote repository (https://github.com/samwow13/ITextGUIEditor.git)
- Pulled existing content from master branch
- Created changes log file

## 2025-02-22
- Added `TemplateWatcherService` to automatically monitor changes in the Templates directory
- Modified `MainForm` to use the template watcher service
- Templates will now automatically reload when any HTML file in the Templates directory is modified, created, deleted, or renamed
