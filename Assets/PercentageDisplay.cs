using UnityEngine;
using System.Collections;
using Z3N;

public class PercentageDisplay : MonoBehaviour {

	private UnityEngine.UI.Text myText;
	private GameManager gameManager;
	private int studentPercentage;

	// Use this for initialization
	void Start () {
	
		myText = GetComponent<UnityEngine.UI.Text> ();
		gameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void LateUpdate () {

		myText.text = gameManager.studentGrade + "%";

	}
}
