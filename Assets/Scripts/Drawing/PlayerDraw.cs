/**
 * File: PlayerDraw.cs
 * Author: Patrick Ferguson
 * Created: 29/01/2016
 * Copyright: (c) Team Z3N 2016.
 * License: Creative Commons Non Commercial Share Alike 3.0 free license
 * Purpose: 
 **/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Z3N
{
    public class PlayerDraw : MonoBehaviour
    {
        #region Structures
        public struct SLinePoint
        {
            Vector2 pos;
            bool isShapeEnd;
            public SLinePoint(Vector2 a_pos, bool a_isShapeEnd)
            {
                pos = a_pos;
                isShapeEnd = a_isShapeEnd;
            }

            public override string ToString()
            {
                return "Point: " + pos.ToString() + ", End shape: " + isShapeEnd;
            }
        }
        #endregion

        #region Variables
        /// <summary>
        /// How often to sample the player's drag input.
        /// </summary>
        public float inputSampleTime = 0.01f;

        /// <summary>
        /// Points along the line in screen pixel coordinates.
        /// </summary>
        protected List<SLinePoint> _linePoints;

        /// <summary>
        /// Last time the line was recorded.
        /// </summary>
        protected float _lastLineRecordTime = 0.0f;
        #endregion

        #region Unity code
        /// <summary>
        /// Called when the entity is first ready.
        /// </summary>
        void Awake()
        {
            _linePoints = new List<SLinePoint>();
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
                isTouchDown = firstTouch.phase == TouchPhase.Began || firstTouch.phase == TouchPhase.Ended;
                isTouchUp = firstTouch.phase == TouchPhase.Ended || firstTouch.phase == TouchPhase.Canceled;
                touchPos = firstTouch.position;
            }

            if (isTouchUp)
            {
                // End the current shape
                EndShape(touchPos);
            }
            else if (isTouchDown)
            {
                // Continue the current shape
                if (Time.time - _lastLineRecordTime > inputSampleTime)
                {
                    AddLinePoint(touchPos);
                    _lastLineRecordTime = Time.time;
                }
            }
        }
        #endregion

        #region Point drawing
        protected void AddLinePoint(Vector2 a_point, bool a_isShapeEnd = false)
        {
            SLinePoint newPt = new SLinePoint(a_point, a_isShapeEnd);
            _linePoints.Add(newPt);

            Debug.Log("Added point: " + newPt);
        }

        protected void EndShape(Vector2 a_point)
        {
            // Add that final point
            AddLinePoint(a_point, true);

            // TODO: If out of ink, time to compare score or show to student
        }
        #endregion
    }
}