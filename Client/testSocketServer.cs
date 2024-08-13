
//TCP Client Script

//This script implements a simple multithreaded TCP server.

//If you use this code, please cite:

//Author: Mahya Mirbagheri
//GitHub Repository: https://github.com/Marry-20/UWP-2wayTCP



using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

//Able to act as a receiver 
public class testSocketServer : MonoBehaviour
{
    public string HostName;
    public string HostPort;
    public string Received = "None";

    public static testSocketServer instance = null;
    private BasicSocketClient _client = null;

    private void Client_DataReceived(object sender, NetworkDataReceivedEventArgs e)
    {
        Debug.Log(e.Message);
        Received = e.Message;
    }

    private void Client_ClientDisconnected(object sender, EventArgs e)
    {
        Debug.Log("Client Disconnected");
    }

    private void Client_ClientConnected(object sender, EventArgs e)
    {
        Debug.Log("Client Connected...");
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        _client = new BasicSocketClient(this.HostName, this.HostPort);
        _client.ClientConnected += Client_ClientConnected;
        _client.ClientDisconnected += Client_ClientDisconnected;
        _client.DataReceived += Client_DataReceived;
    }

    // Use this for initialization
    void Start()
    {
        _client.Connect();
    }

    private void OnDestroy()
    {
        _client.CloseConnection();
        _client.ClientConnected -= Client_ClientConnected;
        _client.ClientDisconnected -= Client_ClientDisconnected;
        _client.DataReceived -= Client_DataReceived;
    }

    public void Send(string SendData)
    {
        _client.SendData(SendData);
    }
}

