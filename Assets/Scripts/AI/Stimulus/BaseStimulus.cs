using Character;

namespace AI.Stimulus
{
    public abstract class BaseStimulus
    {
        protected readonly CharacterBase owner;
        public readonly int priority;

        protected BaseStimulus(CharacterBase owner, int priority)
        {
            this.owner = owner;
            this.priority = priority;
        }
        
        public abstract int countTargets {get;}
        
        public abstract CharacterBase currentTarget { get; }
        
        public abstract void OnEnable();
        
        public abstract void OnDisable();
    }
}