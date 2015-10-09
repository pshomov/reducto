using NUnit.Framework;

namespace Reducto.Tests
{
    public class TopicSet : Action
    {
        public string topic;
    }

    public class FilterVisibility : Action
    {
        public bool visible;
    }

    public struct AppStore
    {
        public string redditTopic;
        public bool visibility;

        public override string ToString()
        {
            return string.Format("topic:{0}, visibility {1}", redditTopic, visibility);
        }
    }

    [TestFixture]
    public class ReducersTests
    {
        private struct Address
        {
            public string city;
            public string streetNr;
        }

        private enum DeliveryMethod
        {
            REGULAR,
            GUARANTEED
        }

        private struct Destination
        {
            public Address addr;
            public DeliveryMethod deliver;
        }

        private struct Order
        {
            public Destination destination;
            public string name;
            public Address origin;
        }

        private struct SetOrigin : Action
        {
            public Address newAddress;
        }

        private struct SetDestination : Action
        {
            public Address newAddress;
        }

        private struct BehindSchedule : Action
        {
        }

        private struct SetDelivery : Action
        {
            public DeliveryMethod method;
        }

        [Test]
        public void should_prvide_way_to_combine_reducers()
        {
            var topicReducer = new SimpleReducer<string>().When<TopicSet>((s, e) => e.topic);
            var visibilityReducer = new SimpleReducer<bool>().When<FilterVisibility>((s, e) => e.visible);
            var reducer = new CompositeReducer<AppStore>(() => new AppStore {redditTopic = "react", visibility = false})
                .Part(s => s.redditTopic, topicReducer)
                .Part(s => s.visibility, visibilityReducer);
            var store = new Store<AppStore>(reducer);
            store.Dispatch(new TopicSet {topic = "Redux is awesome"});
            store.Dispatch(new FilterVisibility {visible = true});

            Assert.AreEqual(new AppStore {redditTopic = "Redux is awesome", visibility = true}, store.GetState());
        }

        [Test]
        public void should_prvide_way_to_create_deep_hierarchy_of_reducers()
        {
            var originReducer = new SimpleReducer<Address>().When<SetOrigin>((s, e) => e.newAddress);
            var destinationReducer = new CompositeReducer<Destination>()
                .Part(s => s.deliver,
                    new SimpleReducer<DeliveryMethod>().When<BehindSchedule>((s, a) => DeliveryMethod.REGULAR)
                        .When<SetDelivery>((_, a) => a.method))
                .Part(s => s.addr, new SimpleReducer<Address>().When<SetDestination>((s, a) => a.newAddress));
            var orderReducer = new CompositeReducer<Order>()
                .Part(s => s.origin, originReducer)
                .Part(s => s.destination, destinationReducer);
            var store = new Store<Order>(orderReducer);
            store.Dispatch(new SetOrigin {newAddress = new Address {streetNr = "Laugavegur 26", city = "Reykjavík"}});
            store.Dispatch(new SetDestination {newAddress = new Address {streetNr = "5th Street", city = "New York"}});
            store.Dispatch(new SetDelivery {method = DeliveryMethod.GUARANTEED});

            store.Dispatch(new BehindSchedule());

            Assert.AreEqual(new Order
            {
                origin = new Address {streetNr = "Laugavegur 26", city = "Reykjavík"},
                destination =
                    new Destination
                    {
                        addr = new Address {streetNr = "5th Street", city = "New York"},
                        deliver = DeliveryMethod.REGULAR
                    }
            }, store.GetState());
        }
    }
}