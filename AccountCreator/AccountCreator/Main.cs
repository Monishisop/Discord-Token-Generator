using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AccountCreator.Extensions;
using System.Text.RegularExpressions;
using Discord;
using CapMonsterCloud;
using CapMonsterCloud.Models.CaptchaTasks;
using CapMonsterCloud.Exceptions;
using CapMonsterCloud.Models.CaptchaTasksResults;

namespace AccountCreator
{
    public partial class Main : Form
    {
        WMPLib.WindowsMediaPlayer a;
        private ToolStripStatusLabel Status;
        private ListBox Logs;
        private int Index;
        string currentPath = Directory.GetCurrentDirectory();
        public Main()
        {
            InitializeComponent();
        }
        private void playmp3(string path)
        {
            a = new WMPLib.WindowsMediaPlayer();
            a.URL = path;
            a.controls.play();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            playmp3(AppDomain.CurrentDomain.BaseDirectory + "\\Music.mp3");
            if (!Directory.Exists(Path.Combine(currentPath, "Accounts")))
                Directory.CreateDirectory(Path.Combine(currentPath, "Accounts"));
        }
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        static void WriteAcc(string account)
        {
            StreamWriter fileauth = new StreamWriter("Accounts/Tokens.txt", true);
            fileauth.WriteLine(account);
            fileauth.Close();
        }
        static void WriteMailDebug(string account)
        {
            StreamWriter fileauth = new StreamWriter("Logs/Debug.txt", true);
            fileauth.WriteLine(account);
            fileauth.Close();
        }
        private async void Start(int ThreadsAmount, int Delay, bool VerifyEmail)
        {
            await Task.Run(() =>
            {
                Status.SafeChangeText(string.Format("Creating Accounts"));

                List<Task> Threads = new List<Task>();

                for (int i = 0; i < ThreadsAmount; i++)
                {
                    Threads.Add(StartThread(Delay, VerifyEmail));
                }

                Task.WaitAll(Threads.ToArray());

                Status.SafeChangeText("Completed Creation");
            });
        }
        private async Task StartThread(int Delay, bool VerifyEmail)
        {
            await Task.Run(async () =>
            {

                Retry:
                try
                {
                    DiscordClient client = new DiscordClient();
                    var capclient = new CapMonsterClient("YOUR_CLIENT_KEY");
                    var email = "";
                    using (TMailLibrary.TMail.Models.Account TMailClient = new TMailLibrary.TMail.Models.Account("https://gmail.ax/"))
                    {
                            email = await TMailClient.GetEmailAdress();
                            try
                            {
                                // Creating a NoCaptchaTaskProxyless task object
                                var captchaTask = new NoCaptchaTaskProxyless
                                {
                                    WebsiteUrl = "https://discord.com/",
                                    WebsiteKey = "6Lef5iQTAAAAAKeIvIY-DeexoO3gj7ryl9rLMEnn",
                                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.146 Safari/537.36"
                                };
                                int taskId = await capclient.CreateTaskAsync(captchaTask);
                                // Get the task result
                                var solution = await capclient.GetTaskResultAsync<NoCaptchaTaskProxylessResult>(taskId);
                                // Recaptcha response to be used in the form
                                var recaptchaResponse = solution.GRecaptchaResponse;
                                client.RegisterAccount(new DiscordRegistration { Username = RandomString(12), Password = RandomString(12), DateOfBirth = "1994-01-01", Email = email, CaptchaKey = recaptchaResponse });
                            }
                            catch (CapMonsterException e)
                            {
                                goto Retry;
                            }

                        if (VerifyEmail)
                            await TMailVerify(TMailClient);
                    }

                    Logs.SafeAddItem(string.Format("Registered: {0}", client.User.Username));
                    WriteAcc(client.Token);
                    await Task.Delay(Delay);
                }
                catch (Exception ex)
                {
                    Logs.SafeAddItem(string.Format("Error Message: {0}", ex.Message));
                    await Task.Delay(Delay);
                    goto Retry;
                    //debug stuff
                    //Logs.SafeAddItem(string.Format("Error Message: {0}", ex.Message));
                    //await Task.Delay(Delay);
                }
            });
        }

        private async Task TMailVerify(TMailLibrary.TMail.Models.Account TMailClient)
        {
            int tries = 0;
            bool isverfied = false;
            while (!isverfied)
            {
                List<TMailLibrary.TMail.Models.Mail> mb = await TMailClient.GetMailBox();

                foreach (TMailLibrary.TMail.Models.Mail mail in mb)
                {
                    if (mail.sender_email == "UNFINISHED DONT USE YET")
                    {
                        WriteMailDebug(mail.html);
                        Match rgx = Regex.Match(mail.html, "UNFINISHED DONT USE YET", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        if (!rgx.Success)
                            throw new Exception("Failed to get Discord Confirm Email URL with Regex");
                        string URL = rgx.Groups["value"].Value;
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            URL.Replace("\"", "");
                            string response = await wc.DownloadStringTaskAsync(URL);
                            if (!response.Contains("Email address successfully verified"))
                                throw new Exception("Failed to verify Email, response at discord.com dont contain 'Email address successfully verified'");
                        }
                        isverfied = true;
                        break;
                    }
                }
                
                tries += 1;
                if (tries >= 120)
                    throw new Exception("No Email found in TMail after 10 minutes");

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (FormStart FormStart = new FormStart())
            {
                FormStart.ShowDialog();
                if (FormStart.Start)
                {
                    Status = toolStripStatusLabel1;
                    Logs = listBox1;
                    Index = 0;

                    Start(FormStart.ThreadsAmount, FormStart.Delay, FormStart.VerifyEmail);
                }
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
