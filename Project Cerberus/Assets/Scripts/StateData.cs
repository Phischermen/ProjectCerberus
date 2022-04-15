using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class StateData
{
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        PhotonPeer.RegisterType(typeof(StateData), (byte) 'S', Serialize, Deserialize);
    }
    
    public bool[] booleans = new bool[8]; //1 Byte
    public byte myByte; //1 byte
    //public float myFloat; //4 bytes
    public Vector2Int myVector2Int; // 2 bytes

    public virtual void Load()
    {
        // Deliberately empty
    }

    public static byte[] Serialize(object o)
    {
        var stateData = (StateData) o;
        byte byte1 = 0;
        byte db = 1;
        // Serialize booleans.
        foreach (var b in stateData.booleans)
        {
            if (b) byte1 += db;
            // Double db. 1 -> 2 -> 4 -> ... -> 128
            db += db;
        }
        // Serialize Vector2Int
        var byte2 = (byte)stateData.myVector2Int.x;
        var byte3 = (byte)stateData.myVector2Int.y;
        return new byte[] {byte1, stateData.myByte, byte2, byte3};
    }

    public static object Deserialize(byte[] data)
    {
        var stateData = new StateData();
        for (var i = 0; i < stateData.booleans.Length; i++)
        {
            // Check each bit of the first byte to see its on.
            stateData.booleans[i] = (data[0] & (1 << i)) != 0;
        }

        stateData.myByte = data[1];
        stateData.myVector2Int.x = data[2];
        stateData.myVector2Int.x = data[3];
        return stateData;
    }
}