using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace WpfTest
{
    public partial class MainWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            worker.RunWorkerAsync("http://www.stuff.co.nz");
            textBox1.Text = "done";
        }

        public delegate void DoUIWorkHandler();

        public void worker_DoWork(object sender, DoWorkEventArgs args)
        {
            string startingUri = (string)args.Argument;
            //Spider s = new Spider();

            ////do little chunks of work
            //var htmlToDisplay = s.GetHtml(startingUri);
            
            
            //args.Result = htmlToDisplay;


            Spider s = new Spider();
            //string html = s.Start(startingSite);

            var thing = s.RunSpiderGetNext(startingUri, 5);

            foreach (var item in thing)
            {
                var a = item;
                Console.WriteLine("a is {0}", a);
                //need to update the UI
                textBox1.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBox1.Text += a + "\r\n"; });
            }

        }

        public void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            //object result = args.Result;
            //string result2 = result.ToString();
            //webBrowser.NavigateToString(result2);
        }

        void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            SuppressScriptErrors(webBrowser, true);
        }

        public void SuppressScriptErrors(System.Windows.Controls.WebBrowser wb, bool Hide)
        {
            FieldInfo fi = typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                object browser = fi.GetValue(wb);
                if (browser != null)
                    browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, browser, new object[] { Hide });
            }
        }


       
    }
}
