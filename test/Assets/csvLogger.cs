using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RosSharp.RosBridgeClient;
public class SessionCsvLogger : MonoBehaviour
{
    public bool logControlAlpha = true;
    public bool logForce = true;
    public bool logTargetReached = false;
    public float alpha = 0;
    public float kHN = 0;
    public float inter = 0;
    public double[] forceDirection = new double[3] { 0.0, 0.0, 0.0 };
    public int targetReached = 0;
    public GameObject rosConnector;
    public GameObject DM;
    //public GameObject KN_Publisher;
    public string username = "_Test1";
    public string sessionDir;
    Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();

    void Awake()
    {
        long unixStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        sessionDir = Path.Combine($"D:\\Unity\\data\\", unixStamp.ToString());
        sessionDir = sessionDir + username;
        Directory.CreateDirectory(sessionDir);

        if (logControlAlpha) writers["control_alpha"] = CreateCsv("control_alpha.csv", "unix_stamp,FinalAlphaWithIntercept,kHN,intercept");
        if (logForce) writers["force"] = CreateCsv("force.csv", "unix_stamp,force,align,controlalpha, lastState");
        if (logTargetReached) writers["target_reached"] = CreateCsv("target_reached.csv", "unix_stamp,target_reached");

        writers["allInOne"] = CreateCsv("allInOne.csv", "unix_stamp,FinalAlphaWithIntercept,kHN,intercept,force,align,controlalpha, lastState,target_reached");

        DM.GetComponent<Data2CSV>().name = sessionDir;
        //DM.GetComponent<Data2CSV>().ExportToCsv();
    }

    StreamWriter CreateCsv(string fname, string header)
    {
        var sw = new StreamWriter(Path.Combine(sessionDir, fname));
        sw.WriteLine(header);
        sw.Flush();
        return sw;
    }

    static double UnixNow() =>
        (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

    public void LogCA(float force,float kHN,float inter)
    {
       
        if (!logControlAlpha) return;
        writers["control_alpha"].WriteLine($"{UnixNow():F3},{force:F4},{kHN:F4},{inter:F4}");
    }

    public void LogForce(float currentForce,float align, float force, string LS)
    {
        if (!logForce) return;
        writers["force"].WriteLine($"{UnixNow():F3},{currentForce:F4},{align:F4},{force:F4},{LS}");
    }

    public void LogTarget(DataManager D)
    {
        if (!logTargetReached) return;
        writers["target_reached"].WriteLine($"{UnixNow():F3},{D.collectCount}");
    }

    public void LogAllInOne(float FAWI, float kHN, float inter, float currentForce, float align, float force, string LS)
    {
        writers["allInOne"].WriteLine($"{UnixNow():F3},{FAWI:F4},{kHN:F4},{inter:F4},{currentForce:F4},{align:F4},{force:F4},{LS},{DataManager.Instance.collectCount}");
    }

    // optional: record target_reached every frame
    void Update()
    {
        alpha = rosConnector.GetComponent<GetAlphaValue>().GetValue();
        kHN = rosConnector.GetComponent<kHN_Subscriber>().kHN;
        inter = rosConnector.GetComponent<Intercept_Subscriber>().inter;
        LogCA(alpha,kHN,inter);
        LogForce(rosConnector.GetComponent<HapticControl>().f,
            rosConnector.GetComponent<HapticControl>().align,
            rosConnector.GetComponent<HapticControl>().controlAlpha,
            rosConnector.GetComponent<HapticControl>().lastState);
        LogAllInOne(alpha, kHN, inter, rosConnector.GetComponent<HapticControl>().f,
            rosConnector.GetComponent<HapticControl>().align,
            rosConnector.GetComponent<HapticControl>().controlAlpha,
            rosConnector.GetComponent<HapticControl>().lastState);

    }

    // Ensure buffers are flushed on quit / stop
    void OnApplicationQuit() => CloseAll();
    void OnDisable() => CloseAll();
    void CloseAll()
    {
        foreach (var w in writers.Values)
            w?.Close();
        writers.Clear();
    }

}