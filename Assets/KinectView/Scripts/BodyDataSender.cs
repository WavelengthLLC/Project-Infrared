/*
 * BodyDataSender.cs
 *
 * Broadcasts body data over the network
 * Requires CustomMessages2.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class BodyDataSender : Singleton<BodyDataSender> {

    public GameObject AvatarSourceView;

    private AvatarSourceView _AvatarSourceView;

    void Update() {


        if (AvatarSourceView == null) {
            Debug.Log("BodyDataConverter is null");
            return;
        }


        _AvatarSourceView = AvatarSourceView.GetComponent<AvatarSourceView>();
        if (_AvatarSourceView == null) {
            Debug.Log("AvatarSourceView component missing");
            return;
        }

        Dictionary<ulong, GameObject> bodyData = _AvatarSourceView.GetData();
        if (bodyData == null) {
            Debug.Log("bodyData is null");
            return;
        }

        //Debug.Log(bodyData);
        // Send over the bodyData one tracked body at a time
        List<ulong> trackingIDs = new List<ulong>(bodyData.Keys);
        //Debug.Log(trackingIDs.Count);
        foreach (ulong trackingID in trackingIDs) {
            CustomMessages2.Instance.SendBodyData(trackingID, bodyData[trackingID]);
        }
    }
}