# Unity-UNET-RTS

![](https://github.com/tommai78101/Unity-UNET-RTS/raw/testing/demo.gif)

[Playable demo.](http://tom-mai78101.itch.io/multiplier)

## Disclaimer:

As of October 23, 2015, the project has been rewritten from scratch to migrate to the new Unity Networking API (UNET), thus deprecating the old Unity Networking API. This means the old project is therefore discontinued and would not be favorable, since Unity 5 is continuously evolving.

Since this prototype was designed for network multiplayer RTS at the start of the development, it was intentionally designed so that no two players are using the same computer. If you are playing alone, you can open two webpages together. Please don't worry about Player 2 for now, and continue in the Start section below.

Unity Web Player does not support high DPI displays or dual monitors setup. But this may change for Unity 5.3 and up, now that it has support for dual monitors. At the moment, it is currently not supported.

**As of March 6, 2016, Multiplier is now the official game title**, because I'm not talented in coming up with creative names. If you happen to know what name to give, please contact me at any time.

## Introduction:

Multiplier, a simplistic real time strategy game, is based on math equations. Given any math equations, your unit depends on the results of said equations, and you can play to see how well your units are.

In other words, the game is simplified into only cloning, merging, and killing. You create clones, you merge, and you kill the other players. Future plans may include cloning based on other simple math equations, such as the Fibonacci Sequence (1, 1, 2, 3, 5, 8, 13, ...), or y = log(x), where x >= 1. 

In this build, you are able to create your own math expressions. As of October 23, 2015, the following simple arithmetic equations are supported:

Addition (e.g., 1+3)  
Subtraction (e.g., 10-6)  
Multiplication (e.g., 2*13)  
Division (e.g., 14/7)  
Power (e.g., 3^4)  
Random (e.g., 5+r)  

Future math expressions may be supported.

[Trello Scrum board can be found here.](https://trello.com/b/VQIT6pl9/master-s-project) This shows the current progression and future plans of the project. It is always updated.

## Start:

You may play against yourself, or have someone to join you on another computer. If you are playing with yourself, you must also do the Player 2 instructions.

Player 1 is the host of the game. Player 2 is the client of the game.

Player 1 instructions (Host):   
* Press "Enable Match Maker".
* In the input field, give your game session a name. For example, "default".
* Press "Create Internet Match".
* Wait for anyone to join. Game should start immediately afterwards.

Player 2 instructions (Client):    
* Press "Enable Match Maker".   
* Press "Find Internet Match".  
* Near the top, if the game has found an internet match, it will display, "Join Match: [Game Session Name]". Press any one of them to join the game.   
* Do not press "Client Ready", until your game finishes initializing. The client will automatically take care of this for you.  
* Both players' games are now ready, and you're all set.   
* Once both players are in the game, both players will start off with 1 unit each. Please continue to read Controls section for how to play.  

## Controls (For Multiplayer):

* **Left Mouse Button** - Select units. (You can only select your own units.) Hold down button and drag to make selection box.   
* **Right Mouse Button** - Move to Location.   
* **S** - Split. Split into two units. Note that there's a cooldown of 10 seconds.      
* **D** - Merge. Merge any two units together.   
* **M** (In-game only) - Toggle Statistics UI.   
* **` (Tilde)** - Bring up the console. (Only when you're hosting or you're in the game)   

Units will attack automatically when it sees an enemy unit nearby within its line of sight.

Note: Singleplayer controls are very similar.

## Changelog:

**Known Issue:** There's a lack of polish in this build. This will definitely change.   
**Known Issue:** Multiplayer Mode and Singleplayer Mode may have hidden bugs due to game engine rewrite.   
**Known Issue:** TODO messages are scattered through the scenes. They are there to remind me of what to do, and will be removed once the scenes are completed.   

### - v0.4-unet:

* Polished the UIs further.   
* Added Simulation Mode. Have fun with it.   
* Fixed some bugs related to multiplayer not updating attributes correctly.   
* Added final touches to the multiplayer.   

### - v0.3-unet:

* Polished the UIs. The menus are now consistent throughout the game.  
* Added Singleplayer Mode.  
* Rewritten Multiplayer Mode. Note that there may be hidden bugs I didn't find while rewriting Multiplayer Mode.  
* Now supports logging your statistics per game sessions.  
* Tutorial Mode is added.  
* Credits is added.  
* Resolved a game design issue where players are now able to prevent themselves from entering a deadlock situation of not able to create more units.  

### - v0.2.3-unet:

* Attribute Presets are now available in single player mode.  
* Attributes Panel UI can now update and work as intended.  
* Added Increase/Decrease to Leveling Rates, allowing players to see relative difference between two levels.  
* Incomplete Attribute Panel changes for custom A.I. player.  
* Fixed issue where parsing negative numbers may result in error.  
* Temporarily turned off Camera Zooming for now.  

### - v0.2.2-unet:

* Forgot to tweak the Singleplayer AI player. Now it's fixed.   

### - v0.2.1-unet:

* Fixed some AI player unit logic.   

### - v0.2-unet:

* Added primitive AI player into the game. You cannot modify the AI player yet.  
* Added singleplayer mode.  
* Added main menu.  
* Incomplete tutorial mode and credits.  
* Incomplete singleplayer UI.  
* Incomplete multiplayer UI.  

### - v0.1.1-unet:   

* Fixed selection box affecting minimap camera panning if dragged from playing field to minimap.   
* Fixed splitting/merging incorrectly assuming units were already finished splitting/merging, and are in idle state.   

### - v0.1-unet:

* Added team colors.   
* Added minimap.   
* Added new unit attribute : Attack Cooldown.   
* Expanded map size.   
* Fleshed out camera panning using minimap.   
* Can now order units using minimap.   
* Can now connect with other players via internet.   
* Fixed issue with unit attributes not being consistent.   
* Fixed issue with GUI not syncing across the network.   
* Fixed issue with unit attributes not being affected by the math equations.    
* Fixed issue with GUI hotkeys where by pressing X to enter a math formula, you will disconnect.   
* Fixed math expression on parsing decimals.   
* Fixed math expression on parsing double parentheses.     
* Fixed units spazzing out when initiating splitting / merging across network.    
* Tweaked UI positions, so camera panning will not be affected when mouse is clicking on the UI.   

### - v0.01 - rewritten:

* Old prototype has been swapped out in favor of the new prototype.   
* Lack of polish to be found, because it's new.   
* Ability to use math expressions to give units their own attributes.   
* Units have now been changed to capsule - shaped.   
* Game rules have not changed.   

### - v0.07:

* Fixed splitting not working in the tutorial.   
* Added health bar to the units in the tutorial.   
* Explained what the health bar does in the tutorial.   
* Combined "Attack" and "Move" together in the tutorial.   

### - v0.06a:

* Hotfix: Mouse isn't getting focus.

### - v0.06 :

* New Tutorial mode added in for new players to try and test.

### - v0.05 :

* Added simple analytic data that will show up on the screen when the game session is over.

### - v0.04 :

* Added ability for units to heal over time.

### - v0.03 :

* Fixed splitting units creating glitchy split animation on the client side.   
* Streamlined merging and splitting for each units.   

### - v0.02 :

* Fixed merging may go out of sync.May introduce new bugs.   
* Fixed scaling issues related to scaling exponentially and not incrementally.   
* Tweaked selection so that players can now select units without any hassles.   

### - v0.01b :

* Fixed being able to merge with other units of different levels.    
* Fixed a few bugs.   
* Added prototype number.Numbering scheme is as follows : [Game Build Type] - v[Big Release].[Small Release][Minor Fixes]   

### - v0.01a :

* Fixed issue with inconsistent dividing behavior.

### - v0.01 - minus :

* Reverted back to previous version due to some changes causing inconsistent behaviors.

### - v0.01 :

* Fixed issue such that the player cannot see if their units are taking damage.   
* Decrease max health point.   
* Decrease spawning time   

## Credits :

**Creator :** Thompson Lee