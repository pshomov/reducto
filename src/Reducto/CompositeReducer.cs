using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Reducto
{
    public class CompositeReducer<State>
    {
        private readonly List<Tuple<FieldInfo, Delegate>> fieldReducers = new List<Tuple<FieldInfo, Delegate>>();
        private readonly Func<State> stateInitializer;

        public CompositeReducer()
        {
            stateInitializer = () => default(State);
        }

        public CompositeReducer(Func<State> initializer)
        {
            this.stateInitializer = initializer;
        }

        public CompositeReducer<State> Part<T>(Expression<Func<State, T>> composer, SimpleReducer<T> reducer)
        {
            return Part(composer, reducer.Get());
        }

        public CompositeReducer<State> Part<T>(Expression<Func<State, T>> composer, CompositeReducer<T> reducer)
        {
            return Part(composer, reducer.Get());
        }

        public CompositeReducer<State> Part<T>(Expression<Func<State, T>> composer, Reducer<T> reducer)
        {
            var memberExpr = composer.Body as MemberExpression;
            var member = (FieldInfo) memberExpr.Member;

            if (memberExpr == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' should be a field.",
                    composer.ToString()));
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' should be a constant expression",
                    composer.ToString()));

            fieldReducers.Add(new Tuple<FieldInfo, Delegate>(member, reducer));
            return this;
        }

        public Reducer<State> Get()
        {
            return delegate(State state, Object action)
            {
                var result = action.GetType() == typeof (InitStoreAction) ? stateInitializer() : state;
                foreach (var fieldReducer in fieldReducers)
                {
                    var prevState = action.GetType() == typeof (InitStoreAction)
                        ? null
                        : fieldReducer.Item1.GetValue(state);
                    var newState = fieldReducer.Item2.DynamicInvoke(prevState, action);
                    object boxer = result; //boxing to allow the next line work for both reference and value objects
                    fieldReducer.Item1.SetValue(boxer, newState);
                    result = (State) boxer; // unbox, hopefully not too much performance penalty
                }
                return result;
            };
        }
    }
}