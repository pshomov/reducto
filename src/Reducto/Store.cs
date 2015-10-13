using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reducto
{
    public interface Action
    {
    }

    public sealed class InitStoreAction : Action
    {
    }

    public delegate State Reducer<State>(State state, Action action);

    public delegate void StateChangedSubscriber<State>(State state);

    public delegate void Unsubscribe();

    public delegate void DispatcherDelegate(Action a);

    public interface IStore<State>
    {
        Unsubscribe Subscribe(StateChangedSubscriber<State> subscription);
        void Dispatch(Action action);
        State GetState();
    }

    public class Store<State> where State : new()
    {
        public delegate State GetStateDelegate();

        private readonly SyncStore store;
        private MiddlewareExecutor middlewares;

        public Store(SimpleReducer<State> rootReducer) : this(rootReducer.Get())
        {
        }

        public Store(CompositeReducer<State> rootReducer) : this(rootReducer.Get())
        {
        }

        public Store(Reducer<State> rootReducer)
        {
            store = new SyncStore(rootReducer);
            Middleware();
        }

        public Unsubscribe Subscribe(StateChangedSubscriber<State> subscription)
        {
            return store.Subscribe(subscription);
        }

        public void Dispatch(Action action)
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

        private class SyncStore : IStore<State>
        {
            private readonly Reducer<State> rootReducer;

            private readonly List<StateChangedSubscriber<State>> subscriptions =
                new List<StateChangedSubscriber<State>>();

            private State state;

            public SyncStore(Reducer<State> rootReducer)
            {
                this.rootReducer = rootReducer;
                state = rootReducer(state, new InitStoreAction());
            }

            public Unsubscribe Subscribe(StateChangedSubscriber<State> subscription)
            {
                subscriptions.Add(subscription);
                return () => { subscriptions.Remove(subscription); };
            }

            public void Dispatch(Action action)
            {
                state = rootReducer(state, action);
                foreach (var s in subscriptions)
                {
                    s(state);
                }
            }

            public State GetState()
            {
                return state;
            }
        }
    }

    public delegate void MiddlewareExecutor(Action a);

    public delegate MiddlewareExecutor MiddlewareChainer(MiddlewareExecutor next);

    public delegate MiddlewareChainer Middleware<State>(IStore<State> store);
}