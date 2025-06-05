
Logicless containers of data. Usually ScriptableObjects or struct types with Serializable.
Other parts of the code interpret this to do behavior.

Structs are notably **never** null. Use them where they make sense!
They also appear as dropdowns in the inspector!

Note that removing fields is dangerous and may cause data loss!
If you move a field between structs, that's still a removal.
If you rename a field, that's also a removal.

If you're not sure where to put something or what to name it,
name test/temporary fields as such. You *can* commit them.
Also respect other people's test fields by not building on top of them.
