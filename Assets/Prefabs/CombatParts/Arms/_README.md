`_EXAMPLEARM` is given as a base to copy.

Arm prefabs should be instanced directly on the CombatRobot gameobject.
The prefab root gameobject must have a component implementing ICombatArm.

The component can assume it is at the root of its matching prefab, so it can access the heirarchy of the prefab.
This lets you control hitboxes, ui, and other fun stuff.
Notably, the local coordinates of these things match the player.

To have a model in the prefab animate with the player,
the matching transforms can be copied from the player.
