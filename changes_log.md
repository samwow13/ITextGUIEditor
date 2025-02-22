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

## 2025-02-22
- Updated `TemplateWatcherService` to monitor both `.html` and `.cshtml` files for changes

## 2025-02-22
- Fixed `TemplateWatcherService` file filter pattern to properly watch `.html`, `.cshtml`, and `.css` files

## 2025-02-22
- Rewrote `TemplateWatcherService` to use separate watchers for each file type (`.html`, `.cshtml`, `.css`) to improve file change detection

## 2025-02-22
- Added "Close Edge on change" checkbox to MainForm next to the "Automatic Saving" checkbox
- Added "Close Edge on change" checkbox functionality to automatically close Edge windows when templates are reloaded

## 2025-02-22
- Added persistence for "Close Edge on change" setting in Windows Registry

## 2025-02-22
- Fixed "Close Edge on change" setting to properly load the saved preference when the application starts
