using System;
using Character;
using UnityEngine;

namespace AI
{
    [RequireComponent(typeof(CharacterBase))]
    public class AITarget : TargetBase
    {
        public CharacterBase character { get; private set; }

        public override bool isTargetValid => character.isAlive;

        protected override void Awake()
        {
            base.Awake();
            character = GetComponent<CharacterBase>();
        }

        public override Color outlineColor => character.characterType switch
        {
            CharacterType.None => GameAsset.instance.outlineNeutral,
            CharacterType.Player => GameAsset.instance.outlinePlayer,
            CharacterType.Ally => GameAsset.instance.outlineAlly,
            CharacterType.Enemy => GameAsset.instance.outlineEnemy,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}