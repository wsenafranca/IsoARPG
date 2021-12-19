using System;
using System.Collections.Generic;

namespace FiniteStateMachine
{
    public static class StateMachineManager
    {
        private static readonly Dictionary<Type, IState> States = new();

        public static T GetState<T>() where T : IState, new()
        {
            if (States.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }

            state = new T();
            States[typeof(T)] = state;
            return (T)state;
        }
    }
}