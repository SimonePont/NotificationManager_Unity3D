using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Notifications.Android;

public class MainScript : MonoBehaviour
{

    public string not_text=""; //string containing the text to print on the screen
    public string not_title="HELLO WORLD!"; //string containing the title to print on the screen
    public Text NotText; //Text object for the text printed on the screen
    public Text NotTitle; //Text object for the text printed on the screen
    public int notifications_to_schedule = 5; //number of notifications to be scheduled at every "add" call
    public int time_between_notifications = 60; //seconds between the scheduling of a notification and the following one

    string _channel_ID = "NotifCh"; //identifier of the notification channel
    AndroidJavaClass android_class; //class of the current activity
    AndroidJavaObject unity_activity; //object representing the current activity
    AndroidJavaObject unity_context; //object representing the current context
    AndroidJavaObject android_plugin_instance; //instance of the NotificationManagerLib plugin
    AndroidNotificationChannel channel = new AndroidNotificationChannel()
    {
        Id = "NotifCh",
        Name = "Default Notification Channel",
        Importance = Importance.High,
        Description = "Generic notifications",
        EnableVibration = true,
    };
    AndroidNotification notification; //notification object used as blueprint

    /// <summary>
    /// Function used to initialize the android plugin
    /// </summary>
    /// <param name="plugin_name"> string containing the package of the android plugin that has to be initialized</param>
    void InitializePlugin(string plugin_name)
    {
        android_class = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unity_activity = android_class.GetStatic<AndroidJavaObject>("currentActivity");
        unity_context = unity_activity.Call<AndroidJavaObject>("getApplicationContext");
        android_plugin_instance = new AndroidJavaObject(plugin_name);
        if (android_plugin_instance == null)
        {
            Debug.Log("Plugin instance error!");
            //not_text = "Plugin instance error!";
        }
        else
        {
            android_plugin_instance.CallStatic("ReceiveUnityActivity", unity_activity);
            android_plugin_instance.CallStatic("ReceiveUnityContext", unity_context);
            Debug.Log("Plugin instance OK!");
            //not_text = "Plugin instance OK!";
        }
    }

    /// <summary>
    /// Function used to check if a new notification has to be sent and fire it through the pre-defined channel
    /// </summary>
    void CheckAndSendNotifications()
    {
        char notify_id = (char)0;
        int img_id = 0;
        string title = "Default title";
        string text = "Default text";
        string icon = "icon_";
        notify_id = android_plugin_instance.Call<char>("CheckNot");
        if (notify_id != (char)0)
        {
            title = android_plugin_instance.Call<string>("GetNotTitle", notify_id);
            text = android_plugin_instance.Call<string>("GetNotText", notify_id);
            img_id = android_plugin_instance.Call<int>("GetNotImgIDByID", notify_id);
            icon = "icon_" + img_id.ToString();
            //not_title = title;
            notification.Title = title;
            notification.Text = text;
            notification.ShouldAutoCancel = true;
            notification.IntentData = "intent";
            notification.SmallIcon = icon;
            notification.FireTime = System.DateTime.Now;
            AndroidNotificationCenter.SendNotificationWithExplicitID(notification, _channel_ID, notify_id);
            android_plugin_instance.Call("DelSingleNot", notify_id);
        }
    }

        public void ListNotifications()
    {
        /* Not implemented!! */
        android_plugin_instance.Call("ShowToastNot", "Not implemented....work in progress");
    }

    /// <summary>
    /// Function used to schedule a new set of notifications ('notifications_to_schedule' notifications, one every 'time_between_notifications' seconds)
    /// </summary>
    public void ScheduleNotifications()
    {
        if (android_plugin_instance != null)
        {
            android_plugin_instance.Call("AddNot", notifications_to_schedule, time_between_notifications);
        }
    }

    /// <summary>
    /// Function used to delete all the scheduled notifications
    /// </summary>
    public void DeleteAllNotifications()
    {
        if (android_plugin_instance != null)
        {
            android_plugin_instance.Call("DelAllNot");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bool res;
        NotText = GameObject.Find("Text").GetComponent<Text>();
        NotTitle = GameObject.Find("Title").GetComponent<Text>();
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
        InitializePlugin("com.example.notificationmanagerlib.NotificationManagerClass");
        if (android_plugin_instance != null)
        {
            /* Not necessary until the unity context is correctly managed by the android plugin
             * 
            res = android_plugin_instance.Call<bool>("createNotificationChannel");
            not_text = "Channel created with return " + res.ToString();
            */
        }
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        if (notificationIntentData != null)
        {
            var id = notificationIntentData.Id;
            var channel = notificationIntentData.Channel;
            var notification = notificationIntentData.Notification;
            not_text = notification.Text;
            not_title = notification.Title;
        }
        else
        {
            not_text = "Notification manager app by Pont Simone";
            not_title = "HELLO WORLD!";
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();
        
        if (android_plugin_instance != null)
        {
            if (notificationIntentData != null)
            {
                var id = notificationIntentData.Id;
                var channel = notificationIntentData.Channel;
                var notification = notificationIntentData.Notification;
                not_text = notification.Text;
                not_title = notification.Title;
                //android_plugin_instance.Call("DelSingleNot", id);
            }
            CheckAndSendNotifications();
        }
        NotText.text = not_text;
        NotTitle.text = not_title;
    }
}
