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
    //StreamSocket是TCP协议的通信类。
    //声明变量，客户端
    StreamSocket socket;
    StreamSocketListener listener;
    String port;
    private HostName host;
    #endif


        public float maxAngle = 90.0f;
        public float pushDistance = 0.009f;
        public LayerMask affectedLayers = -1;//LayerMask����һ�������˵�

        public Texture texture {
            get {
                return material ? material.mainTexture : null;
            }
        }

#if WINDOWS_UWP
        private async void Listener_Start()
        {
            Debug.Log("Listener started");
            try
            {
                //该类来自于connectivity命名空间
                //代表了一个网络适配器
                NetworkAdapter adapter = host.IPInformation.NetworkAdapter;
                //服务器监听，绑定
                //第一个参量是tcp远程端口（不清楚，可能也是本地的端口），第二个参数为协议的保密水平，第三个参数为网络适配器，用来绑定；
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
        //监听的时间处理器，按道理应该挂接到listener上
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Log("Connection received");
            //读取发送来的信息（请求）
            //DataReader属于window.Storage.Streams命名空间，用于读取数据流文件
            //https://docs.microsoft.com/en-us/uwp/api/windows.storage.streams.datareader.readuint32

            DataReader reader = new DataReader(args.Socket.InputStream);
           

            try
            {
                //这个事件是无限循环运行的，只要有一次成功的连接，就一直是接收的状态。
                while (true)
                {
                    //实例化了一个秒表
                    System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
                    //开始计时
                    stopwatch1.Start();
                    //LoadAsync的参数表示将要写入buffer的字节数。
                    //此处是为了测试socket网络处于打开状态
                    //LoadAsync表示先将输入流加载到一个buffer里面，等待后面在从buffer里面读取。
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        // The underlying socket was closed before we were able to read the whole data.
                        return;
                    }
                    // Read the string.
                    //表示读取一个32位的无符号整型数
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
                    //这一段看不懂啊？？
                    Debug.Log("byArray GET0");
                    uint imageCount = await reader.LoadAsync(stringLength);
                    //输入流的读取选项，partial貌似表示异步读取，一次读一个？
                    reader.InputStreamOptions = InputStreamOptions.Partial;
                    Debug.Log("byArray GET1");
                    Debug.Log("byArray GET2");
                    //获取buffer里面还没有被读取的数据大小
                    uint aaa = reader.UnconsumedBufferLength;
                    Debug.Log(aaa);
                    //uint numFileBytes = await reader.LoadAsync(reader.UnconsumedBufferLength);
                    Debug.Log("byArray GET2");
                    //Debug.Log(numFileBytes);
                    //指定接受字节数组的大小
                    byArray = new byte[aaa];

                    //将字节写到接收数组里面
                    reader.ReadBytes(byArray);
                    Debug.Log(byArray);
                    Debug.Log("byArray GET");

                    stopwatch1.Stop(); //关闭了秒表，对一次读取进行了计时，不知道每次会不会清零
                    //将运行用时赋值给了 timespan
                    System.TimeSpan timespan = stopwatch1.Elapsed;
                    //将运行时间换算成毫秒并现实出来
                    double milliseconds = timespan.TotalMilliseconds;
                    Debug.Log("time111");
                    Debug.Log(milliseconds);

                    //将flag1设置为了true，表示这是一次成功的接收
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
        //获取IP地址，返回布尔值
        private bool GetIpAddress()
        {
            //https://docs.microsoft.com/en-us/uwp/api/windows.networking.connectivity.networkinformation.gethostnames
            //获取与本机相连接的主机名称
            foreach (HostName localHost in NetworkInformation.GetHostNames())
            {
                if (null == localHost)
                {
                    return false;
                }
                IPInformation ipInfo = localHost.IPInformation;
                //https://docs.microsoft.com/en-us/uwp/api/windows.networking.connectivity.ipinformation
                //属于connectivity命名空间
                //Ianaxxx（）获取一个代表网络接口类型的值，71代表WiFi地址
                if (localHost.Type == HostNameType.Ipv4 && ipInfo.NetworkAdapter.IanaInterfaceType == 71) 
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
                //一旦初始化完成
                Debug.Log("11");
                listener = new StreamSocketListener();
                Debug.Log("111");
                port = "2000";
                //挂接事件处理器
                //只有接收这个动作触发事件处理器，我希望用update触发事件；
                listener.ConnectionReceived += Listener_ConnectionReceived;
                Debug.Log("1111");
                listener.Control.KeepAlive = false;
                //开始监听，将激活事件处理器。
                //事件处理器设置成无限循环是否有意义“？？
                Listener_Start();
            }
#endif


        }


        void Update() {

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
