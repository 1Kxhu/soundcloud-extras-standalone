using DiscordRPC;
using System;

namespace soundcloud_rpc
{
    public static class rpcManager
    {
        // my application's id, if you want to put in your own images:
        // go to https://discord.com/developers/applications
        // and make a new 'discord application'
        // with your own 'rich presence assets' in the 'rich presence' tab
        /* 7ow (the maintainer, that's me)'s application has these assets:
            - soundcloud-black
            - soundcloud-black-modal
            - paused
        */
        const string applicationID = "1346552289212760146";
        private static DiscordRpcClient client = new DiscordRpcClient(applicationID);
        private static DateTime startTime;

        public static void Start()
        {
            if (!client.IsInitialized)
            {
                client.Initialize();
                startTime = DateTime.UtcNow;
            }
        }

        public static void Stop()
        {
            client.Dispose();
        }

        public static void ListenToSong(string songTitle, string artistName, string songTime, string songDuration, string albumCoverLink)
        {
            client.SetPresence(new RichPresence()
            {
                Details = songTitle,
                State = $"by {artistName} ( {songTime} / {songDuration} )",
                Assets = new Assets()
                {
                    SmallImageKey = "soundcloud-black",
                    SmallImageText = "Listening to SoundCloud",
                    LargeImageKey = albumCoverLink,
                },
                Timestamps = new Timestamps()
                {
                    Start = startTime
                }
            });

            /* 
                Song Title
                by Artist ( 1:12 / 3:50 )
            */
        }

        public static void ListenToModal(string songTitle, string artistName, string albumCoverLink, string action, string primaryPersonName)
        {
            string actionVerb = action.Contains("repost") ? "reposted" : "posted";

            client.SetPresence(new RichPresence()
            {
                Details = songTitle,
                State = $"by {artistName} • {primaryPersonName} {actionVerb}",
                Assets = new Assets()
                {
                    SmallImageKey = "soundcloud-black-modal",
                    SmallImageText = "Modal View",
                    LargeImageKey = albumCoverLink,
                },
                Timestamps = new Timestamps()
                {
                    Start = startTime
                }
            });

            /* 
               Title
               by Artist • Person2 reposted
           */
        }

        public static void Idle(string songTitle, string artistName, string albumCoverLink)
        {
            client.SetPresence(new RichPresence()
            {
                Details = songTitle,
                State = $"by {artistName}",
                Assets = new Assets()
                {
                    SmallImageKey = "paused",
                    SmallImageText = "Paused",
                    LargeImageKey = albumCoverLink,
                },
                Timestamps = new Timestamps()
                {
                    Start = startTime
                }
            });

            /* 
                Song Title
                by Artist
            */
        }
    }
}
