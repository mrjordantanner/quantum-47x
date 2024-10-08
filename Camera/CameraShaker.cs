using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using System.Threading.Tasks.Sources;

[Serializable]
public class CameraShaker : MonoBehaviour
{
    #region Singleton
    public static CameraShaker Instance;
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
    }
    #endregion

    public enum ShakeStyle { Small, Medium, Large }

    [Header("Default Camera Jitter")]
    public float defaultAmplitude = 0.85f;
    public float defaultFrequency = 0.35f;

    [Header("Small")]
    public float s_Duration;
    public float s_Amplitude;
    public float s_Frequency;

    [Header("Medium")]
    public float m_Duration;
    public float m_Amplitude;
    public float m_Frequency;

    [Header("Large")]
    public float l_Duration;
    public float l_Amplitude;
    public float l_Frequency;

    [Header("Other options")]
    public Transform camStartPosition;

    CinemachineVirtualCamera cam;
    CinemachineBasicMultiChannelPerlin noise;

    private void Start()
    {
        cam = FindObjectOfType<CinemachineVirtualCamera>();
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        ResetCameraShake();
    }

 
    public void Shake(ShakeStyle style)
    {
        float dur = 0, amp = 0, freq = 0;

        switch(style)
        {
            case ShakeStyle.Small:
                dur = s_Duration;
                amp = s_Amplitude;
                freq = s_Frequency;
                break;

            case ShakeStyle.Medium:
                dur = m_Duration;
                amp = m_Amplitude;
                freq = m_Frequency;
                break;

            case ShakeStyle.Large:
                dur = l_Duration;
                amp = l_Amplitude;
                freq = l_Frequency;
                break;
        }

        noise.m_AmplitudeGain = amp;
        noise.m_FrequencyGain = freq;
        Invoke(nameof(ResetCameraShake), dur);
    }

    void ResetCameraShake()
    {
        noise.m_AmplitudeGain = defaultAmplitude;
        noise.m_FrequencyGain = defaultFrequency;
    }




 }
