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
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            InitializeComponent();
        }

        public class ArgumentsToPassToBackgroundWorker
        {
            public string URL { get; set; }
            public int NumberOfJumps { get; set; }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            textBoxSitesVisited.Text = "";

            object[] arguments = { textBoxStartingURL.Text, textBoxNumberOfJumps.Text };
            worker.RunWorkerAsync(arguments);
        }

        public delegate void DoUIWorkHandler();

        public void worker_DoWork(object sender, DoWorkEventArgs args)
        {
            //string startingUri = (string)args.Argument;
            object[] arguments = (object[])args.Argument;

            string startingUri = (string)arguments[0];
            string numberOfJumpsAsString = (string)arguments[1];
            int numberOfJumps = Convert.ToInt32(numberOfJumpsAsString);

            Spider s = new Spider();

            IEnumerable<thing> listOfThings = s.RunSpiderGetNext(startingUri, numberOfJumps);

            int bytesTransferred = 0;
            foreach (var item in listOfThings)
            {
                //need to update the UI
                textBoxSitesVisited.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxSitesVisited.Text += item.Uri + "\r\n"; });
                textBoxSitesVisited.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxSitesVisited.ScrollToEnd(); });
                
                webBrowser.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { webBrowser.NavigateToString(item.Html); });

                textBoxMessages.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxMessages.Text = item.Messages + "\r\n"; });

                bytesTransferred += item.SizeOfPageInBytes;

                decimal megaBytesTransferred = 0m;
                megaBytesTransferred = (decimal)bytesTransferred / 1048576;
                textBoxMBTransferred.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxMBTransferred.Text = megaBytesTransferred.ToString("#.##"); });

                //System.Threading.Thread.Sleep(2000);
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
            FieldInfo fi =typeof(System.Windows.Controls.WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi != null)
            {
                object browser = fi.GetValue(wb);
                if (browser != null)
                    browser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, browser, new object[] { Hide });
            }
        }

       

       
    }
}
