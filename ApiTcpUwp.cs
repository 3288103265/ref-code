// Every protocol typically has a standard port number. For example, HTTP is typically 80, FTP is 20 and 21, etc.
// For this example, we'll choose an arbitrary port number.
static string PortNumber = "1337";

protected override void OnNavigatedTo(NavigationEventArgs e)
{
    this.StartServer();
    this.StartClient();
}
//Server 
private async void StartServer()
{
    try
    {
        //instantitate a listener
        var streamSocketListener = new Windows.Networking.Sockets.StreamSocketListener();

        //挂接事件处理器。
        //每次连接后，就会调用事件处理器，接受请求，回复请求。不知道是不是每次发送都需要来回收发，最好的状态是建立一次连接，然后随便发送最好。
        // The ConnectionReceived event is raised when connections are received.
        streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

        //开始监听。
        // Start listening for incoming TCP connections on the specified port. You can specify any port that's not currently in use.
        await streamSocketListener.BindServiceNameAsync(StreamSocketAndListenerPage.PortNumber);

        this.serverListBox.Items.Add("server is listening...");
    }
    catch (Exception ex)
    {
        Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
        this.serverListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
    }
}
//事件处理器。
private async void StreamSocketListener_ConnectionReceived(Windows.Networking.Sockets.StreamSocketListener sender, Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
{
    string request;
    //接受请求，并用变量接受发来的字符串信息
    using (var streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
    {
        request = await streamReader.ReadLineAsync();
    }
//不知道干嘛的
    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));

    // Echo the request back as the response.
    using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite())
    {
        using (var streamWriter = new StreamWriter(outputStream))
        {
            //将发来的字符串信息原样回复
            await streamWriter.WriteLineAsync(request);
            //不知道
            await streamWriter.FlushAsync();
        }
    }
//不知道
    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server sent back the response: \"{0}\"", request)));
//这个有点像关闭tcp发送，但是又好像不是
    sender.Dispose();
//又出现了，不知道
    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("server closed its socket"));
}
//客户端程序
private async void StartClient()
{
    try
    {
        // Create the StreamSocket and establish a connection to the echo server.
        using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
        {
            // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
            //这是远程主机的名字，第二个参数是连接远程主机的端口号。
            //不过只要远程主机的名字和端口号有什么用？？不应该使用ip吗？
            //查看了官方文档后发现，第一个参数可以是ip
            // public IAsyncAction ConnectAsync(HostName remoteHostName, String remoteServiceName)
            var hostName = new Windows.Networking.HostName("localhost");

            this.clientListBox.Items.Add("client is trying to connect...");

            //连接服务器，
            await streamSocket.ConnectAsync(hostName, StreamSocketAndListenerPage.PortNumber);

            this.clientListBox.Items.Add("client connected");

            //这里是用来发送信息的
            // Send a request to the echo server.
            string request = "Hello, World!";
            using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
            {
                using (var streamWriter = new StreamWriter(outputStream))
                {
                    await streamWriter.WriteLineAsync(request);
                    await streamWriter.FlushAsync();
                }
            }

            this.clientListBox.Items.Add(string.Format("client sent the request: \"{0}\"", request));

            //这个是用来接受客户端的回复信息的
            //发送和接受同在一个方法体里面，说明是有先后顺序的。
            // Read data from the echo server.
            string response;
            using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
            {
                using (StreamReader streamReader = new StreamReader(inputStream))
                {
                    response = await streamReader.ReadLineAsync();
                }
            }

            this.clientListBox.Items.Add(string.Format("client received the response: \"{0}\" ", response));
        }

        this.clientListBox.Items.Add("client closed its socket");
    }
    catch (Exception ex)
    {
        Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
        this.clientListBox.Items.Add(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
    }
}