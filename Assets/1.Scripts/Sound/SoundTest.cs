using UnityEngine;
using _1.Scripts.Manager.Core;
using _1.Scripts.Manager.Subs;
using UnityEngine.InputSystem;

public class SoundTest : MonoBehaviour
{
    private SoundManager soundManager;
    private UIManager uiManager;
    private bool bgmPlayed = false;
    private bool settingPopupShowed = false;

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
            if (!settingPopupShowed)
            {
                uiManager.ShowSettingPopup();
                settingPopupShowed = true;
            }
            else
            {
                uiManager.ClosePopup();
                settingPopupShowed = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (!bgmPlayed)
            {
                Service.Log("Sound Test: Play BGM");
                soundManager.PlayBGM(BgmType.Lobby, 0);
                bgmPlayed = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (bgmPlayed)
            {
                Service.Log("Sound Test: Stop BGM");
                soundManager.StopBGM();
                bgmPlayed = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (soundTest2 != null)
            {
                soundManager.PlaySFX(SfxType.Test, soundTest2.position, 0);
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (soundTest != null)
            {
                soundManager.PlaySFX(SfxType.Test, soundTest.position, 0);
            }
        }
    }
}