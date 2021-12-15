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
    
    /*
    [CustomPropertyDrawer(typeof(AttributeData))]
    [CustomPropertyDrawer(typeof(AdditiveModifierData))]
    [CustomPropertyDrawer(typeof(MultiplicativeModifierData))]
    public class AttributeDataPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = property.FindPropertyRelative("attribute");
            var baseValue = property.FindPropertyRelative("value").FindPropertyRelative("baseValue");
            
            //label = EditorGUI.BeginProperty(position, label, property);
            //var attrPosition = EditorGUI.PrefixLabel(position, label);
            var attrPosition = position;
            attrPosition.x = position.width / 2;
            attrPosition.width = position.width / 2;
            var style = new GUIStyle(EditorStyles.popup)
            {
                stretchWidth = false,
                fixedWidth = attrPosition.width / 2 - 64
            };
            
            
            attr.SetEnumValue(EditorGUI.EnumPopup(position, attr.GetEnumValue<Attribute>(), style));
            baseValue.intValue = EditorGUI.IntField(attrPosition, GUIContent.none, baseValue.intValue);
            //EditorGUI.EndProperty();
        }
    }
    */
}