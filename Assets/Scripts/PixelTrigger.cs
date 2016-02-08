using UnityEngine;
using System.Collections;
using Z3N;

public class PixelTrigger : MonoBehaviour {

	private bool touched = false;

	private GameManager myGameManager;

	public LayerMask myLayer;

	void Awake ()
	{
		myGameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

        Ray ray;
        if (Input.touchSupported)
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        else
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, myLayer))
        {
            //Debug.Log(hit.collider.transform.gameObject.name);
            if (hit.collider == transform.GetComponent<MeshCollider>())
            {
                if (!touched && myGameManager.teacherScript.enabled)
                {
                    if (Input.GetMouseButton(0))
                    {
                        touched = true;
                        myGameManager.teacherPixels += 1;
                    }
                }
                if (touched && myGameManager.studentScript.enabled)
                {
                    if (Input.GetMouseButton(0))
                    {
                        touched = false;
                        myGameManager.studentPixels += 1;
                    }
                }
            }
        }
	}
}
