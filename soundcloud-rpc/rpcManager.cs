using DiscordRPC;
using Button = DiscordRPC.Button;

namespace soundcloud_rpc
{
    public static class rpcManager
    {
        const string applicationID = "1346552289212760146";

        private static DiscordRPC.DiscordRpcClient client = new DiscordRPC.DiscordRpcClient(applicationID);

        public static void Start()
        {
            client.Initialize();
        }

        public static void Stop()
        {
            client.Dispose();
        }

        public static void ListenToSong(string songTitle, string artistName, string songTime, string songDuration, string songLink, string albumCoverLink)
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
            });
        }

        public static void Idle(string songTitle, string artistName, string songLink, string albumCoverLink)
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
            });
        }
    }
}
