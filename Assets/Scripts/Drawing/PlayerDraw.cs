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
    public class PlayerDraw : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Number of seconds to keep the drawing up after completion.
        /// </summary>
        public float teacherPlaybackStayTime = 5.0f;
        /// <summary>
        /// Is the player currently drawing a line
        /// </summary>
        static public bool s_isDrawing;

        /// <summary>
        /// Line colour to use.
        /// </summary>
        public Color lineCol = Color.magenta;

        /// <summary>
        /// Object that is following the current shape.
        /// </summary>
        public Transform followObjTrans = null;
        /// <summary>
        /// The ink bar
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image _inkBar;
        /// <summary>
        /// The total number of ink the can be used
        /// </summary>
        [SerializeField]
        private int _inkCount = 5;

        private int _inkRemaining = 5;

        /// <summary>
        public Sprite[] _inkSprites;
        /// <summary>
        /// /// YinYang UI Image to display ink sprites
        /// <summary> 
        /// Handle to the game manager.
        /// </summary>
        public GameManager gameManScript = null;

        /// <summary>
        /// Lerp value per second.
        /// </summary>
        public float followObjSpeed = 1.0f;

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

        private Vector3 _followObjStartPos = Vector3.zero;

        // Cached variables
        private static GameObject _shapeHolder = null;
        private static Transform _shapeHolderTrans = null;
        #endregion

        #region Getter/Setter
        public bool GetIsPlayingBack()
        {
            return _isPlayingBackDrawing;
        }
        /// <summary>
        /// Return true if there is more ink the player can use
        /// </summary>
        public bool InkNotEmpty
        {
            get { return (_inkBar.fillAmount > 0.01f); }
        }
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
            _followObjStartPos = followObjTrans.position;

            // Create line point holder if not around
            if (!_shapeHolder)
            {
                _shapeHolder = new GameObject("ShapeHolder");
                _shapeHolderTrans = _shapeHolder.transform;
            }
            _inkBar.color = lineCol;
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
            bool isInteracting = (Input.touchCount > 1 || Input.GetMouseButton(0));
            if(_inkBar.fillAmount > 0.001f && isInteracting)
            {
                _inkBar.fillAmount -= (Time.deltaTime / _inkCount);
            }
            // Simple code for the moment to simulate the triggering of the teacher's playback
            /*
            if (Input.touchCount == 3 || Input.GetKeyUp(KeyCode.R))
            if (gameObject.activeSelf && isActiveAndEnabled)
            {
                // Simple code for the moment to simulate the triggering of the teacher's playback
                if (Input.touchCount == 3 || Input.GetKeyUp(KeyCode.R))
                {
                    if (isTeacher && !_isPlayingBackDrawing)
                    {
                        // Editor to trigger teacher playback
                        StartTeacherPlayback();
                    }
                }
            }
            */
            //Debug.Log(_inkBar.value);

            if (/*Input.touchCount == 4 ||*/ Input.GetKeyUp(KeyCode.Escape))
            {
                Application.LoadLevel(0);
            }
        }

        /// <summary>
        /// Resets the inkbar on enable
        /// </summary>
        void OnEnable()
        {
            _inkBar.fillAmount = 1.0f;
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
            // Move on to the next shape
            CreateNextDrawingShape();
        }

        /// <summary>
        /// Starts the next shape for drawing.
        /// </summary>
        private void CreateNextDrawingShape()
        {
            if (_inkBar.fillAmount > 0.01f)
            {
                // Set up the new shape
                GameObject newShapeObj = GameObject.Instantiate<GameObject>(shapePrefab);
                ShapeDraw newShape = newShapeObj.GetComponent<ShapeDraw>();
                newShape.SetDrawScriptHandle(this);
                newShape.SetFollowObjHandle(followObjTrans, followObjSpeed, _followObjStartPos);
                newShape.SetIsActiveShape(true);
                newShapeObj.transform.parent = _shapeHolderTrans;

                // Set line colour
                LineRenderer lineRen = newShapeObj.GetComponent<LineRenderer>();
                lineRen.SetColors(lineCol, lineCol);
                Renderer ren = newShapeObj.GetComponent<Renderer>();
                ren.material.color = lineCol;

                _drawnShapes.Add(newShape);
            }
        }

        /// <summary>
        /// Begins the process of playing back the teacher's drawing.
        /// </summary>
        public void StartTeacherPlayback()
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
                // Only animate valid shapes
                if (_drawnShapes[_teacherShapePlaybackProgress].GetShapeHasStarted())
                {
                    _drawnShapes[_teacherShapePlaybackProgress].StartTeacherShapePlayback();
                }
                else
                {
                    // Go to the next shape
                    DonePlayingBackShape();
                }
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

            gameManScript.SwitchToOtherPlayer(isTeacher);
        }

        /// <summary>
        /// Clears the current line renderer.
        /// </summary>
        /// <param name="a_cleanupShapes">Whether to delete the shapes when done.</param>
        public void ClearDrawnLines(bool a_cleanupShapes = false)
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

        /// <summary>
        /// Instantly draw all shapes.
        /// </summary>
        public void DrawAllShapesInstant()
        {
            foreach (ShapeDraw shape in _drawnShapes)
            {
                shape.DrawShapeInstant();
            }
        }
        #endregion
    }
}