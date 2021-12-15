using System;
using AbilitySystem;
using UnityEditor;
using UnityEngine;
using Attribute = AbilitySystem.Attribute;

namespace Editor
{
    [CustomPropertyDrawer(typeof(AttributeSet))]
    public class AttributeSetPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            
            // Draw label
            var guiStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
            EditorGUI.PrefixLabel(position, label, guiStyle);

            var attributes = property.FindPropertyRelative("attributes");
            var labels = Enum.GetNames(typeof(Attribute));
            
            var pos = position;
            for (var i = 0; i < attributes.arraySize; i++)
            {
                var attr = attributes.GetArrayElementAtIndex(i);
                var baseValue = attr.FindPropertyRelative("baseValue");
                var newValue = 0.0f;
                var l = new GUIContent(labels[i]);
                pos.height = 18;
                pos.y += pos.height + 2;

                var valLabel = EditorGUI.BeginProperty(pos, l, attr);
                EditorGUI.BeginChangeCheck();
                switch ((Attribute)i)
                {
                    case Attribute.MaxHealth:
                    case Attribute.Strength:
                    case Attribute.Stamina:
                    case Attribute.Dexterity:
                    case Attribute.Intelligence:
                    case Attribute.AttackSpeed:
                        newValue = Mathf.Max(1, EditorGUI.IntField(pos, valLabel, (int)baseValue.floatValue));
                        break;
                    case Attribute.MaxMana:
                    case Attribute.MinAttackPower:
                    case Attribute.MaxAttackPower:
                    case Attribute.MaxEnergyShield:
                    case Attribute.DefensePower:
                        newValue = Mathf.Max(0, EditorGUI.IntField(pos, valLabel, (int)baseValue.floatValue));
                        break;
                    case Attribute.EvasionRate:
                    case Attribute.CriticalHitRate:
                    case Attribute.BlockRate:
                        newValue = EditorGUI.Slider(pos, valLabel, baseValue.floatValue, 0.0f, 1.0f);
                        break;
                    case Attribute.CriticalHitDamage:
                        newValue = Mathf.Max(0, EditorGUI.FloatField(pos, valLabel, baseValue.floatValue));
                        break;
                    case Attribute.MoveSpeed:
                        newValue = Mathf.Max(0, EditorGUI.FloatField(pos, valLabel, baseValue.floatValue));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    baseValue.floatValue = newValue;
                }
                EditorGUI.EndProperty();
            }
            
            EditorGUI.EndFoldoutHeaderGroup();
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attributes = property.FindPropertyRelative("attributes");
            var y = base.GetPropertyHeight(property, label);
            for (var i = 0; i < attributes.arraySize; i++)
            {
                var attr = attributes.GetArrayElementAtIndex(i);
                y += 18 + 2;
            }

            return y;
        }
    }
}