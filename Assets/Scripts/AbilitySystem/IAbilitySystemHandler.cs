namespace AbilitySystem
{
    public interface IAbilitySystemHandler
    {
        bool isAlive { get; }
        bool ConsumeAbilityResource(AbilityBase abilityBase);
    }
}