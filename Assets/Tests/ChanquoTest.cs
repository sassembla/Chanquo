using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ChanquoCore;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class ChanquoTest
    {
        public class T : IChanquoBase
        {
            public string message;
            public void Something()
            {
                Debug.Log("呼ばれてる");
            }
        }

        [UnityTest]
        public IEnumerator SendAndReceive()
        {
            var c = Chanquo.New<T>();
            c.Send(
                new T()
                {
                    message = "message!"
                }
            );//c <- data;// 送信

            var r = Chanquo.Receive<T>();// 受信 r <- c(呼んだタイミングで溜まっているものを先頭だけpull)
            Assert.True(r.message == "message!");
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendAndSelect()
        {
            var c = Chanquo.New<T>();
            c.Send(
                new T()
                {
                    message = "selected!"
                }
            );//c <- data;// 送信

            var s = Chanquo.Select<T>(
                t =>
                {
                    Assert.True(t.message == "selected!");
                },
                ThreadMode.OnUpdate
            );// 受信 sはレシーバ。あるだけ読み出されるwhile文みたいなもの。

            yield return null;
        }

        [UnityTest]
        public IEnumerator ReceiveInOtherThread()
        {
            var c = Chanquo.New<T>();
            c.Send(
                new T()
                {
                    message = "selected!"
                }
            );//c <- data;// 送信

            var done = false;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    var s = Chanquo.Select<T>(
                        t =>
                        {
                            Assert.True(t.message == "selected!");
                            done = true;
                        },
                        ThreadMode.OnUpdate
                    );// 受信 sはレシーバ。あるだけ読み出
                }
            ));

            thread.Start();
            while (!done)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ReceiveMultiDataInOtherThread()
        {
            var c = Chanquo.New<T>();
            var dataCount = 10;
            for (var i = 0; i < dataCount; i++)
            {
                c.Send(
                    new T()
                    {
                        message = "selected!"
                    }
                );
            }

            var done = false;
            var receiveCount = 0;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    var s = Chanquo.Select<T>(
                        t =>
                        {
                            Assert.True(t.message == "selected!");
                            receiveCount++;
                            if (receiveCount == dataCount)
                            {
                                done = true;
                            }
                        },
                        ThreadMode.OnUpdate
                    );// 受信 sはレシーバ。あるだけ読み出
                }
            ));

            thread.Start();
            while (!done)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SendFromOtherThread()
        {
            var done = false;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    var c = Chanquo.New<T>();
                    c.Send(
                        new T()
                        {
                            message = "selected!"
                        }
                    );//c <- data;// 送信
                }
                )
            );

            var s = Chanquo.Select<T>(
                t =>
                {
                    Assert.True(t.message == "selected!");
                    done = true;
                },
                ThreadMode.OnUpdate
            );


            thread.Start();
            while (!done)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SendDisposeThenReceive()
        {
            var c = Chanquo.New<T>();
            c.Send(
                new T()
                {
                    message = "message!"
                }
            );//c <- data;// 送信
            c.Dispose();

            var r = Chanquo.Receive<T>();// 受信 r <- c(呼んだタイミングで溜まっているものを先頭だけpull)
            Assert.False(r?.message == "message!");
            r?.Something();
            Debug.Log("actual message:" + r?.message);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendDisposeThenSelect()
        {
            var c = Chanquo.New<T>();
            c.Send(
                new T()
                {
                    message = "selected!"
                }
            );//c <- data;// 送信
            c.Dispose();

            var done = false;

            var s = Chanquo.Select<T>(
                t =>
                {
                    Assert.True(true, "should never come here.");
                    done = true;
                },
                ThreadMode.OnUpdate
            );

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (DateTime.Now < waitTime)
                {
                    break;
                }
                yield return null;
            }
        }

        // sは独自のタイミングで呼び出される。が、on threadな仕掛けが欲しい。thread Aで呼ばれたい、みたいな。
        // とかやっておくと、データが流れる仕掛けが欲しい。インターバル指定、Last指定ができれば嬉しい        
        // selectをswitch式みたいに描きたい
        // s.Dispose();// selectの破棄
    }
}
