using System;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using Fish_Raper;

namespace Webhook_Spammer
{
    public partial class Form1 : Form
    {
        public HttpClient httpClient = new HttpClient();
        bool Spamming = false;
        int Requests;
        int TotalRequests;
        string CurrentFish = "null";
        string CurrentUncles = "null";
        string NextUncle = "null";
        string CurrentRareFish = "null";
        string NextRareFish = "null";

        bool firstUncle = true;
        bool firstRareFish = true;
        long previousUncleTime = 0;
        long previousRareFishTime = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                immutableLoginKeyTextbox.UseSystemPasswordChar = false;
            }
            else if (checkBox1.Checked == false)
            {
                immutableLoginKeyTextbox.UseSystemPasswordChar = true;
            }
        }

        public void StartBot()
        {
            Spamming = true;
            button1.Invoke((Action)delegate
            {
                button1.Text = "Stop";
                button1.ForeColor = Color.FromArgb(255, 0, 0);
                SpamWebhook();
                UpdateStatsTask();
                OnlineTask();
                LoginCheckTask();
            });
            
        }

        public void StopBot()
        {
            Spamming = false;
            button1.Text = "Start";
            button1.ForeColor = Color.FromArgb(0, 255, 0);
            Requests = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (immutableUsernameTextBox.Text != "" && immutableLoginKeyTextbox.Text != "")
            {
                if (Spamming == false)
                {
                    StartBot();
                }
                else if (Spamming == true)
                {
                    StopBot();
                }
            }
            else
            {
                StopBot();
            }
        }

        private void RateLimited(bool Value)
        {
            if (Value == true)
            {
                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { label8.Text = "You are being rate limited!"; });
            }
            else if (Value == false)
            {
                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate () { label8.Text = ""; });
            }
        }

        private void Post(string Url, HttpContent Pairs)
        {
            Thread WebhookSendThread = new Thread(async () =>
            {
                try
                {
                    var resp = await httpClient.PostAsync(Url, Pairs);
                    RateLimited(false);
                }
                catch
                {
                    try
                    {
                        RateLimited(true);
                        var resp = await httpClient.PostAsync(Url, Pairs);
                    }
                    catch
                    {
                        //lol
                    }
                }
            });

            WebhookSendThread.Start();
        }

        private void PostWebhook(string Url, string ubaname, string bobinbey)
        {

            string data = JsonConvert.SerializeObject(new { username = ubaname, loginKey = bobinbey });

            Post(Url, new StringContent(data, System.Text.Encoding.UTF8, "application/json"));
        }

        private async Task<string> SendKeyedRequest(string Url, bool capitalized)
        {
            string data = "";
            if (capitalized)
            {
                data = JsonConvert.SerializeObject(new { username = immutableUsernameTextBox.Text, loginKey = immutableLoginKeyTextbox.Text });
            }
            else
            {
                data = JsonConvert.SerializeObject(new { username = immutableUsernameTextBox.Text, loginkey = immutableLoginKeyTextbox.Text });
            }

            HttpResponseMessage resp = null;

            try
            {
                resp = await httpClient.PostAsync(Url, new StringContent(data, System.Text.Encoding.UTF8, "application/json"));
                RateLimited(false);
            }
            catch
            {
                try
                {
                    RateLimited(true);
                    resp = await httpClient.PostAsync(Url, new StringContent(data, System.Text.Encoding.UTF8, "application/json"));
                }
                catch
                {
                    //lol
                }
            }

            if (resp == null)
            {
                return "null";
            }
            else
            {
                return resp.Content.ReadAsStringAsync().Result;
            }
        }

        private async void SpamWebhook()
        {
            while (true)
            {
                if (Spamming == true)
                {
                    try
                    {
                        for (int i = 0; i < Convert.ToInt32(textBox2.Text); i++)
                        {
                            PostWebhook("https://traoxfish.us-3.evennode.com/fish", immutableUsernameTextBox.Text, immutableLoginKeyTextbox.Text);
                            Requests = Requests + 1;
                            TotalRequests = TotalRequests + 1;
                            label9.Text = Convert.ToString(TotalRequests);
                            label5.Text = Convert.ToString(Requests);
                            fishLabel.Text = CurrentFish;
                            uncleLabel.Text = CurrentUncles;
                            nextUncleLabel.Text = NextUncle;
                            rareFishLabel.Text = CurrentRareFish;
                            nextRareFishLabel.Text = NextRareFish;
                        }
                    }
                    catch
                    {

                    }
                }
                else if (Spamming == false)
                {
                    try
                    {
                        Requests = 0;
                        label5.Text = Convert.ToString(Requests);
                        fishLabel.Text = CurrentFish;
                        uncleLabel.Text = CurrentUncles;
                        nextUncleLabel.Text = NextUncle;
                        rareFishLabel.Text = CurrentRareFish;
                        nextRareFishLabel.Text = NextRareFish;
                    }
                    catch
                    {

                    }
                    break;
                }

                await Task.Delay((int)Convert.ToDouble(fishRateTextbox.Text));
            }
        }

        private async void UpdateStatsTask()
        {
            while (true)
            {
                if (Spamming == true)
                {
                    try
                    {
                        //logTextBox.AppendText("Polled stats" + Environment.NewLine);
                        dynamic obj = JsonConvert.DeserializeObject<dynamic>(await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/getfish", false)));
                        dynamic obj1 = JsonConvert.DeserializeObject<dynamic>(await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/getuncles", false)));
                        dynamic obj2 = JsonConvert.DeserializeObject<dynamic>(await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/getrarefish", false)));
                        string rarefishcostreq = "{\"cost\":\"99999999999\"}";
                        using (HttpClient client = new HttpClient())
                        {
                            rarefishcostreq = await client.GetStringAsync("https://traoxfish.us-3.evennode.com/getrarefishcost");
                        }
                        dynamic obj3 = JsonConvert.DeserializeObject<dynamic>(rarefishcostreq);

                        CurrentFish = obj.fish;
                        CurrentUncles = obj1.uncles;
                        NextUncle = obj1.nextuncle;
                        CurrentRareFish = obj2.rarefish;
                        NextRareFish = obj3.cost;
                    }
                    catch
                    {
                        requestLogTextBox.AppendText("Hit exception while updating stats! Waiting 1m to start again..." + Environment.NewLine);
                        StopBot();
                        Task.Delay(60000).ContinueWith(t => StartBot());
                        requestLogTextBox.AppendText("Bot resumed" + Environment.NewLine);
                    }

                    try
                    {
                        if (Convert.ToInt32(NextRareFish) > 50 && Convert.ToInt32(NextRareFish) < Convert.ToInt32(CurrentFish) && rareFishBuyCheckBox.Checked)
                        {
                            for (int i = 0; i < (Convert.ToInt32(CurrentFish) / Convert.ToInt32(NextRareFish)); i++)
                            {
                                dynamic buyobj = JsonConvert.DeserializeObject<dynamic>(await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/buyrarefish", false)));
                                CurrentRareFish = buyobj.rarefish;
                                logTextBox.AppendText("Purchased rare fish for " + NextRareFish + "  |  Current rare fish: " + CurrentRareFish);
                                DateTime foo = DateTime.Now;
                                long currentTime = ((DateTimeOffset)foo).ToUnixTimeMilliseconds();
                                long timeSinceLast;

                                if (firstRareFish)
                                {
                                    previousRareFishTime = currentTime;
                                    firstRareFish = false;
                                }
                                else
                                {
                                    timeSinceLast = currentTime - previousRareFishTime;
                                    TimeSpan ts = TimeSpan.FromMilliseconds(timeSinceLast);
                                    var parts = string
                                        .Format("{0:D2}m:{1:D2}s:{2:D3}ms",
                                            ts.Minutes, ts.Seconds, ts.Milliseconds)
                                        .Split(':')
                                        .SkipWhile(s => Regex.Match(s, @"00\w").Success) // skip zero-valued components
                                        .ToArray();
                                    var formattedTime = string.Join(" ", parts);
                                    logTextBox.AppendText("  |  Time since last: " + formattedTime);
                                    previousRareFishTime = currentTime;
                                }

                                logTextBox.AppendText(Environment.NewLine);
                            }
                        }
                        else if (Convert.ToInt32(NextUncle) > 50 && Convert.ToInt32(NextUncle) < Convert.ToInt32(CurrentFish) && uncleBuyCheckBox.Checked)
                        {
                            for (int i = 0; i < (Convert.ToInt32(CurrentFish) / Convert.ToInt32(NextUncle)); i++)
                            {
                                dynamic buyobj = JsonConvert.DeserializeObject<dynamic>(await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/buyuncle", false)));
                                CurrentUncles = buyobj.uncles;
                                logTextBox.AppendText("Purchased uncle for " + NextUncle + "  |  Current uncles: " + CurrentUncles);

                                DateTime foo = DateTime.Now;
                                long currentTime = ((DateTimeOffset)foo).ToUnixTimeMilliseconds();
                                long timeSinceLast;

                                if (firstUncle)
                                {
                                    previousUncleTime = currentTime;
                                    firstUncle = false;
                                }
                                else
                                {
                                    timeSinceLast = currentTime - previousUncleTime;
                                    TimeSpan ts = TimeSpan.FromMilliseconds(timeSinceLast);
                                    var parts = string
                                        .Format("{0:D2}m:{1:D2}s:{2:D3}ms",
                                            ts.Minutes, ts.Seconds, ts.Milliseconds)
                                        .Split(':')
                                        .SkipWhile(s => Regex.Match(s, @"00\w").Success) // skip zero-valued components
                                        .ToArray();
                                    var formattedTime = string.Join(" ", parts);
                                    logTextBox.AppendText("  |  Time since last: " + formattedTime);
                                    previousUncleTime = currentTime;
                                }

                                logTextBox.AppendText(Environment.NewLine);
                            }
                        }


                    }
                    catch
                    {

                    }
                }
                else if (Spamming == false)
                {
                    break;
                }

                await Task.Delay((int)Convert.ToDouble(pollingRateTextbox.Text));
            }
        }

        //Required to post to /online endpoint every 10 secs or else fish wont count
        private async void OnlineTask()
        {
            while (true)
            {
                if (Spamming == true)
                {
                    try
                    {
                        await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/online", false));
                        if (requestLogTextBox.Text.Split('\n').Length > 20)
                        {
                            requestLogTextBox.Text = "";
                        }
                        requestLogTextBox.AppendText("Called /online" + Environment.NewLine);
                    }
                    catch
                    {

                    }
                }
                else if (Spamming == false)
                {
                    break;
                }

                await Task.Delay((int)Convert.ToDouble(8000));
            }
        }

        private async void LoginCheckTask()
        {
            while (true)
            {
                if (Spamming == true)
                {
                    try
                    {
                        var resp = await Task.Run(() => SendKeyedRequest("https://traoxfish.us-3.evennode.com/checkkey", false));
                        requestLogTextBox.AppendText("Called /checkkey" + Environment.NewLine);
                        if (!resp.Contains("true"))
                        {
                            logTextBox.AppendText("Login key invalid; reauthenticating..." + Environment.NewLine);
                            LoginTask();
                        }
                    }
                    catch
                    {

                    }
                }
                else if (Spamming == false)
                {
                    break;
                }

                await Task.Delay((int)Convert.ToDouble(loginRateTextbox.Text));
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Spamming = false;
            Requests = 0;
            label5.Text = Convert.ToString(Requests);
            button1.Text = "Start";
            button1.ForeColor = Color.FromArgb(0, 255, 0);
            //textBox1.Text = "https://traoxfish.us-3.evennode.com/fish";
            checkBox1.Checked = false;
            uncleBuyCheckBox.Checked = false;
            rareFishBuyCheckBox.Checked = false;
            fishRateTextbox.Text = "200";
            textBox2.Text = "1";
            //immutableUsernameTextBox.Text = "";
            pollingRateTextbox.Text = "2000";
            //immutableLoginKeyTextbox.Text = "";
            loginRateTextbox.Text = "1000";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //FishUpdater.LeaderboardUpdater();
            leaderboardListBox.DrawMode = DrawMode.OwnerDrawFixed;
            //leaderboardListBox.DrawItem += leaderboardListBox_DrawItem;
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private async void LoginTask()
        {
            logTextBox.AppendText("Attempting to login..." + Environment.NewLine);
            var resp = await httpClient.PostAsync("https://traoxfish.us-3.evennode.com/login", new StringContent(JsonConvert.SerializeObject(new { password = passwordLoginTextbox.Text, username = usernameLoginTextbox.Text }), System.Text.Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            dynamic obj = JsonConvert.DeserializeObject<dynamic>(resp.Content.ReadAsStringAsync().Result);
            try
            {
                immutableUsernameTextBox.Text = usernameLoginTextbox.Text;
                immutableLoginKeyTextbox.Text = obj.key;
                logTextBox.AppendText("Logged in with username " + immutableUsernameTextBox.Text + " and key " + immutableLoginKeyTextbox.Text.Substring(0, immutableLoginKeyTextbox.Text.Length - 12) + "************" + Environment.NewLine);
                loginSatusLabel.Text = "Logged in!";
                loginSatusLabel.ForeColor = Color.Green;
            }
            catch
            {
                logTextBox.AppendText("Login failed" + Environment.NewLine);
                MessageBox.Show("Invalid login or internal exception");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoginTask();
        }

        public static string FormatNumber(decimal num)
        {
            if (num > 999999999 || num < -999999999)
            {
                return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
            }
            else
            if (num > 999999 || num < -999999)
            {
                return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
            }
            else
            if (num > 999 || num < -999)
            {
                return num.ToString("0,.#K", CultureInfo.InvariantCulture);
            }
            else
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }
        }

        private async void LeaderboardTask()
        {
            string fishLB = "{\"1\":\"null\"}";
            using (HttpClient client = new HttpClient())
            {
                fishLB = await client.GetStringAsync("https://traoxfish.us-3.evennode.com/leaderboards");
            }
            dynamic fishOBJ = JsonConvert.DeserializeObject<dynamic>(fishLB);

            string rarefishLB = "{\"1\":\"null\"}";
            using (HttpClient client = new HttpClient())
            {
                rarefishLB = await client.GetStringAsync("https://traoxfish.us-3.evennode.com/leaderboards/rarefish");
            }
            dynamic rarefishOBJ = JsonConvert.DeserializeObject<dynamic>(rarefishLB);

            string uncleLB = "{\"1\":\"null\"}";
            using (HttpClient client = new HttpClient())
            {
                uncleLB = await client.GetStringAsync("https://traoxfish.us-3.evennode.com/leaderboards/uncles");
            }
            dynamic uncleOBJ = JsonConvert.DeserializeObject<dynamic>(uncleLB);

            int i = 1;
            leaderboardListBox.Items.Clear();
            foreach (string ranking in fishOBJ)
            {
                var fish = ranking.Substring(ranking.IndexOf(" - ") + 3, ranking.Length - (ranking.IndexOf(" - ") + 3));
                var player = ranking.Substring(0, ranking.IndexOf(" "));
                ///var rarefish = string.IsNullOrEmpty((string)rarefishOBJ[Convert.ToString(i)].Substring(ranking.IndexOf(" - ") + 3, ranking.Length - 1 - (ranking.IndexOf(" - ") + 3)));
                //var uncles = string.IsNullOrEmpty((string)uncleOBJ[Convert.ToString(i)].Substring(ranking.IndexOf(" - ") + 3, ranking.Length - 1 - (ranking.IndexOf(" - ") + 3)));
                //leaderboardListBox.Items.Add(i + ". " + ranking.Substring(0, ranking.IndexOf(" ")) + "  |  Fish: " + (shorthandCheckBox.Checked ? FormatNumber(Convert.ToDouble(fish)) : fish) + "  |  Rare Fish: " + rarefish + "  |  Uncles: " + uncles);
                leaderboardListBox.Items.Add(i + ". " + player + "  |  Fish: " + (shorthandCheckBox.Checked ? FormatNumber(Convert.ToDecimal(fish.Substring(0, fish.Length - 1))) + (player.Equals(immutableUsernameTextBox.Text) ? "p" : fish.Substring(fish.Length - 1)) : Convert.ToDecimal(fish.Substring(0, fish.Length - 1)).ToString("N0") + (player.Equals(immutableUsernameTextBox.Text) ? "p" : fish.Substring(fish.Length - 1))));
                i++;
            }
            requestLogTextBox.AppendText("Updated leaderboards" + Environment.NewLine);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LeaderboardTask();
        }

        private void leaderboardListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            Graphics g = e.Graphics;
            g.FillRectangle(new SolidBrush(Color.White), e.Bounds);
            ListBox lb = (ListBox)sender;
            string item = lb.Items[e.Index].ToString();
            string discrim = item.Substring(item.Length - 1);
            var color = new SolidBrush(Color.Black);
            if (discrim.Equals("p"))
            {
                color = new SolidBrush(Color.Red);
            }
            else if (discrim.Equals("y"))
            {
                color = new SolidBrush(Color.Green);
            }
            g.DrawString(item.Substring(0, item.Length - 1), e.Font, color, new PointF(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }

        private void shorthandCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            LeaderboardTask();
        }
    }
}