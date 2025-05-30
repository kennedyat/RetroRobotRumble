Hi Daniel! (or whoever)

Yeah, interfaces. It might be overkill if the project never needs to completely replace something.
If we know exactly what we want, we don't need abstraction.
But yeah the things we think we want aren't for certain.

We can also use C# delegates or Unity's Events, but are kinda awkward to reason about imo.

Some other GameObject will hold a reference to your GameObject with an `EquipScreen` component
(or whatever you want to rename it). We, in the present, know that `EquipScreen` component is there, so
we can just call `GetComponent<EquipScreen>()` and do stuff. But we don't know that `EquipScreen` will *always* be there.

The caller doesn't need to know too much about `EquipScreen` to do its job of opening the equipment ui.
We limit all it needs to know to `IOpenEquipScreen`. In the future, `EquipScreen` could be gone, but
its fine as long as theres something that implements `IOpenEquipScreen`.

I also have another function to check if the screen is still open.
I like actively polling things but that's my preference so asdfjkl;

-Ryan