using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using soundcloud_rpc.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

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

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            // Get the request object
            var request = e.Request;

            // Get the URI of the request
            string uri = request.Uri;

            // Optionally, read the request's content
            // The request's body can be accessed if the method is POST (or others)
            if (request.Method == "POST")
            {
                var content = new System.IO.StreamReader(request.Content).ReadToEnd();
                MessageBox.Show($"POST Request to {uri} with content: {content}");
            }
            else
            {
                MessageBox.Show($"Request to {uri}");
            }
        }

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
                    // when running with debugger, sidebar is always going to be updated in /bin/
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

            if (await ExecuteScript("should == true") == "true")
            {
                await ExecuteScript($"should = false;\nSetData({trackId}, {clientId})");
                
                UpdateSidepanelDescriptionContent();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeFetchXHRIntercept();
        }

        private async void InitializeFetchXHRIntercept()
        {
            await webView21.EnsureCoreWebView2Async();

            // javascript intercepts Fetch and XHR requests (check WebMessageReceived)
            string script = @"
        // Intercept fetch requests
        const originalFetch = fetch;
        window.fetch = function(url, options) {
            window.chrome.webview.postMessage({type: 'fetch', url: url, method: options?.method, body: options?.body});
            return originalFetch(url, options);
        };

        // Intercept XMLHttpRequest requests
        const originalXHR = XMLHttpRequest;
        XMLHttpRequest = function() {
            const xhr = new originalXHR();
            const originalOpen = xhr.open;
            xhr.open = function(method, url, async, user, password) {
                xhr.addEventListener('readystatechange', function() {
                    if (xhr.readyState === 4) {
                        window.chrome.webview.postMessage({type: 'xhr', url: url, method: method, response: xhr.responseText});
                    }
                });
                originalOpen.apply(xhr, arguments);
            };
            return xhr;
        };
    ";

            await webView21.CoreWebView2.ExecuteScriptAsync(script);

            // Handle messages sent from the JavaScript
            webView21.CoreWebView2.WebMessageReceived += WebMessageReceived;
        }

        string trackDescription = "";
        string clientId = "";
        string trackId = "";

        private async void WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.WebMessageAsJson;

            // parse the message (intercepted, check InitializeFetchXHRIntercept)
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(message);
            if (data.type == "xhr")
            {
                // The track id can be found in XHR requests
                string url = data.url;
                string method = data.method;
                string response = data.response;

                if (method == "GET")
                {
                    if (url.StartsWith("https://api-v2.soundcloud.com/me"))
                    {
                        await ExecuteScript("console.log('(C#) intercepted XHR GET api-v2 /me* request');");

                        // find the track_id using regex
                        // if cannot find in response, try url
                        async Task<bool> matchFromString(string inputString)
                        {
                            var trackIdMatch = System.Text.RegularExpressions.Regex.Match(inputString, "https://api-v2.soundcloud.com/media/soundcloud:tracks:(\\d+)");

                            if (trackIdMatch.Success)
                            {
                                trackId = trackIdMatch.Groups[1].Value;
                                await ExecuteScript($"console.log('(C#) NEW TRACKID {trackId}');\ntrackId = '{trackId}';");

                                if (trackId != trackIdMatch.Groups[1].Value)
                                {
                                    await ExecuteScript("console.log('(C#) told to update description');");
                                    UpdateSidepanelDescriptionContent();
                                }
                                else
                                {
                                    await ExecuteScript("console.log('(C#) trackId is the same as last one');");
                                }

                                return false;
                            }
                            else
                            {
                                await ExecuteScript("console.log('(C#) no trackId found in the request.');");
                                return true;
                            }
                        }

                        if (await matchFromString(response))
                        {
                            await matchFromString(url);
                        }
                    }
                }

                var clientIdMatch = System.Text.RegularExpressions.Regex.Match(url, "client_id=(\\w+)&");
                if (clientIdMatch.Success)
                {
                    clientId = clientIdMatch.Groups[1].Value;
                }
            }
        }

        private async void UpdateSidepanelDescriptionContent()
        {
            if (trackId == "" || clientId == "")
            {
                await ExecuteScript("console.log('(C#) trackId empty || clientId empty');");
                await Task.Delay(500);
                UpdateSidepanelDescriptionContent();
                return;
            }

            await ExecuteScript($"SetData('{trackId}', '{clientId}')");

            await ExecuteScript("updateSidepanelDescriptionContent();");

            return;

            // Construct the URI for the track
            // HERE WE GET THE DESCRIPTION BY MAKING A REQUEST TO API-V2
            // THIS IS NOT SUPPORTED BY SOUNDCLOUD, undocumented
            string trackUri = $"https://api-v2.soundcloud.com/tracks/{trackId}?user_id=1&client_id={clientId}";

            // JSON response
            string newConstructedUriResponse = await SendRequestGET(trackUri);

            if (Debugger.IsAttached)
            {
                File.WriteAllText("trackUriResponse.txt", newConstructedUriResponse);
                //MessageBox.Show(newConstructedUriResponse);
            }

            var trackDescriptionMatch = System.Text.RegularExpressions.Regex.Match(newConstructedUriResponse, "\"description\":\"(.*?)\"");
            if (trackDescriptionMatch.Success)
            {
                trackDescription = trackDescriptionMatch.Groups[1].Value;
            }
            else
            {
                trackDescription = "";
            }

            async void UpdateDescription()
            {
                if (await ExecuteScript($"UpdateDescriptionContent(`{trackDescription}`);") == "updated")
                {
                    await ExecuteScript("console.log('(C#) UPDATED description.');");
                }
                else
                {
                    await ExecuteScript("console.log('(C#) FAILED to Update description, retrying..');");
                    await Task.Delay(1000);
                    UpdateDescription();
                }
            }

            UpdateDescription();
        }

        private async Task<string> SendRequestGET(string uri)
        {
            await ExecuteScript($"console.log('(C#) sending request to {uri}\nin hopes of getting a description!');");
            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                // Optionally, you can add headers if required
                client.Headers.Add("User-Agent", "Mozilla/5.0");

                // Download the response asynchronously
                string result;
                try
                {
                    result = await client.DownloadStringTaskAsync(uri);
                }
                catch
                {
                    result = "";
                }

                return result;
            }
        }
    }
}
