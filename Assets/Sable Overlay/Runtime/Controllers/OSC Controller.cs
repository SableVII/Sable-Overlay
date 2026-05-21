using OscCore;
using System.Collections.Generic;
using UnityEngine;
using BlobHandles;
using Valve.VR;
using System;
using SharpOSC;

public class OSCController
{
    //private static readonly OSCController _instance = new OSCController();

    //static OSCController() { }

    //public static OSCController Instance
    //{
    //    get { return _instance; }
    //}

    static private string _ipAddress = "127.0.0.1";

    static private int _vrcListeningPort = 9001;

    static private int _programListeningPort = 9300;

    static private int _sendingPort = 9000;

    static private OscServer _server;

    static private OscClient _client;
    static public OscClient Client { get { return _client; } }

    static private int[] _reroutePorts =
    {
        9100,
        9200
    };

    // The Sender that sends onto this Overlay Program's Listening port
    static private UDPSender _programRerouteSender = null;
    static private List<UDPSender> _rerouteSenders = new List<UDPSender>();

    static private UDPListener _rerouteListener = null;

    public static void Initialize()
    {
        if (_client == null)
        {
            _client = new OscClient(_ipAddress, _sendingPort);
        }

        if (_server == null)
        {
            _server = new OscServer(_programListeningPort);
            _server.Start();
        }

        // Set-up Reroute Ports
        // Always have this Overlay Program Reroute Port set-up
        _programRerouteSender = new UDPSender(_ipAddress, _programListeningPort);

        _rerouteSenders.Clear();
        foreach (int reroutePort in _reroutePorts)
        {
            _rerouteSenders.Add(new UDPSender(_ipAddress, reroutePort));
        }

        // Callback for rerouting OSC messages
        HandleBytePacket callback = delegate (byte[] bytes)
        {
            // Always send OSC messages to this program's listening port
            _programRerouteSender.Send(bytes);

            // Send all that data to the other programs
            foreach (UDPSender rerouteSender in _rerouteSenders)
            {
                rerouteSender.Send(bytes);
            }
        };

        _rerouteListener = new UDPListener(_vrcListeningPort, callback);
    }

    public static bool TryAddListenBinding(string address, System.Action<OscMessageValues> binding)
    {
        if (_server == null)
        {
            return false;
        }

        return _server.TryAddMethod(address, binding);
    }

    public static void Shutdown()
    {
        if (_server == null)
        {
            return;
        }

        _server.Dispose();

        _rerouteListener.Dispose();

        foreach (UDPSender rerouteSender in _rerouteSenders)
        {
            rerouteSender.Close();
        }
    }
}