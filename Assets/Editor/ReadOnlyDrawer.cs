using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool canEdit = !(EditorApplication.isPlaying || EditorApplication.isPaused);
        GUI.enabled = canEdit;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = !canEdit;
    }
}

[CustomPropertyDrawer(typeof(ReadOnlyRangeAttribute))]
public class ReadOnlyRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attribute = (ReadOnlyRangeAttribute)this.attribute;
        bool canEdit = !(EditorApplication.isPlaying || EditorApplication.isPaused);
        GUI.enabled = canEdit;
        if (property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(position, property, attribute.min, attribute.max, label);
        else if (property.propertyType == SerializedPropertyType.Integer)
            EditorGUI.IntSlider(position, property, (int)attribute.min, (int)attribute.max, label);
        else
            EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
        GUI.enabled = !canEdit;
    }
}