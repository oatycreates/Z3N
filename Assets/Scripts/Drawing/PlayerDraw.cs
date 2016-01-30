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
        #region Structures
        /// <summary>
        /// Describes the points within a shape that the player has drawn.
        /// </summary>
        public struct SLinePoint
        {
            /// <summary>
            /// Viewport position of the point.
            /// </summary>
            Vector2 viewPos;
            bool isShapeEnd;
            /// <summary>
            /// Creates the line point structure.
            /// </summary>
            /// <param name="a_viewPos">Viewport point.</param>
            /// <param name="a_isShapeEnd">True if the point is the last one in the shape.</param>
            public SLinePoint(Vector2 a_viewPos, bool a_isShapeEnd = false)
            {
                viewPos = a_viewPos;
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
        public float inputSampleTime = 0.1f;

        /// <summary>
        /// Depth to draw the line at, later replace this with raycasting.
        /// </summary>
        public float lineDrawDepth = 1.0f;

        /// <summary>
        /// Rendering 
        /// </summary>
        public GameObject linePointPrefab = null;

        /// <summary>
        /// Points along the line in screen pixel coordinates.
        /// </summary>
        protected List<SLinePoint> _linePoints;

        /// <summary>
        /// Last time the line was recorded.
        /// </summary>
        protected float _lastLineRecordTime = 0.0f;

        /// <summary>
        /// Current end of the line.
        /// </summary>
        protected Vector3 _lineEndWorldPt = Vector3.zero;

        // Cached variables
        private LineRenderer _lineRenderer = null;
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
            _linePoints = new List<SLinePoint>();
            _lineRenderer = GetComponent<LineRenderer>();
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
        #endregion

        #region Point drawing
        protected void AddLinePoint(Vector2 a_screenPoint, float a_touchPressureMult, bool a_isShapeEnd = false)
        {
            // Convert the screen point to a viewport point
            Vector2 viewPt = Camera.main.ScreenToViewportPoint(a_screenPoint);

            // Save the point
            SLinePoint newPt = new SLinePoint(viewPt, a_isShapeEnd);
            _linePoints.Add(newPt);

            // TODO: Use raycasting (mask out all non-ground) instead of view port projection.

            // Map the viewport point to the world.
            Vector3 v3ViewPt = new Vector3(viewPt.x, viewPt.y, lineDrawDepth);
            Vector3 worldPt = Camera.main.ViewportToWorldPoint(v3ViewPt);

            // Create a point to show the line
            GameObject.Instantiate(linePointPrefab, worldPt, Quaternion.identity);

            // Connect the line dots
            DrawNewLinePointJoin(worldPt, a_touchPressureMult);

            Debug.Log("Added point: " + newPt);
        }

        protected void EndShape(Vector2 a_point, float a_touchPressureMult)
        {
            // Add that final point
            AddLinePoint(a_point, a_touchPressureMult, true);

            // TODO: If out of ink, time to compare score or show to student
        }

        /// <summary>
        /// Draws a new join between the last added point and the previous.
        /// </summary>
        /// <param name="a_newWorldPoint">Point to draw the line to.</param>
        /// <param name="a_touchPressureMult">Player touch pressure percentage.</param>
        protected void DrawNewLinePointJoin(Vector3 a_newWorldPoint, float a_touchPressureMult)
        {
            if (_linePoints.Count <= 0)
            {
                Debug.LogWarning("Unable to draw line join between 0 points!");
                return;
            }

            // TODO: Draw curved line using: http://www.habrador.com/tutorials/catmull-rom-splines/

            // Draw straight line between latest 2 points
            _lineRenderer.SetVertexCount(_linePoints.Count);
            _lineRenderer.SetPosition(_linePoints.Count - 1, a_newWorldPoint);

            // Simulate basic 'running out of ink'
            _lineRenderer.SetWidth(0.01f * a_touchPressureMult, 1.0f / _linePoints.Count * a_touchPressureMult);

            // Store last world point for drawing the curve
            _lineEndWorldPt = a_newWorldPoint;
        }
        #endregion
    }
}