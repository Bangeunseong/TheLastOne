using UnityEngine;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine.InputSystem;

public class SoundTest : MonoBehaviour
{
    private SoundManager soundManager;
    private UIManager uiManager;
    private bool bgmPlayed = false;

    public Transform soundTest;
    public Transform soundTest2;

    
    void Start()
    {
        soundManager = CoreManager.Instance.soundManager;
        uiManager = CoreManager.Instance.uiManager;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.ShowSettingPopup();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!bgmPlayed)
            {
                soundManager.PlayBGM("LobbyBGM");
                bgmPlayed = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (bgmPlayed)
            {
                soundManager.StopBGM();
                bgmPlayed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (soundTest2 != null)
            {
                soundManager.PlaySFX("Voice_Male_V1_Attack_Mono_03", soundTest2.position);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (soundTest != null)
            {
                soundManager.PlaySFX("Voice_Male_V1_Attack_Mono_03", soundTest.position);
            }
        }
    }
}