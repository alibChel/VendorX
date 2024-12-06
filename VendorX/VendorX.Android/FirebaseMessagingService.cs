using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
//using Firebase.Iid;
//using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VendorX.Services;

namespace VendorX.Droid
{
    //[Service(Exported = false)]
    //[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    //public class FirebaseIIDService : FirebaseInstanceIdService
    //{
    //    const string TAG = "FirebaseIIDService";
    //    public override void OnTokenRefresh()
    //    {
    //        var refreshedToken = FirebaseInstanceId.Instance.Token;
    //        Log.Debug(TAG, "Refreshed token: " + refreshedToken);
    //        SendRegistrationToServer(refreshedToken);
    //    }
    //    void SendRegistrationToServer(string token)
    //    {

    //    }
    //}
    //[Service(Exported = false)]
    //[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    //public class FirebaseMessagingService : Firebase.Messaging.FirebaseMessagingService
    //{
    //    const string TAG = "FirebaseMsgService";
    //    public override void OnMessageReceived(RemoteMessage message)
    //    {
    //        base.OnMessageReceived(message);
    //        Log.Debug(TAG, "Notification Message Body: " + message.GetNotification().Body);
    //        //var messageBody = message.GetNotification().Body;
    //        //SendLocalNotification(messageBody);

    //    }
    //public void SendLocalNotification(string messageBody)
    //{
    //    var intent = new Intent(this, typeof(MainActivity));
    //    intent.AddFlags(ActivityFlags.ClearTop);
    //    intent.PutExtra("message", messageBody);

    //    var requestCode = new Random().Next();
    //    var pendingIntent = PendingIntent.GetActivities(this, requestCode, new Intent[] { intent }, PendingIntentFlags.OneShot);

    //    var notificationBuilder = new NotificationCompat.Builder(this)
    //        .SetContentTitle("test")
    //        .SetContentText(messageBody)
    //        .SetContentIntent(pendingIntent);
    //    var notificationManager = NotificationManager.FromContext(this);

    //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
    //    {
    //        NotificationChannel channel = new NotificationChannel("Notification", "Vendor", NotificationImportance.High);
    //        notificationManager.CreateNotificationChannel(channel);

    //        notificationBuilder.SetChannelId(channel.Id);
    //    }
    //    notificationManager.Notify(0, notificationBuilder.Build());
    //}
    //}
}