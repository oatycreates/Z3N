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
	
		allSymbolsArray = Resources.LoadAll<Sprite>("Symbols");

		foreach (Sprite s in allSymbolsArray)
		{
			allSymbols.Add (s);
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild (i).transform.Find("Symbol") != null) 
			{
				myImages.Add (transform.GetChild (i).transform.Find("Symbol").GetComponent<Image>());
			}
		}

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
