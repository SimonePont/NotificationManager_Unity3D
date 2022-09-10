using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Notifications.Android;

public class MainScript : MonoBehaviour
{

    public string debug_text = "init";
    public Text DbText;
    public int res = 0;

    string _channel_ID = "NotCH";
    AndroidJavaClass android_class;
    AndroidJavaObject unity_activity;
    AndroidJavaObject unity_context;
    AndroidJavaObject android_plugin_instance;
    AndroidNotificationChannel channel = new AndroidNotificationChannel()
    {
        Id = "NotCH",
        Name = "Default Channel",
        Importance = Importance.High,
        Description = "Generic notifications",
    };
    AndroidNotification notification;


    public void Test()
    {
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        Debug.Log("Button pressed");
        //android_plugin_instance.Call("Notify", (char)0, "Title", "Text");

        notification= new AndroidNotification("title", "string text", System.DateTime.Now);
        AndroidNotificationCenter.SendNotification(notification, _channel_ID);
        debug_text = "Instant notify";
    }

    void InitializePlugin(string plugin_name)
    {
        android_class = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unity_activity = android_class.GetStatic<AndroidJavaObject>("currentActivity");
        unity_context = unity_activity.Call<AndroidJavaObject>("getApplicationContext");
        android_plugin_instance = new AndroidJavaObject(plugin_name);
        if (android_plugin_instance == null)
        {
            Debug.Log("Plugin instance error!");
            debug_text = "Plugin instance error!";
        }
        else
        {
            android_plugin_instance.CallStatic("ReceiveUnityActivity", unity_activity);
            android_plugin_instance.CallStatic("ReceiveUnityContext", unity_context);
            
            debug_text = "Plugin instance OK!";
        }
    }

    public void ScheduleNotifications()
    {
        if (android_plugin_instance != null)
        {
            android_plugin_instance.Call("AddNot");
            //debug_text = "Notification scheduled! "+res.ToString();
        }
    }

    public void DeleteAllNotifications()
    {
        if (android_plugin_instance != null)
        {
            android_plugin_instance.Call("DelAllNot");
            //debug_text = "Notification deleted!";
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializePlugin("com.example.notificationmanagerlib.NotificationManagerClass");
        if (android_plugin_instance != null)
        {
            res=android_plugin_instance.Call<int>("createNotificationChannel");
            debug_text = "Channel created with return " + res.ToString();
        }
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            var id = notificationIntentData.Id;
            var channel = notificationIntentData.Channel;
            var notification = notificationIntentData.Notification;
            debug_text = notification.Text;
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        char notify_id = (char)0;
        string title="Default title"; 
        string text="Default text";
        if (android_plugin_instance != null)
        {
            notify_id = android_plugin_instance.Call<char>("CheckNot");
            if (notify_id != 0)
            {
                title= android_plugin_instance.Call<string>("GetNotTitle", notify_id);
                text = android_plugin_instance.Call<string>("GetNotText", notify_id);
                //android_plugin_instance.Call("DelSingleNot", notify_id);
                debug_text = title;
                notification.Title = title;
                notification.Text = text; 
                notification.ShouldAutoCancel = true;
                notification.IntentData = " ";
                notification.FireTime = System.DateTime.Now;
                AndroidNotificationCenter.SendNotificationWithExplicitID(notification, _channel_ID, notify_id);
                //AndroidNotificationCenter.SendNotification(notification, "channel_id");
            }
        }
        DbText.text = debug_text;
    }
}
