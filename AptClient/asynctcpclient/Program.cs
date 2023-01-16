using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;


class CinsAptClient : Form
{
    private TextBox newText;
    private TextBox newText1;
    private ListBox results;
    private ListBox results1;
    private Socket client;
    public Pen asdasd;
    private byte[] data = new byte[1024];
    private int size = 1024;
    public static Label label5 = new Label();
    public static Label label6 = new Label();
    public static Button connect = new Button();

    public CinsAptClient()
    {
        Text = "CinsAptClient";
        Size = new Size(530, 340);
        BackColor = Color.LightBlue;

        //NAME INPUT
        newText = new TextBox();
        newText.Parent = this;
        newText.Size = new Size(360, 2 * Font.Height);
        newText.Location = new Point(10, 260);


        //CHAT INPUT
        newText1 = new TextBox();
        newText1.Parent = this;
        newText1.Size = new Size(98, 2 * Font.Height);
        newText1.Location = new Point(400, 55);


        //LOGS
        results = new ListBox();
        results.Parent = this;
        results.Location = new Point(10, 55);
        results.Size = new Size(160, 11 * Font.Height);
        results.Font= new Font("Arial", 7);

        //CHAT
        results1 = new ListBox();
        results1.Parent = this;
        results1.Location = new Point(210, 55);
        results1.Size = new Size(160, 11 * Font.Height);

        Label label2 = new Label();
        label2.Parent = this;
        label2.Text = "LOGS";
        label2.Location = new Point(70, 31);

        Label label3 = new Label();
        label3.Parent = this;
        label3.Text = "CHAT";
        label3.Location = new Point(272, 31);


        
        label5.Parent = this;
        label5.Text = "TESLA :";
        label5.Location = new Point(400, 129);

        
        label6.Parent = this;
        label6.Text = "MANISA :";
        label6.Location = new Point(400, 159);

        Label label7 = new Label();
        label7.Parent = this;
        label7.Text = "Your Name";
        label7.Location = new Point(400, 31);

        Label label4 = new Label();
        label4.Parent = this;
        label4.Text = "";
        label4.Location = new Point(0, 240);
        label4.BorderStyle = BorderStyle.Fixed3D;
        label4.AutoSize = false;
        label4.Height = 2;
        label4.Width = 520;
        label4.BackColor = Color.Black;

        Button sendit = new Button();
        sendit.Parent = this;
        sendit.Text = "Send";
        sendit.Location = new Point(400, 258);
        sendit.Size = new Size(6 * Font.Height, 24);
        sendit.Click += new EventHandler(ButtonSendOnClick);

        
        connect.Parent = this;
        connect.Text = "Connect";
        connect.Location = new Point(400, 92);
        connect.Size = new Size(6 * Font.Height, 24);
        connect.Click += new EventHandler(ButtonConnectOnClick);

        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
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

    void ButtonConnectOnClick(object obj, EventArgs ea)
    {
        Socket newsock = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
        newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);
    }

    void ButtonSendOnClick(object obj, EventArgs ea)
    {
        byte[] message = Encoding.ASCII.GetBytes(newText1.Text +": " +newText.Text);
        newText.Clear();
        client.BeginSend(message, 0, message.Length, SocketFlags.None,
                     new AsyncCallback(SendData), client);
    }

    void Connected(IAsyncResult iar)
    {
        connect.Enabled = false;
        newText1.Enabled = false;
        client = (Socket)iar.AsyncState;
        try
        {
            client.EndConnect(iar);
            client.BeginReceive(data, 0, size, SocketFlags.None,
                          new AsyncCallback(ReceiveData), client);
        }
        catch (SocketException)
        {
        }
    }

    void ReceiveData(IAsyncResult iar)
    {
        Socket remote = (Socket)iar.AsyncState;
        int recv = remote.EndReceive(iar);
        string stringData = Encoding.ASCII.GetString(data, 0, recv);
        if (stringData.Split("#")[0] == "-DOOR-")
        {
            results.Items.Add("No:" + stringData.Split("#")[1] +" used Card Reader");
        }
        else if (stringData.Split("#")[0] == "-CHAT-")
        {
            results.Items.Add(stringData);
        }
        else if (stringData.Split("#")[0] == "-WEATHER-")
        {
            label6.Text = "MANISA :" + stringData.Split("#")[1]+ "°C";
        }
        else if (stringData.Split("#")[0] == "-TESLA-")
        {
            label5.Text = "TESLA :"+ stringData.Split("#")[1];
        }
        else
        {
            results1.Items.Add(stringData);
        }
        client.BeginReceive(data, 0, size, SocketFlags.None,
                          new AsyncCallback(ReceiveData), client);
    }
    void SendData(IAsyncResult iar)
    {
        Socket remote = (Socket)iar.AsyncState;
        int sent = remote.EndSend(iar);
        remote.BeginReceive(data, 0, size, SocketFlags.None,
                      new AsyncCallback(ReceiveData), remote);
    }
    public static void Main()
    {
        Application.Run(new CinsAptClient());
    }
}