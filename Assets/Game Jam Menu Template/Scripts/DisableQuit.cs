using UnityEngine;
using System.Collections;

public class DisableQuit : MonoBehaviour
{
    [SerializeField]
    GameObject _objToDisable;
    [SerializeField]
    GameObject _objToDisable2;
    //Use this for initialization
    void Start()
    {
        #if UNITY_IPHONE && UNITY_IOS
            _objToDisable.SetActive(false);
            _objToDisable2.SetActive(false);
        #endif
    }

    //Update is called once per frame
    void Update()
    {

    }
}
