using Android.Media.Session;
using Android.OS;
using Android.Service.Notification;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationListener_MAUI
{
    [Service(Label = "NotificationListener", Permission = "android.permission.BIND_NOTIFICATION_LISTENER_SERVICE", Exported = true)]
    [IntentFilter(["android.service.notification.NotificationListenerService"])]
    public class NotificationListener : NotificationListenerService
    {
        public static readonly Class MediaSessionClass = Class.FromType(typeof(MediaSession.Token));
        public static readonly Class NotificationListenerClass = Class.FromType(typeof(NotificationListener));
        public override void OnNotificationPosted(StatusBarNotification? sbn)
        {
            base.OnNotificationPosted(sbn);
            if (sbn?.PackageName == "com.spotify.music")
            {
                var notification = sbn.Notification;
                IParcelable? parcel;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
                {
#pragma warning disable CA1416 // 验证平台兼容性
                    parcel = (IParcelable?)notification?.Extras?.GetParcelable(Notification.ExtraMediaSession, MediaSessionClass);
#pragma warning restore CA1416 // 验证平台兼容性
                }
                else
                {
#pragma warning disable CA1422 // 验证平台兼容性
                    parcel = (IParcelable?)notification?.Extras?.GetParcelable(Notification.ExtraMediaSession);
#pragma warning restore CA1422 // 验证平台兼容性
                }
                if (parcel != null)
                {
                    var result = (MediaSession.Token)parcel;
                    MediaSessionInstance.Instance?.OnMediaSessionCreated(result);
                }
            }
        }
        public override void OnListenerConnected()
        {
            base.OnListenerConnected();
            MediaSessionInstance.Instance?.OnListenerConnected();
        }
        public override void OnListenerDisconnected()
        {
#pragma warning disable CA1416 // 验证平台兼容性
            base.OnListenerDisconnected();
#pragma warning restore CA1416 // 验证平台兼容性
            MediaSessionInstance.Instance?.OnListenerDisconnected();
        }
    }
}
