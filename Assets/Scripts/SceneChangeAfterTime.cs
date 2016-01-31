using UnityEngine;
using System.Collections;

public class SceneChangeAfterTime : MonoBehaviour
{
    public int sceneToChangeTo = 0;
    public float changeWaitSecs = 4.0f;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(ChangeScene());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(changeWaitSecs);

        Application.LoadLevel(sceneToChangeTo);
    }
}
