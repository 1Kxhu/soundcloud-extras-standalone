using soundcloud_rpc.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace soundcloud_rpc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            rpcManager.Start();
            InitializeComponent();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            rpcManager.Stop();
        }

        async Task<string> ExecuteScript(string script)
        {
            await webView21.EnsureCoreWebView2Async();

            string temp = await webView21.ExecuteScriptAsync(script);

            // Strings are returned with opening and closing double quotes by the WebView2
            // So we just cut them
            if (temp.StartsWith("\"") && temp.EndsWith("\""))
            {
                return temp.Substring(1, temp.Length - 2).Replace("\\u003C", "<");
            }
            // null, true, false, etc. don't have double quotes at the end and the start
            // Means we don't have to do anything with the result
            else
            {
                return temp;
            }
        }

        bool initialized = false;
        List<string> lastKnownData = new List<string>();

        private async void timer1_Tick(object sender, EventArgs e)
        {
            if (!initialized)
            {
                // These may look suspicious, but i promise they're not
                // They just retrieve data based on simple criteria I found in DevTools
                // I'm labeling them for readibility and transparency

                // Song's Title
                await ExecuteScript("function GetSongTitle()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__titleLink sc-truncate sc-text-h5 sc-link-primary\")[0].title;\r\n} ");
                await ExecuteScript("function Modal_GetSongTitle()\r\n{\r\nreturn document.getElementsByClassName(\"story__track__title g-type-shrinkwrap-inline g-type-shrinkwrap-large-primary\")[0].textContent;\r\n    \r\n}");

                // Artist's Name
                await ExecuteScript("function GetArtistName()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__lightLink sc-link-light sc-link-secondary sc-truncate sc-text-h5\")[0].title;\r\n} ");
                await ExecuteScript("function Modal_GetArtistName()\r\n{\r\nreturn document.getElementsByClassName(\"story__track__artist g-type-shrinkwrap-block theme-dark g-type-shrinkwrap-large-secondary\")[0].textContent\r\n    \r\n}");

                // Current Listen Time (f.e.: "1:20")
                await ExecuteScript("function GetCurrentTime()\r\n{\r\nreturn document.getElementsByClassName(\"playbackTimeline__timePassed sc-text-primary sc-text-h5\")[0].childNodes[1].textContent;\r\n}");

                // Song Duration (f.e.: "3:30")
                await ExecuteScript("function GetSongDuration()\r\n{\r\nreturn document.getElementsByClassName(\"playbackTimeline__duration sc-text-primary sc-text-h5\")[0].childNodes[1].textContent;\r\n}");

                // Song Link (unused)
                await ExecuteScript("function GetSongLink()\r\n{\r\nreturn document.getElementsByClassName(\"playbackSoundBadge__titleLink sc-truncate sc-text-h5 sc-link-primary\")[0].href;\r\n} ");

                // Album Cover Image URL
                await ExecuteScript("function GetAlbumCoverLink() \r\n{ \r\nlet a = document.getElementsByClassName(\"playbackSoundBadge__avatar sc-media-image sc-mr-2x\")[0].children[0].children[0].style.backgroundImage;\r\nif (a.startsWith(\"url\"))\r\n{\r\nreturn a.substring(5, a.length-2)\r\n}\r\nelse\r\n{\r\nreturn null\r\n}\r\n} ");
                await ExecuteScript("function Modal_GetAlbumCoverLink()\r\n{\r\nreturn document.getElementsByClassName(\"story__artwork__anchor\")[0].getElementsByClassName(\"artwork\")[0].src;\r\n    \r\n}");

                // Modal-only: "Action" text (f.e.: "reposted a playlist")
                await ExecuteScript("function Modal_GetAction()\r\n{\r\nreturn document.getElementsByClassName(\"storyItemInfo__text\")[0].textContent\r\n    \r\n}");

                // Modal-only: the second person, the "poster" or "reposter" (reposter's soundcloud name)
                await ExecuteScript("function Modal_GetPrimaryPersonsName()\r\n{\r\nreturn document.getElementsByClassName(\"storyItemViewerHeader__username sc-type-medium sc-text-body\")[0].children[0].text;\r\n    \r\n}");

                // Is the Song Playing? ("true" or "false")
                await ExecuteScript("function IsPlaying()\r\n{\r\nreturn document.getElementsByClassName(\"playControls__elements\")[0].getElementsByClassName(\"playControl sc-ir playControls__control playControls__play\")[0].classList.contains(\"playing\");\r\n}");

                // Is the Modal Player open? ("true" or "false")
                // enter modal view by clicking a button in the "artist shortcuts gallery" (on the right on the homepage)
                await ExecuteScript("function IsModal() \r\n{\r\n    return document.getElementsByClassName(\"modal__heading\").length != 0;\r\n}");

                // Does the custom sidepanel toggle button exist?
                await ExecuteScript("function CustomButtonExists()\r\n{\r\n    return document.getElementsByClassName(\"sc-rpc-client-btn-SIDEBAR-TOGGLE\").length > 0;\r\n}");

                // Checks if the functions work as intended, if not: we repeat this next second, and so on until it works
                initialized = await ExecuteScript("GetSongTitle()") != "null" && await ExecuteScript("GetArtistName()") != "null" && await ExecuteScript("GetSongLink()") != "null";

                // Means we can finally display the Discord rich presence
                if (initialized)
                {
                    if (!Debugger.IsAttached && File.Exists(GetFilePathFromCurrentDirectory("assets/sidebar.js")))
                    {
                        await ExecuteScript(LoadScriptFromFile("assets/sidebar.js"));
                    }
                    else
                    {
                        Directory.CreateDirectory("assets");
                        File.WriteAllText(GetFilePathFromCurrentDirectory("assets/sidebar.js"), Resources.sidebar);
                        await ExecuteScript(LoadScriptFromFile("assets/sidebar.js"));
                    }

                    // 5 requests / 20 seconds is what discord documentation states is the maximum. (1 request / 4 seconds)
                    // Yet we're going to be 2x slower to be extra sure
                    timer1.Interval = 8000;
                }

                return;
            }


            // shared (NORMAL and MODAL)
            bool isModal = await ExecuteScript("IsModal()") == "true";
            string songTitle;
            string artistName;
            string albumCoverLink;

            // NORMAL-only
            string currentTime = "";
            string songDuration = "";
            bool isPlaying = false;

            // MODAL-only
            string action = "";
            string primaryPersonName = "";

            // there are 2 listening views
            // 1st is the normal view, 2nd is a 'modal' view (when you click the instagram-story-like buttons)
            if (isModal)
            {
                // Execute javascript in WebView2 to gather needed information
                songTitle = await ExecuteScript("Modal_GetSongTitle()");
                artistName = await ExecuteScript("Modal_GetArtistName()");
                albumCoverLink = await ExecuteScript("Modal_GetAlbumCoverLink()");

                primaryPersonName = await ExecuteScript("Modal_GetPrimaryPersonsName()");
                action = await ExecuteScript("Modal_GetAction()");
            }
            else
            {
                songTitle = await ExecuteScript("GetSongTitle()");
                artistName = await ExecuteScript("GetArtistName()");
                albumCoverLink = await ExecuteScript("GetAlbumCoverLink()");

                currentTime = await ExecuteScript("GetCurrentTime()");
                songDuration = await ExecuteScript("GetSongDuration()");

                isPlaying = await ExecuteScript("IsPlaying()") == "true";
            }

            // Inlined method to not repeat ourselves later
            void UpdateStatus()
            {
                if (isModal)
                {
                    rpcManager.ListenToModal(songTitle, artistName, albumCoverLink, action, primaryPersonName);
                }
                else
                {
                    if (isPlaying)
                    {
                        rpcManager.ListenToSong(songTitle, artistName, currentTime, songDuration, albumCoverLink);
                    }
                    else
                    {
                        rpcManager.Idle(songTitle, artistName, albumCoverLink);
                    }
                }
            }

            // Means it's the first time this code is being run
            if (lastKnownData.Count == 0)
            {
                lastKnownData = new List<string> { songTitle, artistName, currentTime, songDuration, isPlaying.ToString() };
                UpdateStatus();
            }
            // We have data, so we compare it to our old one
            // If any of the data is NOT the same, we update the Discord rich presence
            else
            {
                List<string> currentData = new List<string> { songTitle, artistName, currentTime, songDuration, isPlaying.ToString() };

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

        private string LoadScriptFromFile(string v)
        {
            return File.ReadAllText(Environment.CurrentDirectory + "\\" + v);
        }

        private string GetFilePathFromCurrentDirectory(string v)
        {
            return Environment.CurrentDirectory + "\\" + v;
        }

        private async void timer2_Tick(object sender, EventArgs e)
        {
            string customBtnResult = await ExecuteScript("CustomButtonExists()");
            if (customBtnResult == "null")
            {
                initialized = false;
                timer1_Tick(sender, e);
            }
            else if (customBtnResult == "false")
            {
                await ExecuteScript("CreateCustomButton()");
            }
        }
    }
}
