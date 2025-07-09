using UnityEditor;
using UnityEngine;

namespace _GAME.Scripts.Tools.Editor
{
    namespace _GAME.Scripts.Save
    {
        public static class SaveManagerEditor
        {
            private const string GlobalSaveKey = "GlobalSave";

            [MenuItem("Save/Clear All Saves")]
            private static void ClearAllSaves()
            {
                if (EditorUtility.DisplayDialog(
                        "Clear All Saves",
                        "Are you sure you want to delete all saved data?",
                        "Yes", "No"))
                {
                    PlayerPrefs.DeleteKey(GlobalSaveKey);
                    PlayerPrefs.Save();
                    Debug.Log("[SaveManagerEditor] All saves cleared.");
                }
            }
        }
    }
}