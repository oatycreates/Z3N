using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Symbols : MonoBehaviour {

	private List <Image> myImages = new List <Image>();

	private Sprite[] allSymbolsArray;

	private List <Sprite> allSymbols = new List<Sprite>();

	public List <Sprite> symbols = new List<Sprite>();

	// Use this for initialization
	void Start () {
	
		// Load all sprites from the symbols folder into a preliminary array
		allSymbolsArray = Resources.LoadAll<Sprite>("Symbols");

		// Transfer the preliminary allSymbols array to the allSymbols list
		foreach (Sprite s in allSymbolsArray)
		{
			allSymbols.Add (s);
		}

		// Find all the Image components in the children of this transform
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild (i).transform.Find("Symbol") != null) 
			{
				myImages.Add (transform.GetChild (i).transform.Find("Symbol").GetComponent<Image>());
			}
		}

		// Assign a random sprite from the allSymbols list to each image in myImages and remove each sprite when assigned
		foreach (Image i in myImages)
		{
			int r = Random.Range (0,allSymbols.Count);
			i.sprite = allSymbols [r];
			symbols.Add (allSymbols[r]);
			if (allSymbols.Count > 1)
				allSymbols.Remove (allSymbols[r]);
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
