using UnityEngine;
using System.Collections;

public class DisableAfterTime : MonoBehaviour {
    public float destroyWaitSecs = 4.0f;

	// Use this for initialization
	void Start () {
        StartCoroutine(DisableObj());
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private IEnumerator DisableObj()
    {
        yield return new WaitForSeconds(destroyWaitSecs);

        gameObject.SetActive(false);
    }
}
