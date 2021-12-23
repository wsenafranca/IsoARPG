using UnityEngine;

namespace DialogSystem
{
    public class Talkative : TargetBase
    {
        public override Color outlineColor => GameAsset.instance.outlineTalkative;
        
        public override bool isTargetValid => true;
    }
}