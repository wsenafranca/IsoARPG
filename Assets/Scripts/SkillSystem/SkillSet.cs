using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem
{
    public class SkillSet : MonoBehaviour
    {
        public List<Skill> skills;

        private readonly Dictionary<Type, SkillInstance> _skillInstances = new();

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
            if (skill != null) return _skillInstances.TryGetValue(skill.GetType(), out instance);
            
            instance = null;
            return false;

        }
    }
}