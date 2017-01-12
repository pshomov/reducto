using System.Collections.Generic;
using NUnit.Framework;

namespace Reducto.Tests
{
    public class SomeAction
    {
        public string topic;
    }

    [TestFixture]
    public class MiddlewareTests
    {
        [Test]
        public void should_allow_middleware_shortcut_the_store_dispatcher()
        {
            var storeReducerReached = 0;
            var reducer = new SimpleReducer<List<string>>().When<SomeAction>((s, e) =>
            {
                storeReducerReached += 1;
                return s;
            });
            var stateStore = new Store<List<string>>(reducer);
            var middlewareCounter = 0;
            stateStore.Middleware(
                store => next => action =>
                {
                    middlewareCounter += 3;
                    Assert.That(middlewareCounter, Is.EqualTo(3));
                    next(action);
                    middlewareCounter += 3000;
                    Assert.That(middlewareCounter, Is.EqualTo(3333));
                },
                store => next => action =>
                {
                    middlewareCounter += 30;
                    Assert.That(middlewareCounter, Is.EqualTo(33));
                    middlewareCounter += 300;
                    Assert.That(middlewareCounter, Is.EqualTo(333));
                }
                );

            stateStore.Dispatch(new SomeAction());
            Assert.That(middlewareCounter, Is.EqualTo(3333));
            Assert.That(storeReducerReached, Is.EqualTo(0));
        }

        [Test]
        public void should_allow_middleware_to_hook_into_dispatching()
        {
            var storeReducerReached = 0;
            var reducer = new SimpleReducer<List<string>>().When<SomeAction>((s, e) =>
            {
                storeReducerReached += 1;
                return s;
            });
            var stateStore = new Store<List<string>>(reducer);
            var middlewareCounter = 0;
            stateStore.Middleware(
                store => next => action =>
                {
                    middlewareCounter += 3;
                    Assert.That(middlewareCounter, Is.EqualTo(3));
                    next(action);
                    middlewareCounter += 3000;
                    Assert.That(middlewareCounter, Is.EqualTo(3333));
                },
                store => next => action =>
                {
                    middlewareCounter += 30;
                    Assert.That(middlewareCounter, Is.EqualTo(33));
                    Assert.That(storeReducerReached, Is.EqualTo(0));
                    next(action);
                    Assert.That(storeReducerReached, Is.EqualTo(1));
                    middlewareCounter += 300;
                    Assert.That(middlewareCounter, Is.EqualTo(333));
                }
                );

            stateStore.Dispatch(new SomeAction());
            Assert.That(middlewareCounter, Is.EqualTo(3333));
            Assert.That(storeReducerReached, Is.EqualTo(1));
        }

        [Test]
        public void should_allow_middleware_to_shortcut_lower_middleware()
        {
            var storeReducerReached = 0;
            var reducer = new SimpleReducer<List<string>>().When<SomeAction>((s, e) =>
            {
                storeReducerReached += 1;
                return s;
            });
            var stateStore = new Store<List<string>>(reducer);
            var middlewareCounter = 0;
            stateStore.Middleware(
                store => next => action =>
                {
                    middlewareCounter += 3;
                    Assert.That(middlewareCounter, Is.EqualTo(3));
                    middlewareCounter += 3000;
                    Assert.That(middlewareCounter, Is.EqualTo(3003));
                },
                store => next => action =>
                {
                    middlewareCounter += 30;
                    Assert.That(middlewareCounter, Is.EqualTo(33));
                    Assert.That(storeReducerReached, Is.EqualTo(0));
                    next(action);
                    Assert.That(storeReducerReached, Is.EqualTo(1));
                    middlewareCounter += 300;
                    Assert.That(middlewareCounter, Is.EqualTo(333));
                }
                );

            stateStore.Dispatch(new SomeAction());
            Assert.That(middlewareCounter, Is.EqualTo(3003));
            Assert.That(storeReducerReached, Is.EqualTo(0));
        }
    }
}