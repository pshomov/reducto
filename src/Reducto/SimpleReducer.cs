using System;
using System.Collections.Generic;

namespace Reducto
{
    public class SimpleReducer<State>
    {
        private readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();
        private readonly Func<State> stateInitializer;

        public SimpleReducer()
        {
            stateInitializer = () => default(State);
        }

        public SimpleReducer(Func<State> initializer)
        {
            this.stateInitializer = initializer;
        }

        public SimpleReducer<State> When<Event>(Func<State, Event, State> handler)
        {
            handlers.Add(typeof (Event), handler);
            return this;
        }

        public Reducer<State> Get()
        {
            return delegate(State state, Object action)
            {
                var prevState = action.GetType() == typeof (InitStoreAction) ? stateInitializer() : state;
                if (handlers.ContainsKey(action.GetType()))
                {
                    var handler = handlers[action.GetType()];
                    return (State) handler.DynamicInvoke(prevState, action);
                }
                return prevState;
            };
        }
    }
}