using UnityEngine;
using System.Collections;
using Z3N;

public class GradeDisplay : MonoBehaviour {

	private UnityEngine.UI.Text mytext;
	private GameManager gameManager;

	// Use this for initialization
	void Start () {
	
		mytext = GetComponent<UnityEngine.UI.Text> ();
		gameManager = GameObject.Find ("GameManager").GetComponent<GameManager>();
	}
	
	// Update is called once per frame
	void LateUpdate () {

		if (gameManager.studentGrade < 50)
			mytext.text = "F";
		else if (gameManager.studentGrade < 55)
			mytext.text = "E";
		else if (gameManager.studentGrade < 60)
			mytext.text = "E+";
		else if (gameManager.studentGrade < 65)
			mytext.text = "D";
		else if (gameManager.studentGrade < 70)
			mytext.text = "D+";
		else if (gameManager.studentGrade < 75)
			mytext.text = "C";
		else if (gameManager.studentGrade < 80)
			mytext.text = "C+";
		else if (gameManager.studentGrade < 85)
			mytext.text = "B";
		else if (gameManager.studentGrade < 90)
			mytext.text = "B+";
		else if (gameManager.studentGrade < 95)
			mytext.text = "A";
		else
			mytext.text = "A+";

	}
}
