using Leaf.xNet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZEE5_Live_TV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string languages = "ta,kn,pa,bn,en,ml,mr,gu,te,hi";
        private ArrayList channelList = new ArrayList();

        private string getLiveToken()
        {
            HttpRequest tokenRequest = new HttpRequest();
            string response = tokenRequest.Get("https://useraction.zee5.com/token/live.php").ToString();
            string token = JObject.Parse(response).SelectToken("video_token").ToString();
            return token;
        }
        private string getToken()
        {
            HttpRequest tokenRequest = new HttpRequest();
            string response = tokenRequest.Get("https://useraction.zee5.com/token/platform_tokens.php?platform_name=web_app").ToString();
            string token = JObject.Parse(response).SelectToken("token").ToString();
            return token;
        }

        private void getChannels()
        {
            int i = 0;
            string url = "https://catalogapi.zee5.com/v1/channel/bygenre?sort_by_field=channel_number&sort_order=ASC&genres=FREE%20Channels,Hindi%20Entertainment,Hindi%20Movies,English%20Entertainment,Entertainment,Movie,News,Hindi%20News,English%20News,Marathi,Tamil,Telugu,Bengali,Malayalam,Kannada,Punjabi,Kids,Gujarati,Odiya,Music,Lifestyle,Devotional,Comedy,Drama,Sports,Infotainment&country=IN&translation=en&languages="+languages;
            HttpRequest request = new HttpRequest();
            string response = request.Get(url).ToString();
            var token = JObject.Parse(response);

            JArray itemsArray = (JArray)token.SelectToken("items");
            ImageList dowloadedImages = new ImageList();
            dowloadedImages.ImageSize = new Size(150, 150);
            dowloadedImages.ColorDepth = ColorDepth.Depth32Bit;

            foreach (var items in itemsArray)
            {
                foreach (var channel in items.SelectToken("items"))
                {

                    listView_Channels.Items.Add(channel.SelectToken("title").ToString(), i);
                    WebClient w = new WebClient();
                    string imageTempUrl = "https://akamaividz2.zee5.com/image/upload/w_386,h_386,c_scale/resources/" + channel.SelectToken("id").ToString() + "/channel_web/" + channel.SelectToken("list_image").ToString();
                    channelList.Add(channel.SelectToken("id").ToString());
                    byte[] imageBytes = w.DownloadData(imageTempUrl);
                    MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    memoryStream.Write(imageBytes, 0, imageBytes.Length);
                    Image imgs = Image.FromStream(memoryStream, true);
                    dowloadedImages.Images.Add(imgs);

                    i++;

                }
                listView_Channels.LargeImageList = dowloadedImages;
            }
        }

        private string getStream(string ChannelID)
        {
            HttpRequest streamRequest = new HttpRequest();
            streamRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.72 Safari/537.36";
            try
            {
                string response = streamRequest.Get("https://catalogapi.zee5.com/v1/channel/" + ChannelID + "?translation=en&country=IN").ToString();

                if (JObject.Parse(response).SelectToken("stream_url_hls") != null)
                {
                    string streamUrl = JObject.Parse(response).SelectToken("stream_url_hls").ToString() + getLiveToken();
                    return streamUrl;
                }
                else
                {

                    return "NO-STREAM";
                }
            }
            catch
            {
                return "NO-STREAM";
            }
            


        }

        private void playUrl(String videoLink)
        {
            try
            {

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"C:\\Program Files\\VideoLAN\\VLC\\vlc.exe ";

                startInfo.Arguments = videoLink;
                process.StartInfo = startInfo;
                process.Start();
            }
            catch
            {

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = Environment.CurrentDirectory + @"\\Default Player\\mpv.exe";
                startInfo.Arguments = videoLink;
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getChannels();
        }

        private void listView_Channels_MouseClick(object sender, MouseEventArgs e)
        {
            if (getStream(channelList[listView_Channels.FocusedItem.Index].ToString()) != "NO-STREAM")
            {
                playUrl(getStream(channelList[listView_Channels.FocusedItem.Index].ToString()));
            }
            else
            {
                MessageBox.Show("No Stream available", "Stream Not Available");
            }
        }

        private void githubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/henry-richard7");
        }

        private void paypalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.paypal.com/paypalme/henryrics");
        }
    }
}
