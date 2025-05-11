#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _GAME.Scripts.Tools.Editor
{


    [InitializeOnLoad]
    public static class SceneStarter
    {
        private const string AutoBootstrapActiveKey = "AutoBootstrapActive";
        
        private static readonly string _defaultScenePath = "Assets/_GAME/Scenes/Core.unity";
        private static readonly string _workScenePathKey = "WORK_SCENE_PATH";
        private static bool _isAutoBootstrapActive = true;

        static SceneStarter()
        {
            if (PlayerPrefs.HasKey(AutoBootstrapActiveKey))
            {
                _isAutoBootstrapActive = Convert.ToBoolean(PlayerPrefs.GetInt(AutoBootstrapActiveKey));
            }
            else
            {
                PlayerPrefs.SetInt(AutoBootstrapActiveKey, 1);
            }
            
            EditorApplication.playModeStateChanged += LoadStartScene;
        }

        private static void LoadStartScene(PlayModeStateChange state)
        {
            if (!_isAutoBootstrapActive)
            {
                return;
            }
            
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (SceneManager.GetActiveScene().path != _defaultScenePath)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        PlayerPrefs.SetString(_workScenePathKey, SceneManager.GetActiveScene().path);
                        try
                        {
                            EditorSceneManager.OpenScene(_defaultScenePath);
                        }
                        catch
                        {
                            Debug.LogError("Default scene not founded");
                            EditorApplication.isPlaying = false;
                        }
                    }
                    else
                    {
                        EditorApplication.isPlaying = false;
                    }
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                string workScenePath = PlayerPrefs.GetString(_workScenePathKey, "");
                if (!string.IsNullOrEmpty(workScenePath) && SceneManager.GetActiveScene().path != workScenePath)
                {
                    try
                    {
                        EditorSceneManager.OpenScene(workScenePath);
                    }
                    catch
                    {
                        Debug.LogError($"Work scene not founded \n{workScenePath}");
                    }
                }
            }
        }

        [MenuItem("Tools/Scenes/Activate Auto Start Scene %&a")]
        private static void ActivateAutostart()
        {
            _isAutoBootstrapActive = true;
            
            PlayerPrefs.SetInt(AutoBootstrapActiveKey, 1);
            
            Debug.Log("Auto start scene activated");
        }
        
        [MenuItem("Tools/Scenes/Deactivate Auto Start Scene %&d")]
        private static void DeactivateAutostart() 
        {
            _isAutoBootstrapActive = false;
            
            PlayerPrefs.SetInt(AutoBootstrapActiveKey, 0);
            
            Debug.Log("Auto start scene deactivated");
        }
    }
}
#endif