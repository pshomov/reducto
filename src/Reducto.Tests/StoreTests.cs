using System.Collections.Generic;
using NUnit.Framework;
using System;

namespace Reducto.Tests
{
    public class ItemAdded
    {
        public string item;
    }

    [TestFixture]
    public class StoreTests
    {
        [Test]
        public void should_notify_subscribers_while_they_are_subscribed()
        {
            var reducer = new SimpleReducer<List<string>>(() => new List<string> {"Use ReduxVVM"});
            var store = new Store<List<string>>(reducer);

            var changed = 0;
            var unsub = store.Subscribe(state =>
            {
                Assert.NotNull(state);
                changed += 1;
            });

            store.Dispatch(new ItemAdded {item = "Read the Redux docs"});
            store.Dispatch(new ItemAdded {item = "Read the Redux docs"});

            Assert.That(changed, Is.EqualTo(2));
            unsub();
            store.Dispatch(new ItemAdded {item = ""});

            Assert.That(changed, Is.EqualTo(2));
        }

        [Test]
        public void should_register_root_reducer()
        {
            Reducer<List<string>> reducer = (List<string> state, Object action) =>
            {
                if (action.GetType() == typeof (InitStoreAction)) return new List<string> {"Use ReduxVVM"};

                var newState = new List<string>(state);

                switch (action.GetType().Name)
                {
                    case "ItemAdded":
                        var concreteEv = (ItemAdded) action;
                        newState.Add(concreteEv.item);
                        break;
                    default:
                        break;
                }
                return newState;
            };
            var store = new Store<List<string>>(reducer);
            store.Dispatch(new ItemAdded {item = "Read the Redux docs"});

            CollectionAssert.AreEqual(store.GetState(), new List<string> {"Use ReduxVVM", "Read the Redux docs"});
        }

        [Test]
        public void should_register_root_reducer_with_builder()
        {
            var reducer = new SimpleReducer<List<string>>(() => new List<string> {"Use ReduxVVM"})
                .When<ItemAdded>((state, action) =>
                {
                    var newSatte = new List<string>(state);
                    newSatte.Add(action.item);
                    return newSatte;
                })
                .Get();
            var store = new Store<List<string>>(reducer);
            store.Dispatch(new ItemAdded {item = "Read the Redux docs"});

            CollectionAssert.AreEqual(store.GetState(), new List<string> {"Use ReduxVVM", "Read the Redux docs"});
        }

        [Test]
        public void should_return_same_state_when_command_not_for_that_reducer()
        {
            var reducer = new SimpleReducer<List<string>>(() => new List<string> {"Use ReduxVVM"});
            var store = new Store<List<string>>(reducer);
            store.Dispatch(new ItemAdded {item = "Read the Redux docs"});

            CollectionAssert.AreEqual(store.GetState(), new List<string> {"Use ReduxVVM"});
        }
    }
}