﻿/**
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
    public class ShapeDraw : MonoBehaviour
    {
        #region Structures
        /// <summary>
        /// Describes the points within a shape that the player has drawn.
        /// </summary>
        public struct SLinePoint
        {
            /// <summary>
            /// Viewport position of the point.
            /// </summary>
            public Vector2 viewPos;
            /// <summary>
            /// How hard the player was pressing when drawing this point.
            /// </summary>
            public float touchPressureMult;
            /// <summary>
            /// Whether this point is the end of a shape.
            /// </summary>
            public bool isShapeEnd;
            /// <summary>
            /// Creates the line point structure.
            /// </summary>
            /// <param name="a_viewPos">Viewport point.</param>
            /// <param name="a_touchPressureMult">How hard the player was pressing when drawing this point.</param>
            /// <param name="a_isShapeEnd">True if the point is the last one in the shape.</param>
            public SLinePoint(Vector2 a_viewPos, float a_touchPressureMult, bool a_isShapeEnd = false)
            {
                viewPos = a_viewPos;
                touchPressureMult = a_touchPressureMult;
                isShapeEnd = a_isShapeEnd;
            }

            public override string ToString()
            {
                return "Point: " + viewPos.ToString() + ", End shape: " + isShapeEnd;
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// How often to sample the player's drag input.
        /// </summary>
        public float inputSampleTime = 0.03f;

        /// <summary>
        /// Time between each drawn teacher point.
        /// </summary>
        public float teacherPlaybackStep = 0.03f;

        /// <summary>
        /// Depth to draw the line at, later replace this with raycasting.
        /// </summary>
        public float lineDrawDepth = 1.0f;

        /// <summary>
        /// Helps to debug the line points and for particles.
        /// </summary>
        public GameObject linePointPrefab = null;

        /// <summary>
        /// Points along the line in screen pixel coordinates.
        /// </summary>
        private List<SLinePoint> _linePoints;

        /// <summary>
        /// Last time the line was recorded.
        /// </summary>
        private float _lastLineRecordTime = 0.0f;

        /// <summary>
        /// Whether the drawing is currently playing back.
        /// </summary>
        private bool _isPlayingBackDrawing = false;

        /// <summary>
        /// Current end of the line.
        /// </summary>
        private Vector3 _lineEndWorldPt = Vector3.zero;

        /// <summary>
        /// Drawing script that created this shape drawing instance.
        /// </summary>
        private PlayerDraw _parentDrawScript = null;

        /// <summary>
        /// How far along the teacher playback is.
        /// </summary>
        private int _teacherPlaybackProgress = 0;

        /// <summary>
        /// Whether this shape is currently being drawn.
        /// </summary>
        private bool _isCurrentShape = false;

        // Cached variables
        private LineRenderer _lineRenderer = null;
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
            _lastLineRecordTime = 0.0f;
            _lineEndWorldPt = Vector3.zero;
            _isPlayingBackDrawing = false;
            _linePoints = new List<SLinePoint>();
            _lineRenderer = GetComponent<LineRenderer>();

            // Create line point holder if not around
            if (!_linePtHolder)
            {
                _linePtHolder = new GameObject("LinePointHolder");
                _linePtHolderTrans = _linePtHolder.transform;
            }

            // Clear the line
            ClearDrawnLine();
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
            if (_isCurrentShape)
            {
                // Touch position in pixel coordinates
                Vector2 touchPos = Vector2.zero;
                float touchPressureMult = 1.0f;
                bool isTouchDown = false;
                bool isTouchUp = false;

                // Pretend mouse input is touch
                if (Input.GetButton("Fire1"))
                {
                    touchPos = Input.mousePosition;
                    isTouchDown = true;
                }
                else if (Input.GetButtonUp("Fire1"))
                {
                    touchPos = Input.mousePosition;
                    isTouchUp = true;
                }

                // Calculate touch input
                if (Input.touchCount > 0)
                {
                    // Draw only using the first touch
                    Touch firstTouch = Input.touches[0];
                    isTouchDown = firstTouch.phase == TouchPhase.Began || firstTouch.phase == TouchPhase.Moved || firstTouch.phase == TouchPhase.Stationary;
                    isTouchUp = firstTouch.phase == TouchPhase.Ended || firstTouch.phase == TouchPhase.Canceled;
                    touchPos = firstTouch.position;

                    if (firstTouch.maximumPossiblePressure > 0.0f)
                    {
                        touchPressureMult = firstTouch.pressure / firstTouch.maximumPossiblePressure;
                    }
                    else
                    {
                        // Simulate pressure as touch velocity
                        touchPressureMult = 1.0f / firstTouch.deltaPosition.magnitude / firstTouch.deltaTime;
                    }
                }

                // Prevent drawing during playback
                if (!_isPlayingBackDrawing)
                {
                    if (isTouchUp)
                    {
                        // End the current shape
                        EndShape(touchPos, touchPressureMult);
                    }
                    else if (isTouchDown)
                    {
                        // Continue the current shape
                        if (Time.time - _lastLineRecordTime > inputSampleTime)
                        {
                            AddLinePoint(touchPos, touchPressureMult);
                            _lastLineRecordTime = Time.time;
                        }
                    }
                }
            }
        }
        #endregion

        #region Initialisation
        /// <summary>
        /// Attach handle for script callbacks.
        /// </summary>
        /// <param name="a_parentDrawScript">Handle to the script calling this shape draw.</param>
        public void SetDrawScriptHandle(PlayerDraw a_parentDrawScript)
        {
            _parentDrawScript = a_parentDrawScript;
        }

        /// <summary>
        /// Whether the current shape is active.
        /// </summary>
        /// <param name="a_isActive">True if active, false if not.</param>
        public void SetIsActiveShape(bool a_isActive)
        {
            _isCurrentShape = a_isActive;
        }
        #endregion

        #region Line drawing
        /// <summary>
        /// Clears the current line renderer.
        /// </summary>
        public void ClearDrawnLine()
        {
            _lineRenderer.SetVertexCount(0);
        }

        /// <summary>
        /// Begins the process of playing back the teacher's drawing.
        /// </summary>
        public void StartTeacherShapePlayback()
        {
            ClearDrawnLine();
            _teacherPlaybackProgress = 0;
            _isPlayingBackDrawing = true;

            if (_linePoints.Count > 0)
            {
                StartCoroutine(StepTeacherShapePlayback());
            }
        }

        private void AddLinePoint(Vector2 a_screenPoint, float a_touchPressureMult, bool a_isShapeEnd = false)
        {
            // Convert the screen point to a viewport point
            Vector2 viewPt = Camera.main.ScreenToViewportPoint(a_screenPoint);

            // Save the point
            SLinePoint newPt = new SLinePoint(viewPt, a_touchPressureMult, a_isShapeEnd);
            _linePoints.Add(newPt);

            // Connect the line dots
            DrawNewLinePointJoin(viewPt, _linePoints.Count, a_touchPressureMult);

            Debug.Log("Added point: " + newPt);
        }

        private void EndShape(Vector2 a_point, float a_touchPressureMult)
        {
            // Add that final point
            AddLinePoint(a_point, a_touchPressureMult, true);

            // TODO: If out of ink, time to compare score or show to student
            _parentDrawScript.DoneDrawingShape();
        }

        /// <summary>
        /// Draws a new join between the last added point and the previous.
        /// </summary>
        /// <param name="a_newWorldPoint">Point to draw the line to.</param>
        /// <param name="a_newPtCount">Current line point count.</param>
        /// <param name="a_touchPressureMult">Player touch pressure percentage.</param>
        private void DrawNewLinePointJoin(Vector3 a_newViewPt, int a_newPtCount, float a_touchPressureMult)
        {
            if (_linePoints.Count <= 0)
            {
                Debug.LogWarning("Unable to draw line join between 0 points!");
                return;
            }

            // TODO: Use raycasting (mask out all non-ground) instead of view port projection.

            // Map the viewport point to the world.
            Vector3 v3ViewPt = new Vector3(a_newViewPt.x, a_newViewPt.y, lineDrawDepth);
            Vector3 worldPt = Camera.main.ViewportToWorldPoint(v3ViewPt);

            // Create a point to show the line
            GameObject linePtObj = GameObject.Instantiate(linePointPrefab, worldPt, Quaternion.identity) as GameObject;
            linePtObj.transform.parent = _linePtHolderTrans;

            // TODO: Draw curved line using: http://www.habrador.com/tutorials/catmull-rom-splines/

            // Draw straight line between latest 2 points
            _lineRenderer.SetVertexCount(a_newPtCount);
            _lineRenderer.SetPosition(a_newPtCount - 1, worldPt);

            // Simulate basic 'running out of ink'
            _lineRenderer.SetWidth(0.01f * a_touchPressureMult, 1.0f / _linePoints.Count * a_touchPressureMult);

            // Store last world point for drawing the curve
            _lineEndWorldPt = worldPt;
        }

        /// <summary>
        /// Draws an individual line segment during the teacher's playback animation.
        /// </summary>
        /// <returns>Time to wait between each playback animation step.</returns>
        private IEnumerator StepTeacherShapePlayback()
        {
            SLinePoint currPt = _linePoints[_teacherPlaybackProgress];

            // Draw each line point
            DrawNewLinePointJoin(currPt.viewPos, _teacherPlaybackProgress + 1, currPt.touchPressureMult);

            ++_teacherPlaybackProgress;

            // Repeat until drawing is done
            if (_teacherPlaybackProgress < _linePoints.Count)
            {
                // Wait before replaying
                yield return new WaitForSeconds(teacherPlaybackStep);
                StartCoroutine(StepTeacherShapePlayback());
            }
            else
            {
                // Report back to the manager to trigger drawing the next shape.
                _isPlayingBackDrawing = false;
                _parentDrawScript.DonePlayingBackShape();
            }
        }
        #endregion
    }
}