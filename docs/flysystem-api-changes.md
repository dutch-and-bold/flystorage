# Flysystem API Changes

Because Flystorage is an open-minded port of Flysystem which was written in PHP.
We have made some decisions to change the API where needed to appreciate the full power of C#.

All changes made to the Flysystem API are noted below.

## Generic changes

|Change                                           |Reason                                   |
|-------------------------------------------------|-----------------------------------------|
|Interface names now start with `I`               |To comply with Microsoft's naming rules  |
|Exception names now end with `Exception`         |To comply with Microsoft's naming rules  |
|Timestamp is changed to `DatetimeOffset`         |Timestamps are not commonly used in .NET |

## FilesystemAdapter

|Change                                                                            |Reason                                                                                                                                        |
|----------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
|Read defaults to return a `Stream`                                                |Working with Streams are common and accepted in consecutive API's. Returning a string uses precious memory and might set the wrong example.   |
|`ReadString` and `WriteString` extension methods are added for keeping a clean API|There are still cases where you just don't need a Stream but a string, to keep the API readable this method is added for convenience.         |

## Visibility

|Change                                                    |Reason                                                                                     |
|----------------------------------------------------------|-------------------------------------------------------------------------------------------|
|`Visibility` constant `string` value is changed to `enum` |Static strings have understandably been chosen in PHP. But in C# an enum seems more suited.|

## FileAttributes

|Change                                             |Reason                                                                                     |
|---------------------------------------------------|-------------------------------------------------------------------------------------------|
|`FileSize` is changed from `int` to `long`         |A `Stream`'s length in C# is represented in a `long`.                                      |