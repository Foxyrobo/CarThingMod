# CarThingMod

Spawns a car (no, you can't drive it) whenever the player swings the nail. 
Cars can also be manually spawned (default key is 'o')
This mod breaks a lot of stuff in the game!

Several different physics components, like velocity, mass, drag, etc. can be changed in the settings

The cars can be deleted by pressing 'p' (can be changed in settings)

The setting labeled "Use continuous collision?" refers to the collision detection mode.
By default this is off, meaning the cars use discrete collision. 
In english, this means cars can clip through walls if they are fast enough (that's with this setting off).
If this is turned on, cars can no longer do that. The only downside is this uses more processing power, 
so your game will start to lag sooner than it would with this off. Although in my testing, this only 
comes up if you have spawned a lot of cars.

--------
CUSTOM CARS

As of version 1.2.6.4, custom cars can be added! To do this, you must first go to your mods folder.

This can be found by going to your lumafly launcher, clicking on the mods tab at the top, then clicking
on "Open Mods Folder" on the left sidebar. From there, go to CarThingMod, then Cars. This folder is where
your custom cars go. When the game starts, a sample folder should automatically appear, showing you an 
example on how to make these cars!

Each custom car should have a .png file and a .txt file with the same name. The .png file is the texture
used, and the .txt file is the settings file. Formatting for this settings file can be found in the sample
folder.

Assuming everything is successful, each time a car is spawned, it will cycle through all custom cars.
If any custom cars are present, the default car will be skipped (the default car is the same as the sample).

--
But what if my car isn't showing up?
That means one of the following:
1. The settings file wasn't formatted properly.
2. There is no settings file for the car.
3. There is no image file for the car.
Check ModLog.txt for more info!

--------


Mod by Foxyrobo
