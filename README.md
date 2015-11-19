# Unity-UNET-RTS

# Disclaimer:
As of October 23, 2015, the project has been rewritten from scratch to migrate to the new Unity Networking API (UNET), thus deprecating the old Unity Networking API. This means the old project is therefore discontinued and would not be favorable, since Unity 5 is continuously evolving. Also, since the new project now uses System.Data.dll, I'm still looking for ways on exporting it to the Web Player. At the moment, it cannot be played online.

Since this prototype is all about network multiplayer RTS, I intentionally designed it so that no two players are using the same computer. If you are playing alone, you can open two webpages together. Please don't worry about Player 2 for now, and continue in the Start section below.

I have no idea if it's possible to host server games that are hosted online. Let me know anything.

This is a continuation effort of an older project, [Unity-RTS-Tweak](https://github.com/tommai78101/Unity-RTS-Tweak), but rewritten from scratch for Unity 5.2 UNET and up.

# Branches

*Master:* Stable codes only.  
*Testing:* Frequently updated. May break game.

# Introduction:

Multiplier, a simplistic real time strategy game, is based on the math equation, y=2x. In other words, there's only cloning and kill. You create clones, you merge, and you kill the other players. Future plans may include cloning based on other simple math equations, such as the Fibonacci Sequence (1, 1, 2, 3, 5, 8, 13, ...), or:
    
    y = log(x), where x >= 1.

In this build, you are able to create your own math expressions. As of October 23, 2015, the following simple arithmetic equations are supported:

* Addition (e.g., 1+3)
* Subtraction (e.g., 10-6)
* Multiplication (e.g., 2*13)
* Division (e.g., 14/7)
* Power (e.g., 3^4)
* Random (e.g., 5+r)


*Note: Future math expressions may be supported.

Trello Scrum board can be found [here](https://trello.com/b/VQIT6pl9/master-s-project). This shows the current progression and future plans of the project.

# Start:

You may play against yourself, or have someone to join you on another computer. If you are playing with yourself, you must also do the Player 2 instructions.

Player 1 is the host of the game. Player 2 is the client of the game.

### Player 1 instructions (Host):

1. Press "Host New Server".
2. Once you are in the game, you must wait until Player 2 joins the game.

### Player 2 instructions (Client):

1. When you know the host is currently hosting a new server, press "Find Servers".
2. Wait for 1 second.
3. There should be at least 1 server found. If not, then it means your computer could not locate the host server.
4. Click on the server titled, "Unity RTS Prototype".
5. Once both players are in the game, both players will start off with 1 unit each. 


Please continue to read Controls section for how to play.

### Controls:
* Left Mouse Button - Select units. (You can only select your own units.) Hold down button and drag to make selection box.
* Right Mouse Button - Move to Location.
* A - Attack where? (Right click to choose location)
* S - Split. Split into two units. Note that there's a cooldown of 10 seconds.
* D - Merge. Merge any two units together.
* ` (Tilde) - Bring up the console. (Only when you're hosting or you're in the game)

# Changelog:

* Known Issue: There's a lack of polish in this build. This will definitely change



**- v0.1-unet:**

* Added team colors.
* Added minimap.
* Added new unit attribute: Attack Cooldown.
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
* Fixed units spazzing out when initiating splitting/merging across network.
* Tweaked UI positions, so camera panning will not be affected when mouse is clicking on the UI.

**- v0.01-rewritten:**

* Old prototype has been swapped out in favor of the new prototype.
* Lack of polish to be found, because it's new.
* Ability to use math expressions to give units their own attributes.
* Units have now been changed to capsule-shaped.
* Game rules have not changed.

**- v0.07:**

* Fixed splitting not working in the tutorial.
* Added health bar to the units in the tutorial.
* Explained what the health bar does in the tutorial.
* Combined "Attack" and "Move" together in the tutorial.

**- v0.06a:**

* Hotfix: Mouse isn't getting focus.

**- v0.06:**

* New Tutorial mode added in for new players to try and test.

**- v0.05:**

* Added simple analytic data that will show up on the screen when the game session is over.

**- v0.04:**

Added ability for units to heal over time.

**- v0.03:**

* Fixed splitting units creating glitchy split animation on the client side.
* Streamlined merging and splitting for each units.

**- v0.02:**

* Fixed merging may go out of sync. May introduce new bugs.
* Fixed scaling issues related to scaling exponentially and not incrementally. 	
* Tweaked selection so that players can now select units without any hassles.

**- v0.01b:**

* Fixed being able to merge with other units of different levels.
* Fixed a few bugs.
* Added prototype number. Numbering scheme is as follows:

      [Game Build Type] - v[Big Release].[Small Release][Minor Fixes]

**- v0.01a:**

* Fixed issue with inconsistent dividing behavior.

**- v0.01-minus:**

* Reverted back to previous version due to some changes causing inconsistent behaviors.

**- v0.01:**

* Fixed issue such that the player cannot see if their units are taking damage.
* Decrease max health point.
* Decrease spawning time

# Credits:

**Creator:** Thompson Lee