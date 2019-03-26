# Chanquo
Chanquo(pronounce chanko) is the partial implementation of Golang-channel for Unity.


## usage

### create channel then send data to channel

assume that the type like below is exists, 
```csharp
public class T : IChanquoBase
{
    public string message;
    public void Something()
    {
        Debug.Log("呼ばれてる");
    }
}
```

create channel via MakeChannel<T>(), then send new T instance to the channel.
```csharp
var c = Chanquo.MakeChannel<T>();
c.Send(
    new T()
    {
        message = "selected!"
    }
);//c <- data;// 送信
```

### receive data from channel

Receive<T> receives inputted T. "receive" receives only one data from channel.
```csharp
var r = Chanquo.Receive<T>();// 受信 r <- c(呼んだタイミングで溜まっているものを先頭だけpull)
Assert.True(r.message == "message!");
```


### select

the Select<T> sets handler for receiving T data. this method can set "when to receive" for the data.
```csharp
var s = Chanquo.Select<T>(
    (t, ok) =>
    {
        Assert.True(t.message == "selected!");
    },
    ThreadMode.OnUpdate
);
```

### react with channel-death


```csharp
s = Chanquo.Select<T,U>(
    (t, ok) =>
    {
        if (!t.Ok) {
            s.Dispose();
            return;
        }

        Assert.True(t.message == "selected!");
    },
    ThreadMode.OnUpdate
);
```

### select multiple data.
```csharp
var s = Chanquo.Select<T,U>(
    (t, ok) =>
    {
        // receive t.
    },
    (u, ok) => {
        // receive u.
    },
    ThreadMode.OnUpdate
);
```


## license
[license](https://github.com/sassembla/Chanquo/blob/master/LICENSE)