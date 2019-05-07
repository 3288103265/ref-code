using System;
using UnityEngine;
#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace _Decal
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour
    {

        public Material material;
        private Texture2D tex;
        public Sprite sprite;

        private byte[] recvBytes;

        public float maxAngle = 90.0f;
        public float pushDistance = 0.009f;
        public LayerMask affectedLayers = -1;

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
            InitDecal();
#if WINDOWS_UWP
            InitNet();
#endif
        }

        private void InitDecal()
        {
            material = Resources.Load<Material>("Decal_1");
            tex = Resources.Load<Texture2D>("cartoon");
            material.SetTexture("_MainTex", tex);
            sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            Debug.Log("Initlized decal.");
        }
        private void Update()
        {
#if WINDOWS_UWP
            ChangeImg();
#endif
        }

#if WINDOWS_UWP
        private Socket server;
        private IPEndPoint thisEP;
        private IPEndPoint remoteEP;
        private EndPoint remoteHost;

        private void InitNet()
        {
        //初始化socket连接。
            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            thisEP = new IPEndPoint(IPAddress.Parse("192.168.1.146"), 8080);
            remoteEP = new IPEndPoint(IPAddress.Any, 0);
            remoteHost = remoteEP;
            server.Bind(thisEP);
        }

        private void ChangeImg()
        {
        //利用update函数每帧更新一次的特性，每次接受一帧图片然后改变贴画材质，实现动画效果。
            try
            {
                server.ReceiveFrom(recvBytes, ref remoteHost);
                tex.LoadImage(recvBytes);
                if (tex != null)
                {
                    material.SetTexture("_MainTex", tex);
                    sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
                }

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
#endif
    }
}
