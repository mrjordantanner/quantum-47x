using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class VFX : MonoBehaviour
{
    #region Singleton
    public static VFX Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        #endregion

        Init();
    }

    [HideInInspector]
    public GameObject VFXContainer;
    public Volume GlobalConstantEffects;
    public Material hitFlashMaterial;
    public Material shootFlashMaterial;
    public Material imageFlashMaterial;

    [HideInInspector] public ColorAdjustments colorAdjustments;

    void Init()
    {
        VFXContainer = new GameObject("VFXContainer");
    }

    public IEnumerator StartDamageEffects()
    {
        HUD.Instance.ScreenFlash();
        yield return new WaitForSeconds(0);
    }
}
