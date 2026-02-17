using Android.Content;
using Android.Media.Session;
using Android.Media;
using Android.OS;
using Android.Service.Notification;
using Timer = System.Timers.Timer;
using Java.Lang;
using MediaController = Android.Media.Session.MediaController;
using AndroidX.Core.App;
using Android.Support.V4.App;

namespace NotificationListener_MAUI
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity, IMediaSessionTransportationCallback, ISpotifyBroadcastCallback
    {
        Timer SessionTimer = new Timer() { AutoReset = true, Enabled = true, Interval = 100 };
        TextView? StatusView;
        TextView? Position;
        TextView? SongName;
        TextView? Artist;
        TextView? Duration;
        TextView? Url;
        Button? MoveNext;
        Button? MovePrevious;
        Button? PausePlay;
        Button? PositionSet;
        EditText? PositionBox;
        MediaController? Controller;
        SpotifyBroadcastReceiver? Receiver;
        MediaController.TransportControls? TransportControls;
        MediaCallback MediaCallback = new MediaCallback();
        ComponentName? NotificationListenerServiceComponentName;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            StatusView = FindViewById<TextView>(Resource.Id.StatusView);
            SongName = FindViewById<TextView>(Resource.Id.SongName);
            Artist = FindViewById<TextView>(Resource.Id.Artist);
            Duration = FindViewById<TextView>(Resource.Id.Duration);
            Position = FindViewById<TextView>(Resource.Id.Position);
            Url = FindViewById<TextView>(Resource.Id.Url);
            MoveNext = FindViewById<Button>(Resource.Id.MoveNext);
            MovePrevious = FindViewById<Button>(Resource.Id.MovePrevious);
            PausePlay = FindViewById<Button>(Resource.Id.PausePlay);
            PositionSet = FindViewById<Button>(Resource.Id.PositionSet);
            PositionBox = FindViewById<EditText>(Resource.Id.PositionBox);
            MoveNext!.Click += MoveNext_Click;
            MovePrevious!.Click += MovePrevious_Click;
            PausePlay!.Click += PausePlay_Click;
            PositionSet!.Click += PositionSet_Click;
            MediaSessionInstance.Instance = this;
            SpotifyBroadcastCallbackInstance.Instance = this;
            Receiver = new SpotifyBroadcastReceiver();
            RegisterReceiver(Receiver, new IntentFilter("com.spotify.music.metadatachanged"));
            RegisterReceiver(Receiver, new IntentFilter("com.spotify.music.playbackstatechanged"));
            var permitted = NotificationManagerCompat.GetEnabledListenerPackages(this).Contains(PackageName!);
            if (!permitted)
            {
                var intent = new Intent("android.settings.ACTION_NOTIFICATION_LISTENER_SETTINGS");
                StartActivity(intent);
            }
            StatusView!.Text = "NotConnected";
            NotificationListenerServiceComponentName = new ComponentName(this, NotificationListener.NotificationListenerClass);
            SessionTimer.Elapsed += SessionTimer_Elapsed;
            if (permitted)
            {
                RequestReBind();
                CreateSessionFromMediaSessionManager();
            }
        }

        private void PositionSet_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(PositionBox?.Text))
            {
                var succeed = int.TryParse(PositionBox.Text, out int result);
                TransportControls?.SeekTo(succeed ? result : 0);
            }
        }

        private void RequestReBind()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
#pragma warning disable CA1416 // 验证平台兼容性
                NotificationListenerService.RequestRebind(NotificationListenerServiceComponentName);
#pragma warning restore CA1416 // 验证平台兼容性
            }
            else
            {
                PackageManager!.SetComponentEnabledSetting(NotificationListenerServiceComponentName!, Android.Content.PM.ComponentEnabledState.Disabled, Android.Content.PM.ComponentEnableOption.DontKillApp);
                PackageManager.SetComponentEnabledSetting(NotificationListenerServiceComponentName!, Android.Content.PM.ComponentEnabledState.Enabled, Android.Content.PM.ComponentEnableOption.DontKillApp);
            }
        }
        private void PausePlay_Click(object? sender, System.EventArgs e)
        {
            if (Controller?.PlaybackState?.State == PlaybackStateCode.Paused)
            {
                TransportControls?.Play();
            }
            else if (Controller?.PlaybackState?.State == PlaybackStateCode.Playing)
            {
                TransportControls?.Pause();
            }
        }

        private void MovePrevious_Click(object? sender, System.EventArgs e)
        {
            TransportControls?.SkipToPrevious();
        }

        private void MoveNext_Click(object? sender, System.EventArgs e)
        {
            TransportControls?.SkipToNext();
        }

        private void CreateSessionFromMediaSessionManager()
        {
            var manager = (MediaSessionManager)GetSystemService(MediaSessionService)!;
            var activeSpotifySession = manager.GetActiveSessions(NotificationListenerServiceComponentName).Where(t => t.PackageName == "com.spotify.music").FirstOrDefault();
            if (activeSpotifySession != null)
            {
                StatusView!.Text = "Created-SessionManager";
                Controller = activeSpotifySession;
                Controller.RegisterCallback(MediaCallback);
                TransportControls = Controller.GetTransportControls();
                SongName!.Text = Controller?.Metadata?.GetString(MediaMetadata.MetadataKeyTitle);
                Artist!.Text = Controller?.Metadata?.GetString(MediaMetadata.MetadataKeyArtist);
                Duration!.Text = Controller?.Metadata?.GetLong(MediaMetadata.MetadataKeyDuration).ToString();
                Position!.Text = "Unknown";
            }
        }
        private void SessionTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (Controller?.PlaybackState?.State == PlaybackStateCode.Playing)
                {

                    Position!.Text = Controller.PlaybackState.Position.ToString();
                }

            }
            catch
            {
                // Ignore
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Controller?.UnregisterCallback(MediaCallback);
            Controller?.Dispose();
            TransportControls?.Dispose();
            UnregisterReceiver(Receiver);
            Controller = null;
            TransportControls = null;
            MediaSessionInstance.Instance = null;
            SpotifyBroadcastCallbackInstance.Instance = null;
        }
        public void OnMediaSessionCreated(MediaSession.Token token)
        {
            if (!token.Equals(Controller?.SessionToken))
            {
                StatusView!.Text = "Created-Notification";
                if (Controller != null)
                {
                    Controller.UnregisterCallback(MediaCallback);
                    Controller.Dispose();
                    TransportControls?.Dispose();
                }
                var mediaController = new MediaController(this, token);
                Controller = mediaController;
                Controller.RegisterCallback(MediaCallback);
                TransportControls = Controller.GetTransportControls();
                SongName!.Text = Controller.Metadata?.GetString(MediaMetadata.MetadataKeyTitle);
                Artist!.Text = Controller.Metadata?.GetString(MediaMetadata.MetadataKeyArtist);
                Duration!.Text = Controller.Metadata?.GetLong(MediaMetadata.MetadataKeyDuration).ToString();
                Position!.Text = "Unknown";
            }
        }

        public void OnListenerConnected()
        {
            StatusView!.Text = "Connected";
        }

        public void OnListenerDisconnected()
        {
            StatusView!.Text = "Disconnected";
        }

        public void OnMetadataChanged(MediaMetadata? metadata)
        {
            StatusView!.Text = "Created-Metadata";
            SongName!.Text = metadata?.GetString(MediaMetadata.MetadataKeyTitle);
            Artist!.Text = metadata?.GetString(MediaMetadata.MetadataKeyArtist);
            Duration!.Text = metadata?.GetLong(MediaMetadata.MetadataKeyDuration).ToString();
            Position!.Text = "Unknown";
        }

        public void OnMediaPlaybackStateChanged(PlaybackState? state)
        {

        }

        public void OnSessionDestroyed()
        {
            Controller?.UnregisterCallback(MediaCallback);
            Controller?.Dispose();
            TransportControls?.Dispose();
            StatusView!.Text = "Destroyed-Session";
            SongName!.Text = "Destroyed";
            Artist!.Text = "Destroyed";
            Duration!.Text = "Destroyed";
            Position!.Text = "Unknown";
            Controller = null;
            TransportControls = null;
        }

        public void OnPlaybackStatusChanged(PlaybackStatus status)
        {
            if (status.Playing ?? false)
            {
                SessionTimer?.Start();
            }
            else
            {
                SessionTimer?.Stop();
                Position!.Text = status.Position?.ToString();
            }
        }

        public void OnMetadataChanged(Metadata metadata)
        {
            Url!.Text = metadata?.Id;
        }
    }
}