using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationListener_MAUI
{
    public class SpotifyBroadcastReceiver : BroadcastReceiver
    {
        static string SPOTIFY_PACKAGE = "com.spotify.music";
        static string PLAYBACK_STATE_CHANGED = SPOTIFY_PACKAGE + ".playbackstatechanged";
        static string METADATA_CHANGED = SPOTIFY_PACKAGE + ".metadatachanged";
        public override void OnReceive(Context? context, Intent? intent)
        {
            var action = intent?.Action;
            if (action == METADATA_CHANGED)
            {
                var metadata = new Metadata()
                {
                    Id = intent?.GetStringExtra("id"),
                    Artist = intent?.GetStringExtra("artist"),
                    Album = intent?.GetStringExtra("album"),
                    Name = intent?.GetStringExtra("name"),
                    Length = intent?.GetIntExtra("length", 0)

                };
                SpotifyBroadcastCallbackInstance.Instance?.OnMetadataChanged(metadata);
            }
            else if (action==PLAYBACK_STATE_CHANGED)
            {
                var playbackStatus = new PlaybackStatus()
                {
                    Playing = intent?.GetBooleanExtra("playing", false),
                    Position = intent?.GetIntExtra("playbackPosition", 0)
                };
                SpotifyBroadcastCallbackInstance.Instance?.OnPlaybackStatusChanged(playbackStatus);
                // Do something with extracted information
            }
        }
    }
    public interface ISpotifyBroadcastCallback
    {
        public void OnPlaybackStatusChanged(PlaybackStatus status);
        public void OnMetadataChanged(Metadata metadata);
    }
    public static class SpotifyBroadcastCallbackInstance
    {
        public static ISpotifyBroadcastCallback? Instance { get; set; } = null;
    }
    public class Metadata
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
        public int? Length { get; set; }
    }
    public class PlaybackStatus
    {
        public bool? Playing { get; set; }
        public int? Position { get; set; }
    }
}
