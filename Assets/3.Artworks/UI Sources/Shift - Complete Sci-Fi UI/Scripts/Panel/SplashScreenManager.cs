using UnityEngine;

namespace Michsky.UI.Shift
{
    public class SplashScreenManager : MonoBehaviour
    {
        [Header("Resources")]
        public GameObject splashScreen;
        public GameObject mainPanels;

        private Animator splashScreenAnimator;
        private Animator mainPanelsAnimator;
        private TimedEvent ssTimedEvent;

        [Header("Settings")]
        public bool disableSplashScreen;
        public bool enablePressAnyKeyScreen;
        public bool enableLoginScreen;
        public bool showOnlyOnce = true;

        MainPanelManager mpm;
        void Awake()
        {
            // ① 필드 누락 시 에러 찍고 스크립트 비활성화
            if (splashScreen == null || mainPanels == null)
            {
                Debug.LogError("SplashScreenManager: splashScreen/mainPanels 필드를 인스펙터에서 할당하세요.");
                enabled = false;
                return;
            }

            // ② MainPanelManager 캐싱 (null이면 Find 방식으로도 변경 가능)
            mpm = GetComponent<MainPanelManager>();
            if (mpm == null)
            {
                Debug.LogError("SplashScreenManager: 같은 GameObject에 MainPanelManager를 붙여주세요.");
                enabled = false;
                return;
            }

            // ③ Animator 캐싱
            splashScreenAnimator = splashScreen.GetComponent<Animator>();
            mainPanelsAnimator   = mainPanels.GetComponent<Animator>();

            // ④ disableSplashScreen=true 면 스플래시 자체를 뛰어넘고 리턴
            if (disableSplashScreen)
            {
                splashScreen.SetActive(false);
                mainPanels.SetActive(true);
                mpm.OpenFirstTab();
                return;
            }
        }        
        void OnEnable()
        {
            if (showOnlyOnce && GameObject.Find("[Shift UI - Splash Screen Helper]") != null) { disableSplashScreen = true; }
            if (splashScreenAnimator == null) { splashScreenAnimator = splashScreen.GetComponent<Animator>(); }
            if (ssTimedEvent == null) { ssTimedEvent = splashScreen.GetComponent<TimedEvent>(); }
            if (mainPanelsAnimator == null) { mainPanelsAnimator = mainPanels.GetComponent<Animator>(); }
            if (mpm == null) { mpm = gameObject.GetComponent<MainPanelManager>(); }

            if (disableSplashScreen == true)
            {
                splashScreen.SetActive(false);
                mainPanels.SetActive(true);

                //mainPanelsAnimator.Play("Start");
                mpm.OpenFirstTab();
            }

            if (enableLoginScreen == false && enablePressAnyKeyScreen == true && disableSplashScreen == false)
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
            }

            if (enableLoginScreen == true && enablePressAnyKeyScreen == true && disableSplashScreen == false)
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
            }

            if (enableLoginScreen == true && enablePressAnyKeyScreen == false && disableSplashScreen == false)
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
                splashScreenAnimator.Play("Login");
            }

            if (enableLoginScreen == false && enablePressAnyKeyScreen == false && disableSplashScreen == false)
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
                /*splashScreenAnimator.Play("Loading");
                ssTimedEvent.StartIEnumerator();*/
            }

            if (showOnlyOnce == true && disableSplashScreen == false)
            {
                GameObject tempHelper = new GameObject();
                tempHelper.name = "[Shift UI - Splash Screen Helper]";
                DontDestroyOnLoad(tempHelper);
            }
        }

        public void LoginScreenCheck()
        {
            if (enableLoginScreen == true && enablePressAnyKeyScreen == true)
                splashScreenAnimator.Play("Press Any Key to Login");

            else if (enableLoginScreen == false && enablePressAnyKeyScreen == true)
            {
                splashScreenAnimator.Play("Press Any Key to Loading");
                ssTimedEvent.StartIEnumerator();
            }

            /*else if (enableLoginScreen == false && enablePressAnyKeyScreen == false)
            {
                splashScreenAnimator.Play("Loading");
                ssTimedEvent.StartIEnumerator();
            }*/
        }
    }
}