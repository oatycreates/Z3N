using UnityEngine;
using System.Collections;

public class SoundFading : MonoBehaviour
{
    static public float _fadeIn = 1.0f;
    [SerializeField]
    AudioClip _failing;
    [SerializeField]
    AudioClip _succeeding;
    [SerializeField]
    AudioSource _aS;
    [SerializeField]
    AudioSource _aS1;
    [SerializeField]
    private float _volumeController;
    // Use this for initialization
    void Start()
    {
        _volumeController = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        float fadeVal = (_fadeIn / 2);
        _aS.volume = (fadeVal * _volumeController);
        _aS1.volume = (1.0f - fadeVal) * _volumeController;
        if (/* Z3N.PlayerDraw.s_isDrawing */ Input.GetMouseButton(0))
        {
            MakingNoise();
            _volumeController = 1.0f;
        }
        else
        {
            _volumeController -= 0.02f;
        }
    }
    private void MakingNoise()
    {
        if(!_aS.isPlaying)
            _aS.PlayOneShot(_succeeding);
        if (!_aS1.isPlaying && _fadeIn < 0.25f)
            _aS1.PlayOneShot(_failing);
    }
} 
