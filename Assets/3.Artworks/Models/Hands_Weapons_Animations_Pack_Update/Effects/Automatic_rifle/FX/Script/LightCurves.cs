using UnityEngine;
using _1.Scripts.Manager.Core;

public class LightCurves : MonoBehaviour
{
    public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float GraphTimeMultiplier = 1, GraphIntensityMultiplier = 1;
    public bool IsLoop;

    [HideInInspector] public bool canUpdate;
    private float startTime;
    private Light lightSource;
    private CoreManager coreManager;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
        lightSource.intensity = LightCurve.Evaluate(0);
    }

    private void OnEnable()
    {
        startTime = 0f;
        canUpdate = true;
    }

    private void Start()
    {
        coreManager = CoreManager.Instance;
    }

    private void Update()
    {
        if (canUpdate) {
            var eval = LightCurve.Evaluate(startTime / GraphTimeMultiplier) * GraphIntensityMultiplier;
            lightSource.intensity = eval;
        }

        if (startTime < GraphTimeMultiplier)
        {
            if(!coreManager.gameManager.IsGamePaused) startTime += Time.unscaledDeltaTime;
        }
        else
        {
            if (IsLoop) startTime = 0f;
            canUpdate = false;
        }
    }
}