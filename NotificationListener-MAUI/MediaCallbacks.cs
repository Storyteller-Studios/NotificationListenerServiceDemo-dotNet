using Android.Media;
using Android.Media.Session;
using System;
using System.Collections.Generic;
using System.Text;
using MediaController = Android.Media.Session.MediaController;

namespace NotificationListener_MAUI
{
    public class MediaCallback : MediaController.Callback
    {
        public override void OnMetadataChanged(MediaMetadata? metadata)
        {
            base.OnMetadataChanged(metadata);
            MediaSessionInstance.Instance?.OnMetadataChanged(metadata);
        }
        public override void OnPlaybackStateChanged(PlaybackState? state)
        {
            base.OnPlaybackStateChanged(state);
            MediaSessionInstance.Instance?.OnMediaPlaybackStateChanged(state);
        }
        public override void OnSessionDestroyed()
        {
            base.OnSessionDestroyed();
            MediaSessionInstance.Instance?.OnSessionDestroyed();
        }
    }
    public interface IMediaSessionTransportationCallback
    {
        void OnListenerConnected();
        void OnListenerDisconnected();
        void OnMediaSessionCreated(MediaSession.Token token);
        void OnMetadataChanged(MediaMetadata? metadata);
        void OnMediaPlaybackStateChanged(PlaybackState? state);
        void OnSessionDestroyed();
    }
    public static class MediaSessionInstance
    {
        public static IMediaSessionTransportationCallback? Instance;
    }
}