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

        private async void StartServer()
        {
            try
            {
                //instantitate a listener
                var streamSocketListener = new StreamSocketListener();

                //挂接事件处理器。
                //每次连接后，就会调用事件处理器，接受请求，回复请求。不知道是不是每次发送都需要来回收发，最好的状态是建立一次连接，然后随便发送最好。
                // The ConnectionReceived event is raised when connections are received.
                streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

                //开始监听。
                // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
                await streamSocketListener.BindServiceNameAsync("8080");
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
        }
        //事件处理器。
        private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            try
            {
                while(true)
                {
                    //读长度
                    await reader.LoadAsync(sizeof(uint));
                    uint len = reader.ReadUInt32();
                    // 读内容
                    await reader.LoadAsync(len);
                    recvBytes = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(recvBytes);
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

            }
   
        }
        #endif
    }
}
