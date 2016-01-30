/**
 * File: PlayerDraw.cs
 * Author: Patrick Ferguson
 * Created: 29/01/2016
 * Copyright: (c) Team Z3N 2016.
 * License: Creative Commons Non Commercial Share Alike 3.0 free license.
 * Purpose: Allows the player to draw shapes using the mouse or touch input.
 **/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Z3N
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerDraw : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Number of seconds to keep the drawing up after completion.
        /// </summary>
        public float teacherPlaybackStayTime = 5.0f;

        /// <summary>
        /// Whether this player is the teacher (game host).
        /// </summary>
        public bool isTeacher = true;

        /// <summary>
        /// Whether the drawing is currently playing back.
        /// </summary>
        private bool _isPlayingBackDrawing = false;

        /// <summary>
        /// Prefab to use for the shape rendering.
        /// </summary>
        public GameObject shapePrefab = null;

        /// <summary>
        /// Shapes the player has drawn.
        /// </summary>
        private List<ShapeDraw> _drawnShapes;

        /// <summary>
        /// How far along the teacher playback is.
        /// </summary>
        private int _teacherShapePlaybackProgress = 0;

        // Cached variables
        private static GameObject _linePtHolder = null;
        private static Transform _linePtHolderTrans = null;
        #endregion

        #region Unity code
        /// <summary>
        /// Called when the entity is first ready.
        /// </summary>
        void Awake()
        {
            // Set default property values
            _drawnShapes = new List<ShapeDraw>();
            _isPlayingBackDrawing = false;

            // Create line point holder if not around
            if (!_linePtHolder)
            {
                _linePtHolder = new GameObject("LinePointHolder");
                _linePtHolderTrans = _linePtHolder.transform;
            }

            // Start the first shape
            CreateNextDrawingShape();
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
            // Simple code for the moment to simulate the triggering of the teacher's playback
            if (Input.touchCount >= 3 || Input.GetKeyUp(KeyCode.R))
            {
                if (isTeacher && !_isPlayingBackDrawing)
                {
                    // Editor to trigger teacher playback
                    StartTeacherPlayback();
                }
            }
        }
        #endregion

        #region Drawing shapes
        /// <summary>
        /// Called every time the latest shape has finished drawing.
        /// </summary>
        public void DonePlayingBackShape()
        {
            // Move on to the next shape
            ++_teacherShapePlaybackProgress;
            StartNextTeacherPlayback();
        }

        /// <summary>
        /// Called every time the latest shape has finished drawing.
        /// </summary>
        public void DoneDrawingShape()
        {
            // Turn off the old shape
            _drawnShapes[_drawnShapes.Count - 1].SetIsActiveShape(false);

            // Move on to the next shape
            CreateNextDrawingShape();
        }

        /// <summary>
        /// Starts the next shape for drawing.
        /// </summary>
        private void CreateNextDrawingShape()
        {
            GameObject newShapeObj = GameObject.Instantiate<GameObject>(shapePrefab);
            ShapeDraw newShape = newShapeObj.GetComponent<ShapeDraw>();
            newShape.SetDrawScriptHandle(this);
            newShape.SetIsActiveShape(true);
            newShapeObj.transform.parent = _linePtHolderTrans;

            _drawnShapes.Add(newShape);
        }

        /// <summary>
        /// Begins the process of playing back the teacher's drawing.
        /// </summary>
        private void StartTeacherPlayback()
        {
            ClearDrawnLines();
            _teacherShapePlaybackProgress = 0;
            _isPlayingBackDrawing = true;

            if (_drawnShapes.Count > 0)
            {
                StartNextTeacherPlayback();
            }
        }

        /// <summary>
        /// Draws a shape during the teacher's playback animation.
        /// </summary>
        private void StartNextTeacherPlayback()
        {
            if (_teacherShapePlaybackProgress < _drawnShapes.Count)
            {
                _drawnShapes[_teacherShapePlaybackProgress].StartTeacherShapePlayback();
            }
            else
            {
                // TODO: Once the whole drawing is done, flash up every shape for a few seconds and then hide.
                StartCoroutine(FinishTeacherPlayback());
            }
        }

        /// <summary>
        /// Hides the teacher's line after it has been drawn fully and a short wait has elapsed.
        /// </summary>
        /// <returns>Time to wait before hiding the finished shape.</returns>
        private IEnumerator FinishTeacherPlayback()
        {
            yield return new WaitForSeconds(teacherPlaybackStayTime);

            // Done playing back the line, hide
            ClearDrawnLines();
            _isPlayingBackDrawing = false;
        }

        /// <summary>
        /// Clears the current line renderer.
        /// </summary>
        /// <param name="a_cleanupShapes">Whether to delete the shapes when done.</param>
        private void ClearDrawnLines(bool a_cleanupShapes = false)
        {
            foreach (ShapeDraw shape in _drawnShapes)
            {
                shape.ClearDrawnLine();

                if (a_cleanupShapes)
                {
                    // TODO: Use object pooling for shape drawings.
                    GameObject.Destroy(shape.gameObject);
                }
            }
        }
        #endregion
    }
}