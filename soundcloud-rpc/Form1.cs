using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace soundcloud_rpc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rpcManager.Start();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            rpcManager.Stop();
        }

        async Task<string> ExecuteScript(string script)
        {
            string temp = await webView21.ExecuteScriptAsync(script);
            if (temp.StartsWith("\"") && temp.EndsWith("\""))
            {
                return temp.Substring(1, temp.Length - 2);
            }
            else
            {
                return temp;
            }
        }

        Random random = new Random();
        bool initialized = false;

        List<string> lastKnownData = new List<string>();

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (!initialized)
            {
                await ExecuteScript("function GetSongTitle()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__titleLink sc-truncate sc-text-h5 sc-link-primary\")[0].title;\r\n} ");
                await ExecuteScript("function GetArtistName()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__lightLink sc-link-light sc-link-secondary sc-truncate sc-text-h5\")[0].title;\r\n} ");
                await ExecuteScript("function GetCurrentTime()\r\n{\r\nreturn document.getElementsByClassName(\"playbackTimeline__timePassed sc-text-primary sc-text-h5\")[0].childNodes[1].textContent;\r\n}");
                await ExecuteScript("function GetSongDuration()\r\n{\r\nreturn document.getElementsByClassName(\"playbackTimeline__duration sc-text-primary sc-text-h5\")[0].childNodes[1].textContent;\r\n}");
                await ExecuteScript("function GetSongLink()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__titleLink sc-truncate sc-text-h5 sc-link-primary\")[0].href;\r\n} ");
                await ExecuteScript("function GetAlbumCoverLink() \r\n{ \r\nlet a = document.getElementsByClassName(\"playbackSoundBadge__avatar sc-media-image sc-mr-2x\")[0].children[0].children[0].style.backgroundImage;\r\nif (a.startsWith(\"url\"))\r\n{\r\nreturn a.substring(5, a.length-2)\r\n}\r\nelse\r\n{\r\nreturn null\r\n}\r\n} ");
                await ExecuteScript("function IsPlaying()\r\n{\r\nreturn document.getElementsByClassName(\"playControls__elements\")[0].getElementsByClassName(\"playControl sc-ir playControls__control playControls__play\")[0].classList.contains(\"playing\");\r\n}");

                initialized = await ExecuteScript("GetSongTitle()") != "null" && await ExecuteScript("GetArtistName()") != "null" && await ExecuteScript("GetSongLink()") != "null";
                return;
            }

            // dont ratelimit me pls
            // 5 requests / 20 seconds is what discord documentation states is the max
            // yet im going to be 2x slower to be extra sure
            timer1.Interval = random.Next(8000, 12000);

            string songTitle = await ExecuteScript("GetSongTitle()");
            string artistName = await ExecuteScript("GetArtistName()");
            string songLink = await ExecuteScript("GetSongLink()");
            string albumCoverLink = await ExecuteScript("GetAlbumCoverLink()");

            string currentTime = await ExecuteScript("GetCurrentTime()");
            string songDuration = await ExecuteScript("GetSongDuration()");

            bool isPlaying = await ExecuteScript("IsPlaying()") == "true";

            void UpdateStatus()
            {
                if (isPlaying)
                {
                    rpcManager.ListenToSong(songTitle, artistName, currentTime, songDuration, songLink, albumCoverLink);
                }
                else
                {
                    rpcManager.Idle(songTitle, artistName, songLink, albumCoverLink);
                }
            }

            if (lastKnownData.Count == 0)
            {
                lastKnownData = new List<string> { songTitle, artistName, songLink, currentTime, songDuration, isPlaying.ToString() };
                UpdateStatus();
            }
            else
            {
                List<string> currentData = new List<string> { songTitle, artistName, songLink, currentTime, songDuration, isPlaying.ToString() };

                bool dataChanged = false;
                for (int i = 0; i < currentData.Count; i++)
                {
                    if (lastKnownData[i] != currentData[i])
                    {
                        dataChanged = true;
                        break;
                    }
                }

                if (dataChanged)
                {
                    UpdateStatus();
                    lastKnownData = new List<string>(currentData);
                }
            }
        }
    }
}
