using AttributeSystem;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(AttributeValue))]
    public class AttributeValuePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var baseValue = property.FindPropertyRelative("baseValue");
            EditorGUI.PropertyField(position, baseValue, label);
        }
    }
}