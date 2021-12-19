using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem
{
    public class SkillSet : MonoBehaviour
    {
        [SerializeField]
        private List<Skill> skills;

        private readonly Dictionary<Type, SkillInstance> _skillInstances = new();
        
        [SerializeField]
        private Skill[] skillMouseSlot = new Skill[2];

        private void Awake()
        {
            _skillInstances.Clear();

            foreach (var skill in skills)
            {
                _skillInstances[skill.GetType()] = skill.CreateSkillInstance();
            }
        }

        public bool TryGetSkillInstance(Type skillType, out SkillInstance instance)
        {
            return _skillInstances.TryGetValue(skillType, out instance);
        }

        public bool TryGetSkillInstance(Skill skill, out SkillInstance instance)
        {
            return _skillInstances.TryGetValue(skill.GetType(), out instance);
        }

        public bool TryGetSkillFromMouseButton(int button, out SkillInstance instance)
        {
            return _skillInstances.TryGetValue(skillMouseSlot[button].GetType(), out instance);
        }
    }
}