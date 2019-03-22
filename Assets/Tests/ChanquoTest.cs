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
        private ChanquoChannel c;
        private ChanquoAction<T> s;


        [TearDown]
        public void Teardonw()
        {
            c?.Dispose();
            s?.Dispose();
            var s2 = GameObject.FindObjectOfType<ChanquoThreadRunner>();
            Assert.True(s2.update.Count == 0);
        }

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
            var message = "SendAndReceive" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信

            var r = Chanquo.Receive<T>();// 受信 r <- c(呼んだタイミングで溜まっているものを先頭だけpull)
            Assert.True(r.message == message);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendAndSelect()
        {
            var message = "SendAndSelect" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信

            s = Chanquo.Select<T>(
                (t, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    Assert.True(t.message == message);
                },
                ThreadMode.OnUpdate
            );// 受信 sはレシーバ。あるだけ読み出されるwhile文みたいなもの。

            s.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReceiveInOtherThread()
        {
            var message = "ReceiveInOtherThread" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信

            var done = false;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    s = Chanquo.Select<T>(
                        (t, ok) =>
                        {
                            if (!ok)
                            {
                                return;
                            }

                            Assert.True(t.message == message, "actual:" + t.message);
                            s.Dispose();
                            done = true;
                        },
                        ThreadMode.OnUpdate
                    );// 受信 sはレシーバ。あるだけ読み出
                }
            ));

            thread.Start();
            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    Debug.LogError("timeout");
                    break;
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator ReceiveMultiDataInOtherThread()
        {
            var message = "ReceiveMultiDataInOtherThread" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            var dataCount = 10;
            for (var i = 0; i < dataCount; i++)
            {
                c.Send(
                    new T()
                    {
                        message = message
                    }
                );
            }

            var done = false;
            var receiveCount = 0;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    s = Chanquo.Select<T>(
                        (t, ok) =>
                        {
                            if (!ok)
                            {
                                return;
                            }

                            Assert.True(t.message == message);
                            receiveCount++;
                            if (receiveCount == dataCount)
                            {
                                done = true;
                                s.Dispose();
                            }
                        },
                        ThreadMode.OnUpdate
                    );// 受信 sはレシーバ。あるだけ読み出
                }
            ));

            thread.Start();
            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    Debug.LogError("timeout");
                    break;
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SendFromOtherThread()
        {
            var message = "SendFromOtherThread" + Guid.NewGuid().ToString();
            var done = false;
            var thread = new Thread(new ThreadStart(
                () =>
                {
                    c = Chanquo.MakeChannel<T>();
                    c.Send(
                        new T()
                        {
                            message = message
                        }
                    );//c <- data;// 送信
                }
                )
            );

            s = Chanquo.Select<T>(
                (t, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    Assert.True(t.message == message);
                    s.Dispose();
                    done = true;
                },
                ThreadMode.OnUpdate
            );

            thread.Start();

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    Debug.LogError("timeou." + DateTime.Now.Millisecond);
                    break;
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator SendDisposeThenReceive()
        {
            var message = "SendDisposeThenReceive" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信
            c.Dispose();

            var r = Chanquo.Receive<T>();// 受信 r <- c(呼んだタイミングで溜まっているものを先頭だけpull)
            Assert.True(r == null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendDisposeThenSelect()
        {
            var message = "SendDisposeThenSelect" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信
            c.Dispose();

            var done = false;

            s = Chanquo.Select<T>(
                (t, ok) =>
                {
                    Assert.True(true, "should never come here.");
                },
                ThreadMode.OnUpdate
            );

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    // done.
                    break;
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator DisposeSelect()
        {
            var message = "DisposeSelect" + Guid.NewGuid().ToString();
            c = Chanquo.MakeChannel<T>();
            c.Send(
                new T()
                {
                    message = message
                }
            );//c <- data;// 送信

            var done = false;


            s = Chanquo.Select<T>(
                (t, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    Assert.True(t.message == message, "actual:" + t.message);
                    s.Dispose();// Selectの実行後
                    done = true;
                },
                ThreadMode.OnUpdate
            );

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    break;
                }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator DisposeSelectWhenChannelIsDisposed()
        {
            c = Chanquo.MakeChannel<T>();

            var done = false;


            s = Chanquo.Select<T>(
                (t, ok) =>
                {
                    if (!ok)
                    {
                        done = true;
                        s.Dispose();
                        return;
                    }
                    Debug.Log("t:" + t.message);
                },
                ThreadMode.OnUpdate
            );

            c.Dispose();


            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!done)
            {
                if (waitTime < DateTime.Now)
                {
                    Debug.LogError("timeout");
                    break;
                }
                yield return null;
            }
        }


        // sは独自のタイミングで呼び出される。が、on threadな仕掛けが欲しい。thread Aで呼ばれたい、みたいな。
        // とかやっておくと、データが流れる仕掛けが欲しい。インターバル指定、Last指定ができれば嬉しい        
        // selectをswitch式みたいに描きたい
    }
}
