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
using Kinect = Windows.Kinect;

// Receives the body data messages
public class BodyDataReceiver : Singleton<BodyDataReceiver>
{

    private Dictionary<ulong, Vector3[]> _BodiesPos = new Dictionary<ulong, Vector3[]>();
    private Dictionary<ulong, Quaternion[]> _BodiesRot = new Dictionary<ulong, Quaternion[]>();

    public Dictionary<ulong, Vector3[]> GetPosData()
    {
        return _BodiesPos;
    }

    public Dictionary<ulong, Quaternion[]> GetRotData()
    {
        return _BodiesRot;
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
        Debug.Log("Getting messages");
        ulong trackingID = (ulong)msg.ReadInt64();

        //GameObject joint = new GameObject();
        Vector3[] jointPos = new Vector3[25];
        Quaternion[] jointRot = new Quaternion[25];

        for (int i = 0; i < 25; i++)
        {
            jointPos[i] = CustomMessages2.Instance.ReadVector3(msg);
            jointRot[i] = CustomMessages2.Instance.ReadQuaternion(msg);
        }

        _BodiesPos[trackingID] = jointPos;
        _BodiesRot[trackingID] = jointRot;
    }
}