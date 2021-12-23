using System;
using Character;

namespace AI.Stimulus
{
    public abstract class BaseStimulus
    {
        protected readonly CharacterBase owner;
        public readonly int priority;

        public event Action<BaseStimulus, CharacterBase> CharacterEnter;
        public event Action<BaseStimulus, CharacterBase> CharacterExit;

        protected BaseStimulus(CharacterBase owner, int priority)
        {
            this.owner = owner;
            this.priority = priority;
        }

        protected void NotifyCharacterEnter(CharacterBase character)
        {
            CharacterEnter?.Invoke(this, character);
        }
        
        protected void NotifyCharacterExit(CharacterBase character)
        {
            CharacterExit?.Invoke(this, character);
        }
        
        public abstract int countTargets {get;}
        
        public abstract CharacterBase currentTarget { get; }
        
        public abstract void OnEnable();
        
        public abstract void OnDisable();
    }
}