using System.Collections.Generic;
using Concentus.Enums;
using Concentus.Structs;
using UnityEngine;

//TODO https://github.com/ludos1978/OpusDotNet/blob/master/Assets/OpusNetworked.cs
public class OpusVoipSender : MonoBehaviour
{
    public AudioSource MicrophoneAudio;

    public OpusVoipReciever TestReciever;

    public static readonly int OpusChannels = 1;
    public static readonly int SampleRateHz = 12000;
    public static readonly int FrameSize = 120;

    private OpusEncoder encoder;
    private List<float> microphoneDataBuffer;

    private readonly int packageSize = FrameSize * OpusChannels;

    // Start is called before the first frame update
    void Start()
    {
        microphoneDataBuffer = new List<float>();
        encoder = new OpusEncoder(SampleRateHz, OpusChannels, OpusApplication.OPUS_APPLICATION_VOIP);
    }

    void Update()
    {
        //Recording
        if (Input.GetKey(KeyCode.R))
        {
            MicrophoneAudio.loop = true;
            MicrophoneAudio.clip = Microphone.Start(
                null,
                true,
                1,
                AudioSettings.outputSampleRate);
            
            MicrophoneAudio.Play();
        }
        else
        {
            Microphone.End(null);
            MicrophoneAudio.Stop();
        }

        //Sending - Forcing every packet to be the same size for now.
        while (microphoneDataBuffer.Count > packageSize)
        {
            // Debug.Log("running?" + microphoneDataBuffer.Count + " " + packageSize);
            byte[] outputBuffer = new byte[1275]; //Does this get cleared by the encoder?
            encoder.Bitrate = 12000;
            var outputLengthBytes = encoder.Encode(microphoneDataBuffer.GetRange(0, packageSize).ToArray(), 0, FrameSize, outputBuffer, 0, outputBuffer.Length);
            //Send output over wire via event here
            if (TestReciever)
            {
                TestReciever.OnGettingBytesFromSpatialOS((byte[]) outputBuffer.Clone());
            }
            microphoneDataBuffer.RemoveRange(0, packageSize);
        }
    }

    //Called by Unity
    void OnAudioFilterRead(float[] data, int channels)
    {
        microphoneDataBuffer.AddRange(data);
        //Clear the array so no sound gets played.
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }
}
