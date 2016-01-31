/**
 * File: GameManager.cs
 * Author: Patrick Ferguson
 * Created: 30/01/2016
 * Copyright: (c) Team Z3N 2016.
 * License: Creative Commons Non Commercial Share Alike 3.0 free license.
 * Purpose: General game manager for switching game states.
 **/
using UnityEngine;
using System.Collections;

namespace Z3N
{
    public class GameManager : MonoBehaviour
    {
        #region Variables
        public PlayerDraw teacherScript = null;
        public PlayerDraw studentScript = null;
        public ShowPanels gameUIShowScript = null;
        public GameObject studentReadyBtn = null;

        private bool hasStudentDrawn = false;
        #endregion

        #region Unity code
        /// <summary>
        /// Called when the entity is first ready.
        /// </summary>
        void Awake()
        {
            hasStudentDrawn = false;
            studentReadyBtn.SetActive(false);
        }

        /// <summary>
        /// Use this for initialisation.
        /// </summary>
        void Start()
        {

        }

        /// <summary>
        /// Update is called at the start of each frame.
        /// </summary>
        void Update()
        {

        }
        #endregion
        
        #region Player switching
        /// <summary>
        /// Switches from teacher to student, and visa versa.
        /// </summary>
        public void SwitchToOtherPlayer(bool a_isTeacher)
        {
            if (a_isTeacher)
            {
                Debug.Log("Switch to student");
                // Switch from teacher to student
                studentScript.enabled = true;
                teacherScript.enabled = false;
            }
            else
            {
                Debug.Log("Switch to teacher");

                if (!hasStudentDrawn)
                {
                    // Show the teacher's drawing
                    teacherScript.DrawAllShapesInstant();

                    // Start the student playback
                    studentScript.StartTeacherPlayback();
                    hasStudentDrawn = true;
                }
                else
                {
                    hasStudentDrawn = false;

                    // Score the team
                    gameUIShowScript.ShowScorePanel();
                }
            }
        }

        public void StudentReadyPressed()
        {
            studentReadyBtn.SetActive(false);
            Debug.Log("Start teacher playback");
            teacherScript.StartTeacherPlayback();
        }

        /// <summary>
        /// Called when the user clicks the submit button.
        /// </summary>
        public void SubmitButtonPressed()
        {
            if (teacherScript.enabled && !teacherScript.GetIsPlayingBack())
            {
                Debug.Log("Show student menu");
                studentReadyBtn.SetActive(true);
            }
            else if (studentScript.enabled)
            {
                Debug.Log("Start team score showcase");
                // Show the 'Team score' window
                SwitchToOtherPlayer(false);
            }
        }
        #endregion
    }
}