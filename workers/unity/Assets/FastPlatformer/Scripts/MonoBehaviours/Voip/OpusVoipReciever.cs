using System.Collections.Generic;
using Concentus.Structs;
using UnityEngine;

//TODO https://github.com/ludos1978/OpusDotNet/blob/master/Assets/OpusNetworked.cs
public class OpusVoipReciever : MonoBehaviour
{
    public AudioSource AudioPlayback;

    private int opusChannels;
    private int frameSize;
    private int sampleRateHz;

    private OpusDecoder decoder;

    private List<float> receiveBuffer;

    // Start is called before the first frame update
    void Start()
    {
        sampleRateHz = OpusVoipSender.SampleRateHz;
        frameSize = OpusVoipSender.FrameSize;
        opusChannels = OpusVoipSender.OpusChannels;
        receiveBuffer = new List<float>();
        decoder = new OpusDecoder(sampleRateHz, opusChannels);

        // setup a playback audio clip, length is set to 1 sec (should not be used anyways)
        var myClip = AudioClip.Create("VoipPlayback", sampleRateHz, opusChannels, sampleRateHz, true, OnAudioRead, OnAudioSetPosition);
        AudioPlayback.loop = true;
        AudioPlayback.clip = myClip;
        AudioPlayback.Play();
    }

    private void OnAudioRead(float[] data)
    {
        var pullSize = Mathf.Min(data.Length, receiveBuffer.Count);
        var dataBuf = receiveBuffer.GetRange(0, pullSize).ToArray();
        dataBuf.CopyTo(data, 0);
        receiveBuffer.RemoveRange(0, pullSize);

        // clear rest of data
        for (var i = pullSize; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }

    private void OnAudioSetPosition(int position)
    {
        //Not used.
    }

    public void OnGettingBytesFromSpatialOS(byte[] data)
    {
        Debug.Log("Got " + data.Length + "bytes");
        float[] outputBuffer = new float[sampleRateHz * opusChannels]; //Sample rate times num channels
        int thisFrameSize = decoder.Decode(data, 0, data.Length, outputBuffer, 0, frameSize);
        receiveBuffer.AddRange(outputBuffer);
    }
}
