# Chanquo
Chanquo(pronounce chanko) is the partial implementation of Golang-channel for Unity.


## usage

### create channel then send data to channel

assume that the type like below is exists, 
```csharp
public struct T {}
```

create channel and send to the typed channel.
```csharp
var ch = Chan<T>.Make();
ch.Send(new T(){});
```

### receive data from channel

```csharp
// start receiving data asynchronously.
ch.Receive<T>(
    (T data, bool ok) => {
        // ok will become false when the channel is closed somewhere.
        // when ok is false, data is empty.
    }
);

or 

// receive data until Chan<T> is closed.
yield return Channels.For<T>(
    T t =>
    {
        
    }
);

or 

// wait first data then go through.
yield return Channels.WaitFirst<T>();

or 

// wait first data then go through and get received data.
var cor = Channels.WaitFirstResult<T>();
yield return cor;
var result = cor.Result;
```



### close channel

Close() closes channel. or you can close channel by specify type.

```csharp
ch.Close();

or

Channels.Close<T>();
```

when channel is closed, WaitFirst<T>(), WaitFirstResult<T>() and 


## license
[license](https://github.com/sassembla/Chanquo/blob/master/LICENSE)