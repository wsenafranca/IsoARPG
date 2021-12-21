using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FiniteStateMachine
{
    public class StateMachine : MonoBehaviour
    {
        private readonly Dictionary<Type, List<Transition>> _transitions = new();
        private List<Transition> _currentTransitions = new();
        private readonly List<Transition> _anyTransitions = new();
   
        private static readonly List<Transition> EmptyTransitions = new();

        public float currentStateElapsedTime { get; private set; }
        private IState _currentState;
        public IState currentState
        {
            get => _currentState;
            set
            {
                if (value == null) return;
                
                _currentState?.OnStateExit(this);
                _currentState = value;

                _transitions.TryGetValue(_currentState.GetType(), out _currentTransitions);
                _currentTransitions ??= EmptyTransitions;

                currentStateElapsedTime = 0;
                _currentState?.OnStateEnter(this);
            }
        }

        private void Update()
        {
            var transition = nextTransition;
            if (transition != null)
            {
                currentState = transition.to;
            }
            
            currentState?.OnStateUpdate(this, currentStateElapsedTime);
            currentStateElapsedTime += Time.deltaTime;
        }

        public T GetCurrentState<T>() where T : IState
        {
            return (T)currentState;
        }

        public void AddTransition(IState from, IState to, Func<bool> pred)
        {
            if (!_transitions.TryGetValue(from.GetType(), out var transitions))
            {
                transitions = new List<Transition>();
                _transitions[from.GetType()] = transitions;
            }

            transitions.Add(new Transition(to, pred));
        }

        public void AddAnyTransition(IState to, Func<bool> pred)
        {
            _anyTransitions.Add(new Transition(to, pred));
        }
        
        private class Transition
        {
            public readonly IState to;
            public readonly Func<bool> condition;
            
            public Transition(IState to, Func<bool> condition)
            {
                this.to = to;
                this.condition = condition;
            }
        }

        private Transition nextTransition => 
            _anyTransitions.FirstOrDefault(transition => transition.condition()) ?? 
            _currentTransitions.FirstOrDefault(transition => transition.condition());
    }
}