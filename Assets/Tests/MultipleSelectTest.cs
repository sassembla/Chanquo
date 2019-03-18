using System;
using System.Collections;
using ChanquoCore;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MultipleSelectTest
    {
        private ChanquoChannel ct;
        private ChanquoChannel cu;

        private ChanquoAction<T, U> s;


        [TearDown]
        public void Teardonw()
        {
            ct?.Dispose();
            cu?.Dispose();
            s?.Dispose();
            var s2 = GameObject.FindObjectOfType<ChanquoThreadRunner>();
            Assert.True(s2.update.Count == 0);
        }

        public class T : ChanquoBase
        {
            public string message;
        }

        public class U : ChanquoBase
        {
            public string message;
        }


        [UnityTest]
        public IEnumerator SelectMultiple()
        {
            var messageT = "SelectMultiple" + Guid.NewGuid().ToString();
            var messageU = "SelectMultiple" + Guid.NewGuid().ToString();

            ct = Chanquo.MakeChannel<T>();
            ct.Send(
                new T()
                {
                    message = messageT
                }
            );

            cu = Chanquo.MakeChannel<U>();
            cu.Send(
                new U()
                {
                    message = messageU
                }
            );

            var receiveT = false;
            var receiveU = false;
            s = Chanquo.Select<T, U>(
                t =>
                {
                    if (t.Ok)
                    {
                        Assert.True(t.message == messageT);
                        receiveT = true;
                    }
                },
                u =>
                {
                    if (u.Ok)
                    {
                        Assert.True(u.message == messageU);
                        receiveU = true;
                    }
                }
            );

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!receiveT || !receiveU)
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
        public IEnumerator SelectMultipleThenDisposeReceiveOnlyOne()
        {
            var messageT = "SelectMultipleThenDisposeReceiveOnlyOne" + Guid.NewGuid().ToString();
            var messageU = "SelectMultipleThenDisposeReceiveOnlyOne" + Guid.NewGuid().ToString();

            ct = Chanquo.MakeChannel<T>();
            ct.Send(
                new T()
                {
                    message = messageT
                }
            );

            cu = Chanquo.MakeChannel<U>();
            cu.Send(
                new U()
                {
                    message = messageU
                }
            );

            var receiveT = false;
            var receiveU = false;
            s = Chanquo.Select<T, U>(
                t =>
                {
                    if (t.Ok)
                    {
                        Assert.True(!receiveU);
                        Assert.True(t.message == messageT);
                        receiveT = true;
                        s.Dispose();
                    }
                },
                u =>
                {
                    if (u.Ok)
                    {
                        Assert.True(!receiveT);
                        Assert.True(u.message == messageU);
                        receiveU = true;
                        s.Dispose();
                    }
                }
            );

            var waitTime = DateTime.Now + TimeSpan.FromSeconds(1);
            while (!receiveT && !receiveU)
            {
                if (waitTime < DateTime.Now)
                {
                    Debug.LogError("timeout");
                    break;
                }
                yield return null;
            }
        }
    }
}