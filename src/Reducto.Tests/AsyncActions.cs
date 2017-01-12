using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Reducto.Tests
{
    public struct LoginInfo
    {
        public string username;
    }

    [TestFixture]
    public class AsyncActions
    {
        [Test]
        public async Task should_allow_for_async_execution_of_code()
        {
            var storeReducerReached = 0;
            var reducer = new SimpleReducer<List<string>>(() => new List<string> {"a"}).When<SomeAction>((s, e) =>
            {
                storeReducerReached += 1;
                return s;
            });
            var store = new Store<List<string>>(reducer);

            var result = await store.Dispatch(store.asyncAction<int>(async (dispatcher, store2) =>
            {
                await Task.Delay(300);
                Assert.That(store2()[0], Is.EqualTo("a"));
                dispatcher(new SomeAction());
                return 112;
            }));

            Assert.That(storeReducerReached, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(112));
        }

        [Test]
        public async Task should_allow_for_passing_parameters_to_async_actions()
        {
            var storeReducerReached = 0;
            var reducer = new SimpleReducer<List<string>>(() => new List<string> {"a"}).When<SomeAction>((s, e) =>
            {
                storeReducerReached += 1;
                return s;
            });
            var store = new Store<List<string>>(reducer);

            var action1 = store.asyncAction<LoginInfo, int>(async (dispatcher, store2, msg) =>
            {
                await Task.Delay(300);
                Assert.That(msg.username, Is.EqualTo("John"));
                dispatcher(new SomeAction());
                return 112;
            });
            var result = await store.Dispatch(action1(new LoginInfo
            {
                username = "John"
            }));

            Assert.That(storeReducerReached, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(112));
        }
    }
}