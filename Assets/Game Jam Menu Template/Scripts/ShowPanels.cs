using UnityEngine;
using System.Collections;

public class ShowPanels : MonoBehaviour {

	public GameObject optionsPanel;							//Store a reference to the Game Object OptionsPanel 
	public GameObject optionsTint;							//Store a reference to the Game Object OptionsTint 
	public GameObject menuPanel;							//Store a reference to the Game Object MenuPanel 
	public GameObject pausePanel;							//Store a reference to the Game Object PausePanel 
    public GameObject difficultyPanel;                      //Store a reference to the Game Object DifficultyPanel 
    public GameObject difficultyTint;                       //Store a reference to the Game Object DifficultyTint 
    public GameObject infoPanel;                            //Store a reference to the Game Object InfoPanel 
    public GameObject infoTint;						        //Store a reference to the Game Object InfoTint 
    public GameObject creditsPanel;                         //Store a reference to the Game Object CreditsPanel

    //Call this function to activate and display the Options panel during the main menu
    public void ShowOptionsPanel()
	{
		optionsPanel.SetActive(true);
		optionsTint.SetActive(true);
	}

	//Call this function to deactivate and hide the Options panel during the main menu
	public void HideOptionsPanel()
	{
		optionsPanel.SetActive(false);
		optionsTint.SetActive(false);
	}

	//Call this function to activate and display the main menu panel during the main menu
	public void ShowMenu()
	{
		menuPanel.SetActive (true);
	}

	//Call this function to deactivate and hide the main menu panel during the main menu
	public void HideMenu()
	{
		menuPanel.SetActive (false);
	}
	
	//Call this function to activate and display the Pause panel during game play
	public void ShowPausePanel()
	{
		pausePanel.SetActive (true);
		optionsTint.SetActive(true);
	}

	//Call this function to deactivate and hide the Pause panel during game play
	public void HidePausePanel()
	{
		pausePanel.SetActive (false);
		optionsTint.SetActive(false);

	}

    //Call this function to activate and display the Difficulty panel during the main menu
    public void ShowDifficultyPanel()
    {
        difficultyPanel.SetActive(true);
        difficultyTint.SetActive(true);
    }

    //Call this function to deactivate and hide the Difficulty panel during the main menu
    public void HideDifficultyPanel()
    {
        difficultyPanel.SetActive(false);
        difficultyTint.SetActive(false);
    }

    //Call this function to activate and display the Info panel during the main menu
    public void ShowInfoPanel()
    {
        infoPanel.SetActive(true);
        infoTint.SetActive(true);
    }

    //Call this function to deactivate and hide the Info panel during the main menu
    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
        infoTint.SetActive(false);
    }

    //Call this function to deactivate and hide the Credits panel during the main menu
    public void ShowCreditsPanel()
    {
        creditsPanel.SetActive(true);
        infoTint.SetActive(true); // The credit panel uses the infoTint GameObject as they occupy the same position on screen
    }

    //Call this function to deactivate and hide the Credits panel during the main menu
    public void HideCreditsPanel()
    {
        creditsPanel.SetActive(false);
    }

}
