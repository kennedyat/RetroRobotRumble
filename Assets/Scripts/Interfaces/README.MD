
Descriptions of how to use a thing, without knowing what the thing is.

You probably have a GameObject `gameObject` (configured through the inspector),
which you know some component implements `IInterface`.
You can't call `GetComponent<T>`, since you'd need to specify what type T exactly.
Instead you want the following:
```c#
gameObject.GetComponent(typeof(IInterface)) as IInterface
```
This gives you null or the interface.

Since we never said what type it is, other programmers can secretly replace it
with something else without needing to edit all the call sites.

However note that maybe an interface turns out to be insufficient.
We could directly update the interface, which requires messing with all the callers and implementers.
We can instead make a new interface, and slowly try to remove usage (deprecate) of the old one.
This is really annoying and redundant but less of an instant headache.
The `Obsolete` attribute helps here.

```c#
interface IInterface2 {
    long? DivideAndFloor(long a, long b);
}

[Obsolete("Use IInterface2 for bigger inputs, better name, and handling divide by zero")]
interface IInterface {
    int Divide(int a, int b);
}
```
