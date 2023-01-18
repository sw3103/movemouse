# Introduction
Move Mouse is a simple piece of software that is designed to simulate user activity.

Originally designed to prevent Windows from locking the user session or going to sleep, Move Mouse can be deployed in a wide range of situations.
# Donate
Move Mouse will always be free, but if you would like to buy me a beer to show your appreciation, you can do so using the following link. Thanks!

https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QZTWHD9CRW5XN
# Installation
## Microsoft Store
The easiest way to install Move Mouse on Windows 10 or 11 is from the Microsoft Store using the link below.

https://www.microsoft.com/en-gb/p/move-mouse/9nq4ql59xlbf
### Pros
- Simple installation.
- Automatic updates.
### Cons
- Microsoft Store not normally available on work/corporate machines.
- CLI options unavailable.

## GitHub
Move Mouse may be open source, but you do not need to be a developer, or compile source code to use the GitHub version.

1. Vist the [Releases](https://github.com/sw3103/movemouse/releases/) page and locate the latest version.
2. Under the Assets section you will see three available downloads. Download the "move-mouse-x.x.x.zip" file, which contains a single "Move Mouse.exe" file.
3. Before you extract the ZIP file, you may wish to [Unblock](https://www.thewindowsclub.com/fix-windows-blocked-access-file) it to prevent the annoying security warnings.
4. Extract the ZIP file to your chosen location and run "Move Mouse.exe".
### Pros
- Custom installation path.
- CLI options.
- Portable (better option for work/corporate machines).
### Cons
- Manual updates.
# User Guide
## Getting Started
By default, Move Mouse is configured to move your mouse cursor every 30 seconds.

You can start this by clicking Move Mouse, at which point you will see the outer ring turn green, and the 30 second countdown begins.

<img src="docs/mm_green.png" width="200" height="200">

Once the countdown is complete, Move Mouse will perform the specified Actions (in this case, moving the mouse cursor).

Move Mouse will continue to repeat these Actions, unless you stop them by clicking once again.

You can also start/stop Move Mouse by double-clicking the system tray icon.

## Hover Buttons
You will quickly notice that Move Mouse will present you with some options when you hover your mouse cursor.

<img src="docs/mm_hover.png" width="200" height="200">

### Gear
Opens the Settings section where you can access the many different options to help you customise Move Mouse for your requirements.

### Envelope
Send feedback email to me.

### Question Mark
Brings you to this help site.

### Twitter
Takes you to Move Mouse's Twitter page.

### Cross
Closes Move Mouse.
## Status Colours
The outer ring on Move Mouse will change colour to reflect the current status.
### Idle (Blue)
Move Mouse is doing nothing, and will continue to do so until it is started from the main window, the system tray icon, or you have set a start Schedule.

<img src="docs/mm_blue.png" width="200" height="200">

### Running (Green)
Move Mouse is active and counting down towards performing your Actions.

<img src="docs/mm_green.png" width="200" height="200">

### Executing (Red)
Move Mouse is performing your Actions.

<img src="docs/mm_red.png" width="200" height="200">

### Paused (Yellow)
Move Mouse has automatically paused because it has detected user activity (this option needs to be enabled in Behaviour).

<img src="docs/mm_yellow.png" width="200" height="200">

### Sleeping (Purple)
Move Mouse is asleep because there is an active Blackout window in operation.

<img src="docs/mm_purple.png" width="200" height="200">

# Privacy Policy
## Personal Information
This application does not collect or transmit any user’s Personally Identifiable Information (PII). No personal information is used, stored, secured or disclosed by services this application works with.
## Technical Information
Limited technical information is sent (such as system date and time) but none of that is used or stored.

Move Mouse is able to access the local filesystem for the purpose of browsing for scripts and application files, although this information is not collected. 
## Statistical Information
A small set of non-identifiable information is sent to one or more of the organisations below for statistical information about app usage, device type and capabilities. Statistical service providers.

- Microsoft 
## Third Parties
If the app makes use of third party services, their usage of information is excluded from this privacy policy. You will be clearly made aware of the third parties involved in the app and we will ensure the very minimal set of data is set to those third parties. 
## Contact
If there are any questions regarding this privacy policy you may contact us using the information below.
- contact@movemouse.co.uk
- https://twitter.com/movemouse
