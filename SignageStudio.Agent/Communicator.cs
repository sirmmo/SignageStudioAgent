using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using agsXMPP;
using agsXMPP.protocol.client;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System.Diagnostics;
using System.Drawing;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Chrome;

namespace SignageStudio.Agent
{
    public class Communicator
    {

        Process chromedriverprocess = null;
        private XmppClientConnection xmpp;
        String username = "marco.montanari";
        string server = "gmail.com";
        string password = "cip50z31";
        Jid jid;
        string panel_name = "prova123";


        private RemoteWebDriver wd;

        private bool connected = false;

        public void Refresh()
        {
            if (connected)
                Disconnect();
            Start();
        }

        public void Start()
        {
            //get new data
            Connect(username, password, panel_name);
            connected = true;
        }

        private void Connect(string username, string password, string panel_name)
        {

            jid = new Jid(username, server, panel_name);
            xmpp = new XmppClientConnection(jid.Server);
            try
            {
                xmpp.Open(jid.User, password);
                xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
                xmpp.OnAuthError += new XmppElementHandler(xmpp_OnAuthError);
                xmpp.OnClose += new ObjectHandler(xmpp_OnClose);
            }
            catch (Exception)
            {
                if (ConnectionError != null)
                    ConnectionError(this, EventArgs.Empty);
            }
        }


        void xmpp_OnClose(object sender)
        {

        }

        void xmpp_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {

            Disconnect();
        }

        public event EventHandler ConnectionError;
        public event EventHandler Connected;

        private void xmpp_OnLogin(object sender)
        {
            if (Connected != null)
                Connected(this, EventArgs.Empty);
            //Notifico a tutti la mia presenza
            Presence p = new Presence(ShowType.chat, "Online");
            p.Type = PresenceType.available;
            xmpp.Send(p);
            xmpp.OnMessage += new MessageHandler(xmpp_OnMessage);

            //Mono.Zeroconf.RegisterService rs = new Mono.Zeroconf.RegisterService();
            //rs.Name = "_signage_client";
            //rs.Port = 80;
            //rs.RegType = "_tcp";
            //rs.Register();

            xmpp.Send(new Message("marco.montanari@gmail.com/Aramis", "test"));

            ProcessStartInfo chromedriver = new ProcessStartInfo("chromedriver.exe");
            chromedriver.CreateNoWindow = true;
            chromedriverprocess = Process.Start(chromedriver);


            DesiredCapabilities capability = DesiredCapabilities.Chrome();
            capability.SetCapability("chrome.binary", "chrome-win32\\chrome.exe");
            capability.SetCapability("chrome.switches", new List<String>() { "--kiosk", "--disable-translate", "--allow-outdated-plugins", "--profile-directory profiles" }.ToArray());
            wd = new RemoteWebDriver(new System.Uri("http://localhost:9515/"), capability);
            wd.Navigate().GoToUrl("http://www.google.com/");

            wd.FindElementByName("q").SendKeys("prova");
        }

        public event EventHandler Quit;

        void xmpp_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
        {
            if (msg.Body != null)
            {
                Console.WriteLine(msg.Body);
                String[] cmds = msg.Body.Split(new char[]{' '},2);
                switch (cmds[0])
                {
                    case "go":
                        wd.Navigate().GoToUrl(cmds[1]);
                        break;
                    case "alive":
                        msg.Body = wd.Url;
                        msg.From = jid.ToString();
                        msg.To = "marco.montanari@gmail.com/Aramis";
                        xmpp.Send(msg);
                        break;
                    case "refresh":
                        break;

                    case "load":
                        break;

                    case "quit":
                        if (Quit != null)
                            Quit(this, EventArgs.Empty);
                        break;

                    case "js":
                        wd.ExecuteScript(cmds[1]);
                        break;
                    default:
                        break;
                }
            }
            msg = null;
            Presence p = new Presence();
            p.Type = PresenceType.available;
            xmpp.Send(p);
        }
        public event EventHandler Disconnected;

        public void Disconnect()
        {

            xmpp.Close();
            wd.Quit();
            chromedriverprocess.Close();
            if (Disconnected != null)
                Disconnected(this, EventArgs.Empty);
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }
    }
}
