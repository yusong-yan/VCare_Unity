using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.RenderStreaming;
using System.Threading;
using Unity.RenderStreaming.Signaling;
using System;

public class SendMainCamReceiveVideo : MonoBehaviour
{
#pragma warning disable 0649
        [SerializeField] private RenderStreaming renderStreaming;
        [SerializeField] private SingleConnection singleConnection;
        [SerializeField] private RawImage remoteVideoImage;
        [SerializeField] private InputField connectionIdInput;
        [SerializeField] private Button setUpButton;
        [SerializeField] private Button hangUpButton;
        [SerializeField] private CameraStreamSender CamStreamer;
        [SerializeField] private VideoStreamReceiver receiveVideoViewer;
        [SerializeField] private AudioStreamReceiver receiveAudioViewer;
        [SerializeField] private AudioSource receiveAudioSource;
        [SerializeField] private MicrophoneStreamSender microphoneStreamer;
        [SerializeField] Dropdown dropdownCamera;
        [SerializeField] Camera[] cameras;
#pragma warning restore 0649

        private string connectionId;
    // Start is called before the first frame update

    void Awake()
    {
        microphoneStreamer.SetDeviceIndex(0);
        microphoneStreamer.enabled = true;
        CamStreamer.enabled = true;
        setUpButton.interactable = true;
        hangUpButton.interactable = false;
        receiveVideoViewer.enabled = true;
        connectionIdInput.interactable = true;
        setUpButton.onClick.AddListener(SetUp);
        hangUpButton.onClick.AddListener(HangUp);
        connectionIdInput.onValueChanged.AddListener(input => connectionId = input);
        connectionIdInput.text = $"{UnityEngine.Random.Range(0, 99999):D5}";
        microphoneStreamer.OnStartedStream += id => receiveAudioViewer.enabled = true;
        receiveVideoViewer.OnUpdateReceiveTexture += texture => remoteVideoImage.texture = texture;
        receiveAudioViewer.OnUpdateReceiveAudioClip += clip =>
        {
            receiveAudioSource.clip = clip;
            receiveAudioSource.loop = true;
            receiveAudioSource.Play();
        };
    }

    void Start()
    {
        OnChangeCamera(0);
        dropdownCamera.onValueChanged.AddListener(OnChangeCamera);
        if (renderStreaming.runOnAwake)
            return;
        renderStreaming.Run(
            hardwareEncoder: RenderStreamingSettings.EnableHWCodec,
            signaling: RenderStreamingSettings.Signaling);
    }
    private void SetUp()
    {
        setUpButton.interactable = false;
        hangUpButton.interactable = true;
        connectionIdInput.interactable = false;
        singleConnection.CreateConnection(connectionId);
        
    }
    private void HangUp()
    {
        singleConnection.DeleteConnection(connectionId);
        remoteVideoImage.texture = null;
        setUpButton.interactable = true;
        hangUpButton.interactable = false;
        connectionIdInput.interactable = true;
        connectionIdInput.text = $"{UnityEngine.Random.Range(0, 99999):D5}";
    }
    
    void OnChangeCamera(int value)
    {
        //Debug.Log(cameras.Length);
        SetOrigin(value);
    }

    
    public void SetOrigin(int value)
    {
        // //Debug.Log("active:"+value);
        // cameras[value].enabled = true;
        // for(int i = 0; i < cameras.Length; i++){
        //     if (i != value){
        //         //Debug.Log("deactive:"+i);
        //         cameras[i].enabled = false;
        //     }
        // }
    }
    private void Update() {
        
    }
}

internal enum SignalingType
{
    WebSocket,
    Http,
    Furioos
}
internal static class RenderStreamingSettings
{
    private static bool s_enableHWCodec = false;
    private static SignalingType s_signalingType = SignalingType.WebSocket;
    private static string s_signalingAddress = "localhost";
    private static float s_signalingInterval = 5;
    private static bool s_signalingSecured = false;
    public static bool EnableHWCodec
    {
        get { return s_enableHWCodec; }
        set { s_enableHWCodec = value; }
    }
    public static SignalingType SignalingType
    {
        get { return s_signalingType; }
        set { s_signalingType = value; }
    }
    public static string SignalingAddress
    {
        get { return s_signalingAddress; }
        set { s_signalingAddress = value; }
    }
    public static bool SignalingSecured
    {
        get { return s_signalingSecured; }
        set { s_signalingSecured = value; }
    }
    public static float SignalingInterval
    {
        get { return s_signalingInterval; }
        set { s_signalingInterval = value; }
    }
    public static Unity.RenderStreaming.Signaling.ISignaling Signaling
    {
        get
        {
            switch (s_signalingType)
            {
                case SignalingType.Furioos:
                {
                    var schema = s_signalingSecured ? "https" : "http";
                    return new FurioosSignaling(
                        $"{schema}://{s_signalingAddress}", s_signalingInterval, SynchronizationContext.Current);
                }
                case SignalingType.WebSocket:
                {
                    var schema = s_signalingSecured ? "wss" : "ws";
                    return new WebSocketSignaling(
                        $"{schema}://{s_signalingAddress}", s_signalingInterval, SynchronizationContext.Current);
                }
                case SignalingType.Http:
                {
                    var schema = s_signalingSecured ? "https" : "http";
                    return new HttpSignaling(
                        $"{schema}://{s_signalingAddress}", s_signalingInterval, SynchronizationContext.Current);
                }
            }
            throw new InvalidOperationException();
        }
    }
}