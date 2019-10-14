using System;
using System.Collections;
using System.Threading;
using Chanquo.v2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public struct T
    {
        public string id;
        public T(bool a)
        {
            id = Guid.NewGuid().ToString();
        }
    }

    public class Chanquo2Test
    {
        [UnityTest]
        public IEnumerator SendThenWaitUntilReveive()
        {
            var done = false;

            var ch = Chan<T>.Make();
            ch.Send(new T(true));
            ch.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    Assert.NotNull(a.id);
                    done = true;
                }
            );

            while (!done)
            {
                yield return null;
            }

            ch.Close();
        }

        [UnityTest]
        public IEnumerator SendThenWaitUntilReveive2()
        {
            var done = false;

            var ch = Chan<T>.Make();
            ch.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    Assert.NotNull(a.id);
                    done = true;
                }
            );
            ch.Send(new T(true));

            while (!done)
            {
                yield return null;
            }

            ch.Close();
        }

        [UnityTest]
        public IEnumerator MakeThenClose()
        {
            var ch = Chan<T>.Make();
            ch.Close();

            Assert.True(!Channels.IsExists<T>());
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendThenClose()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T(true));
            ch.Close();

            Assert.True(!Channels.IsExists<T>());
            yield return null;
        }

        [UnityTest]
        public IEnumerator SendThenReceive2ch()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T(true));

            var done = false;
            var ch2 = Chan<T>.Make();
            ch2.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    Assert.NotNull(a.id);
                    done = true;
                }
            );

            while (!done)
            {
                yield return null;
            }
            ch2.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator SendFromAnotherThread()
        {
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch = Chan<T>.Make();
                        ch.Send(new T(true));
                    }
                )
            );
            thread.Start();

            var done = false;
            var ch2 = Chan<T>.Make();
            ch2.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    var go = new GameObject();
                    Assert.NotNull(a.id);
                    done = true;
                }
            );

            while (!done)
            {
                yield return null;
            }
            ch2.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator SendFromAnotherThreadMulti()
        {
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch = Chan<T>.Make();
                        ch.Send(new T(true));
                    }
                )
            );
            thread.Start();

            var thread2 = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch = Chan<T>.Make();
                        ch.Send(new T(true));
                    }
                )
            );
            thread2.Start();

            var doneCount = 0;
            var ch2 = Chan<T>.Make();
            ch2.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    var go = new GameObject();
                    Assert.NotNull(a.id);
                    doneCount++;
                }
            );

            while (doneCount != 2)
            {
                yield return null;
            }
            ch2.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator ReceiveOnAnotherThread()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T(true));

            var done = false;
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch2 = Chan<T>.Make();
                        ch2.Receive(
                            (a, ok) =>
                            {
                                if (!ok)
                                {
                                    return;
                                }
                                Assert.NotNull(a.id);
                                done = true;
                            }
                        );
                    }
                )
            );
            thread.Start();

            while (!done)
            {
                yield return null;
            }
            ch.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator SendSingleAndReceiveMulti()
        {
            var ch = Chan<T>.Make();

            var done = false;
            var ch1 = Chan<T>.Make();
            ch1.Receive(
                (a, ok) =>
                {
                    Assert.Fail();
                }
            );

            var ch2 = Chan<T>.Make();
            ch2.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    Assert.False(done);
                    done = true;
                }
            );

            ch.Send(new T(true));

            while (!done)
            {
                yield return null;
            }
            ch.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator SendMultiAndReceiveMulti()
        {
            var ch0 = Chan<T>.Make();
            var ch1 = Chan<T>.Make();

            var doneCount = 0;

            var ch2 = Chan<T>.Make();
            ch2.Receive(
                (a, ok) =>
                {
                    Assert.Fail();
                }
            );

            var ch3 = Chan<T>.Make();
            ch3.Receive(
                (a, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    doneCount++;
                }
            );

            ch0.Send(new T(true));
            ch1.Send(new T(true));

            while (doneCount != 2)
            {
                yield return null;
            }

            ch0.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator ReceiveOnAnotherThreadOneOf()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T(true));

            var done = false;
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch2 = Chan<T>.Make();
                        ch2.Receive(
                            (a, ok) =>
                            {
                                if (!ok)
                                {
                                    return;
                                }
                                Assert.False(done);
                                done = true;
                            }
                        );
                    }
                )
            );
            thread.Start();

            var thread2 = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch3 = Chan<T>.Make();
                        ch3.Receive(
                            (a, ok) =>
                            {
                                if (!ok)
                                {
                                    return;
                                }
                                Assert.False(done);
                                done = true;
                            }
                        );
                    }
                )
            );
            thread2.Start();

            while (!done)
            {
                yield return null;
            }
            ch.Close();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator SendAndReceiveOnAnotherThread()
        {
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch = Chan<T>.Make();
                        ch.Send(new T(true));
                    }
                )
            );
            thread.Start();

            var done = false;
            var thread1 = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch2 = Chan<T>.Make();
                        ch2.Receive(
                            (a, ok) =>
                            {
                                if (!ok)
                                {
                                    return;
                                }
                                Assert.False(done);
                                done = true;
                            }
                        );
                    }
                )
            );
            thread1.Start();

            var thread2 = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch3 = Chan<T>.Make();
                        ch3.Receive(
                            (a, ok) =>
                            {
                                if (!ok)
                                {
                                    return;
                                }
                                Assert.False(done);
                                done = true;
                            }
                        );
                    }
                )
            );
            thread2.Start();

            while (!done)
            {
                yield return null;
            }

            Channels.Close<T>();

            Assert.True(!Channels.IsExists<T>());
        }

        private void ThreadMake()
        {
            var thread = new Thread(
                new ThreadStart(
                    () =>
                    {
                        var ch = Chan<T>.Make();
                        ch.Send(new T());
                    }
                )
            );
            thread.Start();
        }

        [UnityTest]
        public IEnumerator SendFromMultipleThread()
        {
            var receiveCount = 0;
            var expect = 10;
            for (var i = 0; i < expect; i++)
            {
                ThreadMake();
            }

            var ch = Chan<T>.Make();
            ch.Receive(
                (data, ok) =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    receiveCount++;
                }
            );

            while (receiveCount != expect)
            {
                yield return null;
            }

            Assert.True(receiveCount == expect);
        }

        [UnityTest]
        public IEnumerator WaitFirst()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T(true));
            yield return Channels.WaitFirst<T>();

            Channels.Close<T>();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator WaitFirstResult()
        {
            var ch = Chan<T>.Make();
            var data = new T(true);
            ch.Send(data);

            var cor = Channels.WaitFirstResult<T>();
            yield return cor;
            var result = cor.Result;
            Assert.True(result.id == data.id);

            Channels.Close<T>();

            Assert.True(!Channels.IsExists<T>());
        }

        [UnityTest]
        public IEnumerator WaitFirstResultOnCoroutine()
        {
            var ch = Chan<T>.Make();
            var data = new T(true);
            ch.Send(data);

            var cor = Channels.WaitFirstResult<T>();
            var go = new GameObject("test").AddComponent<TestMB>();
            go.StartCoroutine(cor);
            while (cor.IsDone)
            {
                yield return null;
            }

            var result = cor.Result;
            Assert.True(result.id == data.id);

            Channels.Close<T>();

            Assert.True(!Channels.IsExists<T>());
        }

        private IEnumerator SendCor()
        {
            var ch = Chan<T>.Make();
            ch.Send(new T());
            yield return null;

            // この時点でForが終了する。
            ch.Close();
        }

        [UnityTest]
        public IEnumerator For()
        {
            var cor = SendCor();
            var go = new GameObject("test").AddComponent<TestMB>();
            go.StartCoroutine(cor);

            var done = false;

            yield return Channels.For<T>(
                t =>
                {
                    done = true;
                }
            );

            Assert.True(done);
        }

        private IEnumerator CloseCor()
        {
            yield return null;
            // 閉じるだけでもForは終了する。
            Channels.Close<T>();
        }

        [UnityTest]
        public IEnumerator For0()
        {
            var cor = CloseCor();
            var go = new GameObject("test").AddComponent<TestMB>();
            go.StartCoroutine(cor);

            var cor2 = Channels.For<T>(
                t => { }
            );
            yield return cor2;

            Assert.True(cor2.IsDone);
        }

        [UnityTest]
        public IEnumerator For1()
        {
            var done = false;
            var ch = Chan<T>.Make();
            var data = new T(true);
            ch.Send(data);

            var cor = Channels.For<T>(
                t =>
                {
                    done = true;
                    Assert.True(data.id == t.id);
                }
            );
            var go = new GameObject("test").AddComponent<TestMB>();
            go.StartCoroutine(cor);

            ch.Close();

            while (!done)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator For2()
        {
            var cor = Channels.For<T>(
                t => { }
            );
            var go = new GameObject("test").AddComponent<TestMB>();
            go.StartCoroutine(cor);

            // チャンネルを閉じるとyieldを抜ける。
            Channels.Close<T>();

            Assert.True(cor.IsDone);
            yield break;
        }
    }
}
