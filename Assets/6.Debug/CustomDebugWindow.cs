using _1.Scripts.Entity.Scripts.Player.Core;
using _1.Scripts.Entity.Scripts.Player.Data;
using UnityEditor;
using UnityEngine;

namespace _6.Debug
{
    public class CustomDebugWindow : EditorWindow
    {
        private GameObject playerObj;

        [MenuItem("Window/Custom Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<CustomDebugWindow>("Debug Window");
        }

        private void OnGUI()
        {
            GUILayout.Label("Custom Debug Tool", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Recover Focus Gauge"))
            {
                playerObj = GameObject.FindWithTag("Player");
                if (!playerObj.TryGetComponent(out Player playerComponent)) return;
                playerComponent.PlayerCondition.OnRecoverFocusGauge(FocusGainType.Debug);
            }

            if (GUILayout.Button("Recover Instinct Gauge"))
            {
                playerObj = GameObject.FindWithTag("Player");
                if (!playerObj.TryGetComponent(out Player playerComponent)) return;
                playerComponent.PlayerCondition.OnRecoverInstinctGauge(InstinctGainType.Debug);
            }
        }
    }
}