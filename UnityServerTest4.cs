using System;
using UnityEngine;

//#if WINDOWS_UWP
//using System.Net;
//using System.Net.Sockets;
//#endif

namespace _Decal
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour
    {
        public Material material;
        public Sprite sprite;
        private Texture2D tex;

        public float maxAngle = 90.0f;
        public float pushDistance = 0.009f;
        public LayerMask affectedLayers = -1;

        private byte[] recvBytes;
        //Determine when to stop the communication and quit.

        public Texture texture
        {
            get
            {
                return material ? material.mainTexture : null;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void Start()
        {
            recvBytes = new byte[65535];
            InitDecal();
//#if WINDOWS_UWP
//            InitNet();
//#endif
        }

        private void Update()
        {
//#if WINDOWS_UWP
//            LoadImg();
//#endif
        }

        void InitDecal()
        {
            material = Resources.Load<Material>("Decal_1");
            tex = Resources.Load<Texture2D>("cartoon");
            material.SetTexture("_MainTex", tex);
            sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            Debug.Log("Initlized decal.");
        }
//#if WINDOWS_UWP
//        private Socket server;
//        private IPEndPoint thisEP;
//        private IPEndPoint remoteEP;
//        private EndPoint remoteHost;

//        private void InitNet()
//        {
//            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
//            thisEP = new IPEndPoint(IPAddress.Parse("192.168.1.146"), 11000);
//            remoteEP = new IPEndPoint(IPAddress.Any, 0);
//            remoteHost = remoteEP;
//            server.Bind(thisEP);
//        }

//        private void LoadImg()
//        {
//            try
//            {
//                server.ReceiveFrom(recvBytes, ref remoteHost);
//                tex.LoadImage(recvBytes);
//                if (tex != null)
//                {
//                    material.SetTexture("_MainTex", tex);
//                    sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
//                }
             
//            }
//            catch (Exception e)
//            {
//                Debug.Log(e.Message);
//            }
//        }
//#endif
    }
}
