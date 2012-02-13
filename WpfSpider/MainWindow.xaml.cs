using System;
using System.Collections.Generic;
using System.Windows;
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
            textBoxSitesVisited.Text = "";

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            object[] arguments = { textBoxStartingURL.Text, textBoxNumberOfJumps.Text };
            worker.RunWorkerAsync(arguments);
        }

        public void worker_DoWork(object sender, DoWorkEventArgs args)
        {
            object[] arguments = (object[])args.Argument;

            string startingSite = (string)arguments[0];
            string numberOfJumpsAsString = (string)arguments[1];
            int numberOfJumps = Convert.ToInt32(numberOfJumpsAsString);

            Spider s = new Spider();

            //deferred execution ie it wont call RunSpiderGetNext until it is iterated over in the foreach block
            //a lazy enumerator sequence?
            IEnumerable<WebPageInfo> listOfWebPageInfoOfSiteCurrentlyOn = s.RunSpiderGetNext(startingSite, numberOfJumps);

            int bytesTransferred = 0;
            foreach (var item in listOfWebPageInfoOfSiteCurrentlyOn)
            {
                textBoxSitesVisited.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxSitesVisited.Text += item.Uri + "\r\n"; });
                textBoxSitesVisited.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxSitesVisited.ScrollToEnd(); });

                textBoxMessages.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxMessages.Text = item.Messages + "\r\n"; });

                webBrowser.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { webBrowser.NavigateToString(item.Html); });

                bytesTransferred += item.SizeOfPageInBytes;

                var megaBytesTransferred = (decimal)bytesTransferred / 1048576;
                textBoxMBTransferred.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate { textBoxMBTransferred.Text = megaBytesTransferred.ToString("#.##"); });

                //assigning the variable therefore I don't need a return
                //bool? isSleepChecked = null;
                //checkBoxSleep.Dispatcher.Invoke(new Action(() =>
                //                                            {
                //                                                isSleepChecked = checkBoxSleep.IsChecked;
                //                                            })
                //                                ,DispatcherPriority.Normal);


                //using the func to return the nullable boolean.  lambda expression
                //bool? isSleepChecked = (bool?)checkBoxSleep.Dispatcher.Invoke(new Func<bool?>(() => checkBoxSleep.IsChecked));
                //rewritten using a delegate
                bool? isSleepChecked = (bool?)checkBoxSleep.Dispatcher.Invoke(new Func<bool?>(delegate { return checkBoxSleep.IsChecked; }));

                if (isSleepChecked == true)
                    Thread.Sleep(2000);
            }

        }

        //to stop the annoying javascript popups in the wpf browser control
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

        private void checkBoxSleep_Checked(object sender, RoutedEventArgs e)
        {}
    }
}