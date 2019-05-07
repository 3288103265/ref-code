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
        private bool isImgOk;
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
            isImgOk = false;
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
                 this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }


        //事件处理器的定义
        private async void ServerDatagramSocket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
           
            try
            {
                while(true)
                {
                    DataReader dataReader = args.GetDataReader();
                    recvBytes = new byte[dataReader.UnconsumedBufferLength];
                    dataReader.ReadBytes(recvBytes);
                    isImgOk = true;
                }
                
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail. 
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                // dump data
                Debug.Log("Read Stream failed: " + exception.Message);
            }
           
            // TODO:加关停socket的命令和逻辑。    
        }
        #endif
    }
}
