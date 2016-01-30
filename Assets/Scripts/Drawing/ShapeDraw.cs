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
        /// Speed that the teacher playback lerps between each point per second.
        /// </summary>
        public float teacherPlaybackRate = 1.0f;

        /// <summary>
        /// Thickness of the line.
        /// </summary>
        public float maxLineThickness = 0.1f;

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
        /// Object that is following the current shape.
        /// </summary>
        private Transform _followObjTrans = null;

        /// <summary>
        /// Start position of the follow object.
        /// </summary>
        private Vector3 _followObjStartPos = Vector3.zero;

        /// <summary>
        /// Lerp value per second.
        /// </summary>
        private float _followObjSpeed = 1.0f;

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
        /// Lerps between this point and the current line point.
        /// </summary>
        private Vector2 _lastPlaybackViewPt = Vector2.zero;

        /// <summary>
        /// Current progress between line segments.
        /// </summary>
        private float _linePlaybackProgress = 0.0f;

        /// <summary>
        /// Rate to fade the playback line out at.
        /// </summary>
        private float _playbackLineFadeRate = 1.0f;

        /// <summary>
        /// Progress towards totally fading out the line post playback.
        /// </summary>
        private float _playbackLineFadeProgress = 0.0f;

        /// <summary>
        /// Whether this shape is currently being drawn.
        /// </summary>
        private bool _isCurrentShape = false;

        /// <summary>
        /// Layers to accept when raycasting for checking player input.
        /// Get 'DrawableArea'.
        /// </summary>
        private LayerMask _lineRaycastDesired = LayerMask.NameToLayer("DrawableArea");

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
            if (_isPlayingBackDrawing)
            {
                StepTeacherShapePlayback();
            }

            if (_isCurrentShape && _parentDrawScript.gameObject.activeSelf && _parentDrawScript.isActiveAndEnabled)
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
                    PlayerDraw.s_isDrawing = isTouchDown;
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

                /*Ray tempRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
                RaycastHit[] rays = Physics.RaycastAll(tempRay, 300.0f, (~_lineRaycastDesired));
                Debug.DrawRay(tempRay.origin, tempRay.direction * 300.0f, rays.Length > 0 ? Color.green : Color.red);
                for (int i = 0; i < rays.Length; ++i)
                {
                    Debug.Log("Hit " + rays[i].transform.gameObject.name);
                }*/

                // Prevent drawing during playback
                if (!_isPlayingBackDrawing)
                {
                    Vector2 viewPt = Camera.main.ScreenToViewportPoint(touchPos);
                    if (isTouchUp)
                    {
                        // End the current shape
                        EndShape(viewPt, touchPressureMult);
                    }
                    else if (isTouchDown)
                    {
                        // Sample at an interval
                        if (Time.time - _lastLineRecordTime > inputSampleTime)
                        {
                            // Prevent the player from drawing off the sand
                            Vector3 v3ViewPt = new Vector3(viewPt.x, viewPt.y, lineDrawDepth);
                            Ray viewRay = Camera.main.ViewportPointToRay(v3ViewPt);
                            if (RayIsOnGround(viewRay, _lineRaycastDesired))
                            {
                                // Continue the current shape
                                AddLinePoint(viewPt, touchPressureMult);
                                _lastLineRecordTime = Time.time;
                            }
                            /*else if (_linePoints.Count > 0)
                            {
                                // Off the sand, end the current shape
                                EndShape(viewPt, touchPressureMult);
                            }*/
                        }
                    }
                }
            }
            /*else if (!_isPlayingBackDrawing && _playbackLineFadeProgress < 1.0f)
            {
                // Fade the line out
                _playbackLineFadeProgress += _playbackLineFadeRate * Time.deltaTime;
                float currFadeOutMult = (1.0f - _playbackLineFadeProgress) * maxLineThickness;
                // Simulate basic 'running out of ink'
                _lineRenderer.SetWidth(currFadeOutMult * maxLineThickness * (1.0f / _linePoints.Count), currFadeOutMult * maxLineThickness);
            }*/
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
        /// Set object to follow the trail.
        /// </summary>
        /// <param name="a_followObjTrans">Transform</param>
        /// <param name="a_followSpeed">Speed</param>
        public void SetFollowObjHandle(Transform a_followObjTrans, float a_followSpeed)
        {
            _followObjTrans = a_followObjTrans;
            _followObjSpeed = a_followSpeed;
            
            // Start the follow person at their starting position
            _lineEndWorldPt = _followObjTrans.position;
            _followObjStartPos = _followObjTrans.position;
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
            if (_linePoints.Count > 0)
            {
                _isPlayingBackDrawing = true;

                _linePlaybackProgress = 1.0f;
                _lastPlaybackViewPt = _linePoints[0].viewPos;
            }
        }

        /// <summary>
        /// Whether this shape has any content in it.
        /// </summary>
        /// <returns>True if started, false if not.</returns>
        public bool GetShapeHasStarted()
        {
            return _linePoints.Count > 0;
        }

        private void AddLinePoint(Vector2 a_viewPt, float a_touchPressureMult, bool a_isShapeEnd = false)
        {
            // Save the point
            SLinePoint newPt = new SLinePoint(a_viewPt, a_touchPressureMult, a_isShapeEnd);
            _linePoints.Add(newPt);

            // Connect the line dots
            DrawNewLinePointJoin(a_viewPt, _linePoints.Count, a_touchPressureMult);

#if UNITY_EDITOR
            Debug.Log("Added point: " + newPt);
#endif
        }

        private void EndShape(Vector2 a_viewPoint, float a_touchPressureMult)
        {
            // Add that final point
            AddLinePoint(a_viewPoint, a_touchPressureMult, true);

            // TODO: If out of ink, time to compare score or show to student

            // Notify the parent that this shape is done
            _isCurrentShape = false;
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
            
            // Map the viewport point to the world
            Vector3 v3ViewPt = new Vector3(a_newViewPt.x, a_newViewPt.y, lineDrawDepth);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ViewportPointToRay(v3ViewPt), out hit, 1000.0f))
            {
                Vector3 worldPt = hit.point;

                // Create a point to show the line
                GameObject linePtObj = GameObject.Instantiate(linePointPrefab, worldPt, Quaternion.identity) as GameObject;
                linePtObj.transform.parent = _linePtHolderTrans;

                // TODO: Draw curved line using: http://www.habrador.com/tutorials/catmull-rom-splines/

                // Draw straight line between latest 2 points
                _lineRenderer.SetVertexCount(a_newPtCount);
                _lineRenderer.SetPosition(a_newPtCount - 1, worldPt);

                // Simulate basic 'running out of ink'
                _lineRenderer.SetWidth(maxLineThickness * (1.0f / _linePoints.Count) * a_touchPressureMult, maxLineThickness * a_touchPressureMult);

                // Store last world point for drawing the curve
                _lineEndWorldPt = worldPt;

                UpdatePlayerFollow();
            }
        }

        /// <summary>
        /// Lerps to an individual line segment during the teacher's playback animation.
        /// </summary>
        private void StepTeacherShapePlayback()
        {
            SLinePoint currPt = _linePoints[_teacherPlaybackProgress];

            // Lerp from current point to new point
            _linePlaybackProgress += teacherPlaybackRate * Time.deltaTime / Vector2.Distance(_lastPlaybackViewPt, currPt.viewPos);
            _linePlaybackProgress = Mathf.Min(_linePlaybackProgress, 1.0f);
            Vector2 newViewPos = Vector2.Lerp(_lastPlaybackViewPt, currPt.viewPos, _linePlaybackProgress);

            // Draw each line point
            DrawNewLinePointJoin(newViewPos, _teacherPlaybackProgress + 1, currPt.touchPressureMult);

            if (_linePlaybackProgress >= 1.0f)
            {
                _linePlaybackProgress = 0.0f;
                _lastPlaybackViewPt = currPt.viewPos;
                
                // Repeat until drawing is done
                ++_teacherPlaybackProgress;
                if (_teacherPlaybackProgress >= _linePoints.Count)
                {
                    // Report back to the manager to trigger drawing the next shape.
                    _isPlayingBackDrawing = false;
                    UpdatePlayerFollow();
                    _parentDrawScript.DonePlayingBackShape();
                }
            }
        }

        /// <summary>
        /// Returns whether the input ray is hitting the desired object.
        /// </summary>
        /// <param name="a_viewRay"></param>
        /// <param name="a_desiredLayer"></param>
        /// <returns></returns>
        private bool RayIsOnGround(Ray a_viewRay, LayerMask a_desiredLayer)
        {
            RaycastHit hitInfo = new RaycastHit();
            // Find the first object hit
            bool hitSomething = Physics.Raycast(a_viewRay, out hitInfo, 1000.0f);
            return hitSomething ? hitInfo.transform.gameObject.layer.Equals(a_desiredLayer.value) : false;
        }
        #endregion

        #region Player following
        private void UpdatePlayerFollow()
        {
            if (_followObjTrans)
            {
                if (_isPlayingBackDrawing)
                {
                    // Lerp towards the target position
                    Vector3 newPos = _lineEndWorldPt;// Vector3.Lerp(_followObjTrans.position, _lineEndWorldPt, _followObjSpeed * Time.deltaTime);
                    _followObjTrans.position = newPos;
                }
                else if (_followObjTrans.gameObject.name.Contains("Teacher"))
                {
                    // Hide the teacher's finger unless it is playing back
                    _followObjTrans.position = _followObjStartPos;
                }
            }
        }
        #endregion
    }
}