using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace _Decal {

    [RequireComponent( typeof( MeshFilter ) )]//The RequireComponent attribute automatically adds required components as dependencies.
    [RequireComponent( typeof( MeshRenderer ) )]
    [ExecuteInEditMode]
    public class Decal : MonoBehaviour {
        private Vector3 oldScale;
        public Material material;
        public Sprite sprite;
        public Texture2D tex;
        byte[] byArray;
        string flag = "aaaa";
        bool flag1 = false;
        
        Renderer rend;
        uint aaa;
    #if WINDOWS_UWP
    StreamSocket socket;
    StreamSocketListener listener;
    String port;
    private HostName host;
    #endif


        public float maxAngle = 90.0f;
        public float pushDistance = 0.009f;
        public LayerMask affectedLayers = -1;//LayerMask创建一个下拉菜单

        public Texture texture {
            get {
                return material ? material.mainTexture : null;//若material不为空，则返回material的texture
            }
        }

#if WINDOWS_UWP
        private async void Listener_Start()
        {
            Debug.Log("Listener started");
            try
            {

                NetworkAdapter adapter = host.IPInformation.NetworkAdapter;

                await listener.BindServiceNameAsync(
                    port,
                SocketProtectionLevel.PlainSocket,
                    adapter);
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }

            Debug.Log("Listening");
        }

        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Log("Connection received");
            DataReader reader = new DataReader(args.Socket.InputStream);
           

            try
            {
                while (true)
                {
                    System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                    stopwatch1.Start();
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }
                    // Read the string.
                    uint stringLength = reader.ReadUInt32();
                    Debug.Log(stringLength);
                    //uint actualStringLength = await reader.LoadAsync(stringLength);
                    //Debug.Log(actualStringLength);
                    //if (stringLength != actualStringLength)
                    //{
                    //    // The underlying socket was closed before we were able to read the whole data.
                    //    return;
                    //}
                    ////flag = reader.ReadString(actualStringLength);

                    //Debug.Log(sizeFieldCount);
                    Debug.Log("byArray GET0");
                    uint imageCount = await reader.LoadAsync(stringLength);
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    Debug.Log("byArray GET1");
                    Debug.Log("byArray GET2");
                    uint aaa = reader.UnconsumedBufferLength;
                    Debug.Log(aaa);
                    //uint numFileBytes = await reader.LoadAsync(reader.UnconsumedBufferLength);
                    Debug.Log("byArray GET2");
                    //Debug.Log(numFileBytes);
                    byArray = new byte[aaa];


                    reader.ReadBytes(byArray);
                    Debug.Log(byArray);
                    Debug.Log("byArray GET");

                    stopwatch1.Stop(); //  停止监视
                    System.TimeSpan timespan = stopwatch1.Elapsed;
                    double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数
                    Debug.Log("time111");
                    Debug.Log(milliseconds);

                    flag1 = true;


                    //// Read the string.
                    //uint stringLength = reader.ReadUInt32();
                    //Debug.Log(stringLength);
                    //uint actualStringLength = await reader.LoadAsync(stringLength);
                    //Debug.Log(actualStringLength);
                    //if (stringLength != actualStringLength)
                    //{
                    //    // The underlying socket was closed before we were able to read the whole data.
                    //    return;
                    //}

                    //flag = reader.ReadString(actualStringLength);
                    //Debug.Log(flag);
                    //flag1 = true;
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
        }
        private bool GetIpAddress()
        {
            foreach (HostName localHost in NetworkInformation.GetHostNames())
            {
                if (null == localHost)
                {
                    return false;
                }
                IPInformation ipInfo = localHost.IPInformation;
                if (localHost.Type == HostNameType.Ipv4 && ipInfo.NetworkAdapter.IanaInterfaceType == 71)  //自动获取IP地址。6为y有线，71为无线
                {
                    host = localHost;
                    Debug.Log(localHost);
                    return true;
                }
            }
            return false;
        }
#endif

        void Start() {
        
#if WINDOWS_UWP
            if (GetIpAddress())
            {

                Debug.Log("11");
                listener = new StreamSocketListener();
                Debug.Log("111");
                port = "2000";
                listener.ConnectionReceived += Listener_ConnectionReceived;
                Debug.Log("1111");
                listener.Control.KeepAlive = false;

                Listener_Start();
            }
#endif


        }


        void Update() {


            //if (transform.parent && transform.parent.hasChanged)
            // when inspector is not allowed //当位置发生改变时候
            //transform.parent.hasChanged = false;

            if (flag1)
            {
                material = Resources.Load("Decals_1") as Material;
                
                tex.LoadImage(byArray);


                if (tex != null)
                {
                    material.SetTexture("_MainTex", tex);
                    sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
                }
                //Vector3 scale = this.transform.localScale;

                //if (sprite != null)
                //{
                //    float ratio = sprite.rect.width / sprite.rect.height;

                //    if (!Mathf.Approximately(oldScale.x, scale.x))
                //    {
                //        scale.y = scale.x / ratio;
                //    }
                //    if (!Mathf.Approximately(oldScale.y, scale.y))
                //    {
                //        scale.x = scale.y * ratio;
                //    }

                //    if (!Mathf.Approximately(scale.x / scale.y, ratio))
                //    {
                //        scale.x = scale.y * ratio;
                //    }
                //}
                //this.transform.localScale = scale;
                //Debug.Log("psh");
                //Debug.Log(scale);
                //oldScale = scale;
                Resources.UnloadUnusedAssets();
                System.GC.Collect();

                flag1 = false;

                Debug.Log("time11");
         
               
            }
        }

        void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube( Vector3.zero, Vector3.one );
        }

    }



}
