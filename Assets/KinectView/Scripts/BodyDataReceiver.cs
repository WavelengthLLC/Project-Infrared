/*
 * BodyDataReceiver.cs
 *
 * Receives body data from the network
 * Requires CustomMessages2.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

// Receives the body data messages
public class BodyDataReceiver : Singleton<BodyDataReceiver>
{

    private Dictionary<ulong, Transform[]> _Bodies = new Dictionary<ulong, Transform[]>();

    public Dictionary<ulong, Transform[]> GetData()
    {
        return _Bodies;
    }

    void Start()
    {
        CustomMessages2.Instance.MessageHandlers[CustomMessages2.TestMessageID.BodyData] =
            this.UpdateBodyData;
    }

    // Called when reading in Kinect body data
    void UpdateBodyData(NetworkInMessage msg)
    {
        // Parse the message
        Debug.Log("Got vectors message");
        ulong trackingID = (ulong)msg.ReadInt64();
        Transform jointTrans = new GameObject().transform;
        Transform[] jointTransforms = new Transform[25];

        for (int i = 0; i < 25; i++)
        {
            jointTrans.position = CustomMessages2.Instance.ReadVector3(msg);
            jointTrans.rotation = CustomMessages2.Instance.ReadQuaternion(msg);
            jointTransforms[i] = jointTrans;
        }

        _Bodies[trackingID] = jointTransforms;
    }
}