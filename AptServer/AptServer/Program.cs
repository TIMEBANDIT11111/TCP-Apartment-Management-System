using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;

class CinsAptServer : Form
{
    private static string weather = "";
    private static string teslastock = "";
    private static string oldweather = "";
    private static string oldteslastock = "";
    private static ListBox results;
    public static Label label1 = new Label();
    public static Label label2 = new Label();
    private static List<string> messages = new List<string>();

    public CinsAptServer()
    {
        Text = "CinsAptServer";
        Size = new Size(350, 580);
        BackColor = Color.LightBlue;

        label1.Parent = this;
        label1.Text = "Manisa: " + weather + "°C";
        label1.AutoSize = true;
        label1.Location = new Point(75, 5);

        label2.Parent = this;
        label2.Text = "Tesla: " + teslastock;
        label2.AutoSize = true;
        label2.Location = new Point(200, 5);

        results = new ListBox();
        results.Parent = this;
        results.Location = new Point(10, 85);
        results.Size = new Size(315, 25 * Font.Height);

        WeatherInformation wth = new WeatherInformation();
        Thread temperature = new Thread(new ThreadStart(wth.WeatherUpdater));
        temperature.Start();

        ThreadedTcpSrvr obj = new ThreadedTcpSrvr();
        Thread t = new Thread(new ThreadStart(obj.ThreadedTcpServer));
        t.Start();
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }

    class ThreadedTcpSrvr
    {
        private TcpListener client;

        public void ThreadedTcpServer()
        {
            client = new TcpListener(9050);
            client.Start();

            results.Items.Add("CinsAptServer is active, waiting for clients");
            while (true)
            {
                while (!client.Pending())
                {
                    Thread.Sleep(1000);
                }
                ConnectionThread newconnection = new ConnectionThread();
                newconnection.threadListener = this.client;
                Thread newthread = new Thread(new
                          ThreadStart(newconnection.HandleConnection));
                newthread.Start();
            }
        }
    }

    class MsgReceiver
    {
        public NetworkStream ns;
        public void MessageHandler()
        {

            try
            {
                byte[] data = new byte[1024];
                int recv;
                string welcome = "Welcome to the CinsApt";
                data = Encoding.ASCII.GetBytes(welcome);
                ns.Write(data, 0, data.Length);
                while (true)
                {
                    data = new byte[1024];
                    recv = ns.Read(data, 0, data.Length);
                    if (recv != 0)
                    {
                        messages.Add(System.Text.Encoding.UTF8.GetString(data));
                        results.Items.Add(System.Text.Encoding.UTF8.GetString(data));

                    }
                    else
                    {
                        break;
                    }
                }
            }catch(Exception e)
            {
            }
        }
    }

    class ConnectionThread
    {
        public TcpListener threadListener;
        private static int connections = 0;
        public static string username = " ";
        public async void HandleConnection()
        {
            int idx = 0;
            int recv;
            byte[] data = new byte[1024];
            byte[] msg = new byte[1024];
                TcpClient client = threadListener.AcceptTcpClient();
                NetworkStream ns = client.GetStream();

                MsgReceiver clnt = new MsgReceiver();
                clnt.ns = ns;
            connections++;
            if (connections > 8)
            {
                ns.Close();
                client.Close();
                connections--;
                results.Items.Add("Max user limit is 8.");
            }
                Thread newthread = new Thread(new
                          ThreadStart(clnt.MessageHandler));
                newthread.Start();

            results.Items.Add("An user connected to server");
            try
            {

                while (true)
                {

                    data = new byte[1024];
                    if (idx < messages.Count)
                    {
                        Thread.Sleep(10);
                        msg = Encoding.ASCII.GetBytes(messages[idx]);
                        ns.Write(msg, 0, msg.Length);
                        idx += 1;

                    }
                }
            }
            catch (Exception e)
            {
                ns.Close();
                client.Close();
                connections--;
                results.Items.Add("An user disconnected from server");
            }
        }
        
    }
    class WeatherInformation
    {
        public void WeatherUpdater()
        {
            while (true)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.weatherapi.com/v1/current.json?key=1a2a8309b4fd4032be3180747230701&q=manisa");
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string strResponse = reader.ReadToEnd();
                JObject joResponse = JObject.Parse(strResponse);
                weather = joResponse["current"]["temp_c"].ToString();
                if (weather != oldweather && weather != null)
                {
                    LabelUpdater.SetText(ActiveForm, label1, "Manisa: " + weather + "°C");
                    messages.Add("-WEATHER-#" + weather);
                    oldweather = weather;
                }
                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("https://www.alphavantage.co/query?function=TIME_SERIES_INTRADAY&symbol=TSLA&interval=5min&apikey=NJO07SIMDFWJ161X");
                request2.Method = "GET";
                HttpWebResponse response2 = (HttpWebResponse)request2.GetResponse();
                Stream dataStream2 = response2.GetResponseStream();
                StreamReader reader2 = new StreamReader(dataStream2);
                string strResponse2 = reader2.ReadToEnd();
                JObject joResponse2 = JObject.Parse(strResponse2);
                teslastock = joResponse2["Time Series (5min)"].ToList()[0].ToList()[0].ToList()[0].ToString().Split(':')[1].Replace("\"", "");
                if (teslastock != oldteslastock && teslastock != null)
                {

                    LabelUpdater.SetText(ActiveForm, label2, "Tesla:" + teslastock);
                    messages.Add("-TESLA-#" + teslastock);
                    oldteslastock = teslastock;
                }
                Random rnd = new Random();
                DateTime currentTime = DateTime.Now;
                messages.Add("-DOOR-#" + rnd.Next(1, 9).ToString() + " " + currentTime.ToString("hh:mm tt"));
                Thread.Sleep(25000);
            }
        }
    }

    class LabelUpdater
    {
        delegate void TxtCallBack(Form f, Control ctr, string text);
        public static void SetText(Form form, Control ctr, string text)
        {
            if (ctr.InvokeRequired)
            {
                try
                {
                    TxtCallBack a = new TxtCallBack(SetText);
                    form.Invoke(a, new object[] { form, ctr, text });
                }
                catch (Exception e)
                {
                }
            }
            else
            {
                ctr.Text = text;
            }
        }
    }
    public static void Main()
    {
        Application.Run(new CinsAptServer());

    }
}