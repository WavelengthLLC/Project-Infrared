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

    private Dictionary<ulong, GameObject[]> _Bodies = new Dictionary<ulong, GameObject[]>();

    public Dictionary<ulong, GameObject[]> GetData()
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
        GameObject joint = new GameObject();
        GameObject[] jointTransforms = new GameObject[25];

        for (int i = 0; i < 25; i++)
        {
            joint.transform.position = CustomMessages2.Instance.ReadVector3(msg);
            joint.transform.rotation = CustomMessages2.Instance.ReadQuaternion(msg);
            jointTransforms[i] = joint;
        }

        _Bodies[trackingID] = jointTransforms;
    }
}