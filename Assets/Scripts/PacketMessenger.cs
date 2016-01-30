using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace Z3N
{
    ///<summary> A simple struct for containing the data for points in the line</summary>
    public struct LinePoint
    {
        public Vector2 position;
        public bool lastPoint;
    }
    ///<summary> A singlton interface to add points of the line to be sent to the other player </summary>
    public class Messenger : MonoBehaviour
    {
        #region Variables
        static public Stack<List<LinePoint>> s_drawing;
        static public Stack<List<LinePoint>> s_recivedPacket;
        static private List<LinePoint> s_newestLine;
        static private Messenger s_ref;
        static public short s_drawingCount = 0;
        public const int portNum = 3056;
        #endregion 
        #region Functions
        void Start()
        {
            s_drawing = new Stack<List<LinePoint>>();
            s_recivedPacket = new Stack<List<LinePoint>>();
            s_newestLine = new List<LinePoint>();
        }
        //Pushes the LinePoint infor back into the stack to be sent
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
        //Deletes the current stored data
        static public void ClearMessenger()
        {
            while(s_drawing.Count > 0)
            {
                s_drawing.Pop();
            }
            s_drawing = new Stack<List<LinePoint>>();
            s_drawingCount++;
        }
        #endregion
        #region Methods
        static public Messenger Reference
        {
            get {return s_ref;} 
        }
        #endregion
    }
    ///<summary> A basic interface for the network</summary>
    public class PacketMessenger : NetworkBehaviour
    {
        #region Unity Code
        //Use this for initialization
        [SerializeField]
        private TextMesh _IP;
        [SerializeField]
        UnityEngine.UI.InputField _text;
        NetworkConnection secondPlayerConnection;
        private bool _toSend;

        void Start()
        {
            Network.InitializeServer(2, Messenger.portNum, false);
            secondPlayerConnection = new NetworkConnection();
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
                Network.Connect(_text.text, Messenger.portNum + 5);
                NetworkPlayer currConnected = Network.connections[0];
                //Check network connection ID and network host ID!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                secondPlayerConnection.Initialize(currConnected.ipAddress, 0, 1, NetworkServer.hostTopology);
            }
            Debug.Log("Current number of connections " + Network.connections.Length.ToString());
           if(_toSend)
           {/*
                LinePoint[][] BADDAINELBAD;
                int I = 0;
                while(I < Messenger.s_drawing.Count)
                {
                    List<LinePoint> listCarry = Messenger.s_drawing.Pop();
                    BADDAINELBAD = new LinePoint[];
                    for (int J = 0; J < listCarry.Count; ++I)
                    {
                        BADDAINELBAD[I][J] = listCarry[J];
                    }
                    ++I;
                } */
                RpcSend(Messenger.s_drawing, Messenger.s_drawingCount);
                _toSend = false;
           }
           
    	}
        #endregion
        public void Send()
        {
            _toSend = true;
        }
        //Sends a packt to the other player
        [ClientRpc]
        private void RpcSend(Stack<List<LinePoint>> a_data, int _id)
        {
            Messenger.s_recivedPacket = a_data;
        }
        //Clears the current static stack
        public void Clear()
        {
            Messenger.ClearMessenger();
        }
        //Pushes linepoint info back to later be sent
        public void Push(LinePoint a_linePoint)
        {
            Messenger.PushLinePoint(a_linePoint);
        }
        /// <summary>
        /// The other players drawing
        /// </summary>
        /// <returns></returns>
        public Stack<List<LinePoint>> OtherPlayersDrawing()
        {
            return Messenger.s_recivedPacket;
        }
    }
}