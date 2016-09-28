/*
 * BodyDataConverter.cs
 *
 * Converts Kinect body data into a format that can easily be
 * sent and read over the network
 * Requires KinectBodyData.cs
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodyDataConverter : MonoBehaviour {

    public GameObject KinectBodyData;
    
    // Dictionary that maps body trackingIDs to an array of joint locations
    private Dictionary<ulong, GameObject[]> _Bodies =
        new Dictionary<ulong, GameObject[]>();
    private BodySourceManager _BodyManager;

    // Public function so other scripts can send out the data
    public Dictionary<ulong, GameObject[]> GetData() {
        return _Bodies;
    }
    
    void Update() {

        if (KinectBodyData == null) {
            Debug.Log("No Kinect Body Data");
            return;
        }
        
        _BodyManager = KinectBodyData.GetComponent<BodySourceManager>();
        if (_BodyManager == null) {
            Debug.Log("No Body source Manager");
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null) {
            return;
        }

        // Get tracked IDs
        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data) {

            if (body == null) {
                continue;
            }
                
            if (body.IsTracked) {
                trackedIds.Add(body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // Delete untracked bodies
        foreach (ulong trackingId in knownIds){

            if (!trackedIds.Contains(trackingId)) {
                _Bodies.Remove(trackingId);
            }
        }

        // If a body is tracked, update the dictionary
        foreach (var body in data) {

            if (body == null) {
                continue;
            }
            
            if (body.IsTracked) {
                if (!_Bodies.ContainsKey(body.TrackingId)) {
                    _Bodies[body.TrackingId] = CreateBodyData();
                }
                
                UpdateBodyData(body, _Bodies[body.TrackingId]);
            }
        }
    }
    
    private GameObject[] CreateBodyData() {
        GameObject[] body = new GameObject[25];
        for (int i = 0; i < 25; i++) {
            body[i] = new GameObject();
            body[i].transform.position = Vector3.zero;
            body[i].transform.rotation = Quaternion.identity;
            body[i].transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        return body;
    }
    
    private void UpdateBodyData(Kinect.Body body, GameObject[] bodyData) {

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++) {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.JointOrientation sourceJointOrientation = body.JointOrientations[jt];
            //Debug.Log("Source joint:" + sourceJoint);
            //Debug.Log("Vector:" + GetVector3FromJoint(sourceJoint));
            //Debug.Log("BodyData[(int)jt]:" + bodyData[(int)jt]);
            bodyData[(int)jt].transform.position = GetVector3FromJoint(sourceJoint);
            bodyData[(int)jt].transform.rotation = GetQuaternionFromJointOrientation(sourceJointOrientation);
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    private static Quaternion GetQuaternionFromJointOrientation(Kinect.JointOrientation jointOrientation)
    {
        return new Quaternion(jointOrientation.Orientation.X, jointOrientation.Orientation.Y, jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
    }
}
