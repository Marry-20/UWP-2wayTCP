using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#if !UNITY_EDITOR && UNITY_WSA
using Windows.Foundation;
using Windows.System.Threading;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

class BasicSocketClient
{
    private string _hostNameString = "";
    private string _hostPortString = "";

#if !UNITY_EDITOR && UNITY_WSA
        StreamSocket _clientSocket = null;
        DataReader _reader = null;
        DataWriter _writer = null;
#endif
    private bool _isConnected = false;

    public event EventHandler ClientConnected;
    public event EventHandler ClientDisconnected;
    public event EventHandler<NetworkDataReceivedEventArgs> DataReceived;

    public BasicSocketClient(String HostName, String PortNumber)
    {
        _hostNameString = HostName;
        _hostPortString = PortNumber;
    }

    private void DebugOutput(String message)
    {
        Debug.Write($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}\r\n");
    }

    private void OnClientConnected()
    {
        ClientConnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnClientDisconnected()
    {
        ClientDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private void OnDataReceived(NetworkDataReceivedEventArgs e)
    {
        EventHandler<NetworkDataReceivedEventArgs> handler = DataReceived;
        if (null != handler)
        {
            handler(this, e);
        }
    }

    public async void Connect()
    {
#if !UNITY_EDITOR && UNITY_WSA
        if (_isConnected)
        {
            throw new InvalidOperationException("Client Already Connected");
        }

        try
        {
            HostName hostName = new HostName(_hostNameString);
            _clientSocket = new StreamSocket();
                
            await _clientSocket.ConnectAsync(hostName, _hostPortString);
            _isConnected = true;

            OnClientConnected();

            _reader = new DataReader(_clientSocket.InputStream);
            _reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            _reader.InputStreamOptions = InputStreamOptions.Partial;

            _writer = new DataWriter(_clientSocket.OutputStream);

            _ = ThreadPool.RunAsync(ReceiveData);
        }
        catch (Exception ex)
        {
            if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
            {
                throw new SystemException("Unknown Socket Error", ex.InnerException);
            }

            DebugOutput($"Connect to server failed: {ex.Message}");
            _clientSocket.Dispose();
            _clientSocket = null;
        }
#endif
    }

    public void CloseConnection()
    {
#if !UNITY_EDITOR && UNITY_WSA
        _isConnected = false;
        _clientSocket.Dispose();
        _clientSocket = null;

        OnClientDisconnected();
#endif
    }

    public async void SendData(string strMessage)
    {
#if !UNITY_EDITOR && UNITY_WSA
        try
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Client Not Connected");
            }

            UInt32 bytesToWrite = _writer.MeasureString(strMessage);
            _writer.WriteString(strMessage);
            _ = await _writer.StoreAsync();
        }
        catch (Exception ex)
        {
            if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
            {
                throw new SystemException("Socket Error: Send Data Failed", ex.InnerException);
            }

            DebugOutput($"Socket Error: Send Data Failed: {ex.Message}");
            CloseConnection();
        }
#endif
    }

#if !UNITY_EDITOR && UNITY_WSA
    private async void ReceiveData(IAsyncAction operation)
    {
        try
        {
            while (_isConnected)
            {
                uint bytesToRead = await _reader.LoadAsync(2048);
                String data = _reader.ReadString(_reader.UnconsumedBufferLength);

                NetworkDataReceivedEventArgs args = new NetworkDataReceivedEventArgs()
                {
                    Message = data,
                    TimeReceived = DateTime.Now
                };

                OnDataReceived(args);
            }
        }
        catch (Exception ex)
        {
            if (SocketError.GetStatus(ex.HResult) == SocketErrorStatus.Unknown)
            {
                throw new SystemException("Socket Error: Receive Data Failed", ex.InnerException);
            }

            if (_isConnected)
            {
                DebugOutput($"Receive data failed: {ex.Message}");
                CloseConnection();
            }
        }
    }
#endif
}

