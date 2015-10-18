using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reducto
{
    public sealed class InitStoreAction
    {
    }

    public delegate State Reducer<State>(State state, Object action);

    public delegate void StateChangedSubscriber<State>(State state);

    public delegate void Unsubscribe();

    public delegate void DispatcherDelegate(Object a);

    public interface IBasicStore<State>
    {
        Unsubscribe Subscribe(StateChangedSubscriber<State> subscription);
        void Dispatch(Object action);
        State GetState();
    }

    public class Store<State>
    {
        public delegate State GetStateDelegate();

        private readonly BasicStore store;
        private MiddlewareExecutor middlewares;

        public Store(SimpleReducer<State> rootReducer) : this(rootReducer.Get())
        {
        }

        public Store(CompositeReducer<State> rootReducer) : this(rootReducer.Get())
        {
        }

        public Store(Reducer<State> rootReducer)
        {
            store = new BasicStore(rootReducer);
            Middleware();
        }

        public Unsubscribe Subscribe(StateChangedSubscriber<State> subscription)
        {
            return store.Subscribe(subscription);
        }

        public void Dispatch(Object action)
        {
            middlewares(action);
        }

        public Task<Result> Dispatch<Result>(Func<DispatcherDelegate, GetStateDelegate, Task<Result>> actionWithParams)
        {
            return actionWithParams(Dispatch, GetState);
        }

        public Task Dispatch(Func<DispatcherDelegate, GetStateDelegate, Task> actionWithParams)
        {
            return actionWithParams(Dispatch, GetState);
        }

        public Func<T, Func<DispatcherDelegate, GetStateDelegate, Task<Result>>> asyncAction<T, Result>(
            Func<DispatcherDelegate, GetStateDelegate, T, Task<Result>> m)
        {
            return a => (dispatch, getState) => m(dispatch, getState, a);
        }

        public Func<T, Func<DispatcherDelegate, GetStateDelegate, Task>> asyncActionVoid<T>(
            Func<DispatcherDelegate, GetStateDelegate, T, Task> m)
        {
            return a => (dispatch, getState) => m(dispatch, getState, a);
        }

        public Func<DispatcherDelegate, GetStateDelegate, Task<Result>> asyncAction<Result>(
            Func<DispatcherDelegate, GetStateDelegate, Task<Result>> m)
        {
            return (dispatch, getState) => m(dispatch, getState);
        }

        public State GetState()
        {
            return store.GetState();
        }

        public void Middleware(params Middleware<State>[] middlewares)
        {
            this.middlewares =
                middlewares.Select(m => m(store))
                    .Reverse()
                    .Aggregate<MiddlewareChainer, MiddlewareExecutor>(store.Dispatch, (acc, middle) => middle(acc));
        }

        private class BasicStore : IBasicStore<State>
        {
            private readonly Reducer<State> rootReducer;

            private readonly List<StateChangedSubscriber<State>> subscriptions =
                new List<StateChangedSubscriber<State>>();

            private State state;

            public BasicStore(Reducer<State> rootReducer)
            {
                this.rootReducer = rootReducer;
                state = rootReducer(state, new InitStoreAction());
            }

            public Unsubscribe Subscribe(StateChangedSubscriber<State> subscription)
            {
                subscriptions.Add(subscription);
                return () => { subscriptions.Remove(subscription); };
            }

            public void Dispatch(Object action)
            {
                state = rootReducer(state, action);
                foreach (var subscribtion in subscriptions)
                {
                    subscribtion(state);
                }
            }

            public State GetState()
            {
                return state;
            }
        }
    }

    public delegate void MiddlewareExecutor(Object action);

    public delegate MiddlewareExecutor MiddlewareChainer(MiddlewareExecutor nextMiddleware);

    public delegate MiddlewareChainer Middleware<State>(IBasicStore<State> store);
}