FPS-Project
===========
FPS-Project is work name for First-person game that is aimed primarily on addons. This means that on initial release, the core will be as small as possible and built-in plugins should be easily toggleable. This can create interesting experience where you can experiment, modify, create unique maps etc. The most notable down-side is currently the thing that addons must be compiled to whole project, they can't be added seperately (not in roadmap to change this yet, sorry). This can change in future. Feel free to use this code anywhere you want, but please mention where did you get it, others might find it useful too.


#features
* Character
  * Wallrunning
  * Climbing
  * Jumping over obstacles (tbd)
  * Stats (health, armor threat) (script can be used on AI/Player)
  * Checkpoints (and support for timed runs)
* Weapon system (script can be used on AI/Player)
  * Physical/Raycast bullets
  * Should be usable on any type of weapon (including melee soon) 
* AI
  * Movement (using navmesh pathfinding)
  * Threats
  * Behaviors
    * Idle
    * Attack
      * Ranged
      * Melee (tbd)
    * Inquire (tbd)
    * Patrol (tbd)
      * Patrol path generation (planned) 
    * Flee (tbd)
    * Cover (planned)
    * Help (planned)
    * Follow (tbd)
    * Defend (planned)
    * Dead (planned)
* Phases (WIP)
* Dynamic level generation (tbd)
* Inventory system (planned)
* Multiplayer (tbd after Unity 5 release)

#updates
##α20-10-2014
Added and optimized models for melee and ranged weapons from asset
store. Further improved weapon system which can now make use of 3D
models. Still no news on when melee will be complete (It's not hard but
hardly interesting). United formating of some elements and you can
survive 10 bullets without armor instead of 4 (will be further tuned,
but this was needed due to machine guns that can shoot 10 bullets/s and
well.. you can count)
##α19-10-2014
bugfixes, weaponsSystem updates and AI downgrades. Inquire behavior is
just pain in the ass and will probably take awhile until I get it right.
Most games fail to do so, so I hope I'll be sucessfull with my ideas. I
think I'll make other behaviors before returning to inquire.
##α15-10-2014
One step closer to better weapons. The new system allows very easy creation of new weapons and adds support for melee weapons a possibly other weapon types in the future. It will also feature in next update dropping active weapon upon death. It still needs to be tuned but player can shoot (once again) already and believe, he's happy. yh and added new type CapacityOf to API and some AI behavior tweaks.
##α13-10-2014
Added beginning to behaviors and also checkpoint trigger particle animation is added to make it look more the way AIs like it. They are really dumb for now so don't blame them if you don't like it.
##α12-10-2014
AI is now smarter and can use pathfinding to track down its enemies and
on their way to the enemy they picked up a lot of mess so project is now
cleaner and smaller.
##initial
