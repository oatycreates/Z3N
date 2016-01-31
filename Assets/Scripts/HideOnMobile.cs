using UnityEngine;
using System.Collections;

public class HideOnMobile : MonoBehaviour {

	// Use this for initialization
    void Awake ()
    {
#if UNITY_IOS || UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1
        gameObject.SetActive(false);
#endif
    }

    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
