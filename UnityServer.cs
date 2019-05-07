using UnityEngine;
using System.Collections.Generic;
using System;
#if WINDOWS_UWP
using Windows.Networking;
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
        public Sprite sprite;
        private Texture2D tex;
        private byte[] recvBytes;
        private bool isImgOk;

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
            recvBytes = new byte[1024*65];
            InitDecal();
            #if WINDOWS_UWP
            InitServer();
            #endif
        }

        private void Update()
        {
            if(isImgOk==true)
            {
                tex.LoadImage(recvBytes);
                if (tex != null)
                {
                    material.SetTexture("_MainTex", tex);
                    sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
                }
            }
            isImgOk = false;
        }

        private void InitDecal()
        {
            material = Resources.Load<Material>("Decal_1");
            tex = Resources.Load<Texture2D>("cartoon");
            material.SetTexture("_MainTex", tex);
            sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
            Debug.Log("Initlized decal.");
        }

        #if WINDOWS_UWP

        private DatagramSocket serverDatagramSocket;

        private async void InitServer()
        {
            try
            {   //实例化DatagramSocket
                serverDatagramSocket = new DatagramSocket();

                //挂接事件处理器
                // The ConnectionReceived event is raised when connections are received.
                serverDatagramSocket.MessageReceived += ServerDatagramSocket_MessageReceived;

                //绑定本机端口，这样可以接受来自任意远端的信息；也可指定单一远端通过connect操作。
                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await serverDatagramSocket.BindServiceNameAsync("8080");
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            }
        }


        //事件处理器的定义
        private async void ServerDatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            while(true)
            {
                using (DataReader dataReader = args.GetDataReader())
                {
                    //接受文件这里是关键，但是不能确定这样是否能行
                    //用变量接收发送来的数据
                    //string。trim（）删去前导和尾随空白符，但是并不会删去字符串中间的空白符。
                    dataReader.ReadBytes(recvBytes);
                } 
                //发送速度如果大于更新的帧率，则会丢帧，最终帧率将决定于两个里面较小的哪个
                isImgOk=false;       
            }     
            // TODO:加关停socket的命令和逻辑。    
        }
        #endif
    }
}
