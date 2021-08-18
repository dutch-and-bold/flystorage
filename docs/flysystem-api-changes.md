# Flysystem API Changes

Because Flystorage is an open-minded port of Flysystem which was written in PHP.
We have made some decisions to change the API where needed to appreciate the full power of C#.

The changes made to the Flysystem are noted down on this page as much as possible.

## Generic changes

|Change                                       |Reason                                   |
|---------------------------------------------|-----------------------------------------|
|Interface names start with `I`               |To comply with Microsoft's naming rules  |
|Exception names end with `Exception`         |To comply with Microsoft's naming rules  |
|Timestamp is changed to DatetimeOffset       |Timestamps are not commonly used in .NET |

## FilesystemAdapter

|Change                                    |Reason                                                                                                                                     |
|------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
|Read defaults to return a `Stream`        |Working with Streams are common and accepted in consecutive API's. Returning a string uses precious memory and might set the wrong example.|
|ReadString is added for keeping a nice API|There are still cases where you don't need a Stream but just string, to keep the API readable this method is added for convenience       |

## Visibility

|Change                                           |Reason                                                                                     |
|-------------------------------------------------|-------------------------------------------------------------------------------------------|
|Visibility constant string value is now an enum  |Static strings have understandably been chosen in PHP. But in C# an enum seems more suited.|