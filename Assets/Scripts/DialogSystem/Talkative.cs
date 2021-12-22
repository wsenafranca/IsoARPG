using Player;
using UnityEngine;

namespace DialogSystem
{
    public class Talkative : TargetBase
    {
        public string displayName;
        
        public override Color outlineColor => GameAsset.instance.outlineTalkative;

        public void Talk(PlayerController player)
        {
            print($"Hello, {player.displayName}! I'm " + displayName);
        }
        
        public override bool isTargetValid => true;
    }
}