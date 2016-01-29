using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Z3N
{ 
    public struct LinePoint
    {
        public Vector2 position;
        public bool lastPoint;
    }
    public class Messenger : MonoBehaviour
    {
        #region Variables
        static public Stack<List<LinePoint>> s_drawing;
        static private List<LinePoint> s_newestLine;
        static private Messenger s_ref;
        #endregion 
        #region Functions
        void Start()
        {
            s_drawing = new Stack<List<LinePoint>>();
            s_newestLine = new List<LinePoint>();
        }
        static public void PushLinePoint(LinePoint a_linePoint)
        {
            if(a_linePoint.lastPoint)
            {
                s_newestLine.Add(a_linePoint);
                s_drawing.Push(s_newestLine);
                s_newestLine = new List<LinePoint>();
            }
            else
            {
                s_newestLine.Add(a_linePoint);
            }
        }
        static public void ClearMessenger()
        {
            while(s_drawing.Count > 0)
            {
                s_drawing.Pop();
            }
            s_drawing = new Stack<List<LinePoint>>();
        }
        #endregion
        #region Methods
        static public Messenger Reference
        {
            get {return s_ref;} 
        }
        #endregion
    }
    public class PacketMessenger : MonoBehaviour
    {
        #region Unity Code
        //Use this for initialization
        [SerializeField]
        private TextMesh _IP;
        [SerializeField]
        UnityEngine.UI.InputField _text;
        private NetworkClient _thisClient;
        private NetworkClient _otherClient;

        void Start()
        {
            Network.InitializeServer(2,3056,false);
    	}
    	
    	//Update is called once per frame
    	void Update()
        {
    	    if(_IP.text == "Hello World")
            {
                _IP.text = Network.player.ipAddress;
            }
            if(Input.GetKey(KeyCode.KeypadEnter))
            {
                Network.Connect(_text.text,3056);
            }
            Debug.Log("Current number of connections " + Network.connections.Length.ToString());
    	}

        
        #endregion 
    }
}