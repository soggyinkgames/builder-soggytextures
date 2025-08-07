namespace SoggyInkGames.SoggySolutions.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(YourScriptName))]
    public class SoggySolutionsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            YourScriptName script = (YourScriptName)target;

            if (GUILayout.Button("Do Something in Editor"))
            {
                Debug.Log("Editor button pressed on " + script.gameObject.name);
            }
        }
    }
}