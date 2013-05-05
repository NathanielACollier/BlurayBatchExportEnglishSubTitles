using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExtractEnglishSubTitlesWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));


        public MainWindow()
        {
            InitializeComponent();
            InitializeLog4Net();

            log.Info("Application started.");


        }


        private void InitializeLog4Net()
        {
            string configText = @"
                                <log4net>
                                  <root>
                                    <level value='ALL' />   
                                  </root>
                                </log4net>
                                ";
            // configure from stream
            System.IO.Stream configStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(configText));
            log4net.Config.XmlConfigurator.Configure(configStream);

            // add in our notify appender
            var repository = (log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetRepository();

            var appender = new Log4NetNotifyAppender.NotifyAppender();
            appender.NewLogEntry += appender_NewLogEntry;

            appender.Layout = new log4net.Layout.PatternLayout("[%date{yyyy-MM-dd hh:mm:sstt}] - %-5level %logger - %message%newline");
            appender.ActivateOptions();

            repository.Root.AddAppender(appender);
        }

        void appender_NewLogEntry(object sender, Log4NetNotifyAppender.NewLogEntryEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                MainViewModel model = (MainViewModel)this.DataContext;
                model.Message += e.Message;
            }));
        }

        private void AddBlurayFolderButton_Click_1(object sender, RoutedEventArgs e)
        {
            MainViewModel model = (MainViewModel)this.DataContext;
            var folderDialog = new OpenFolderDialog.OpenFolderDialog();

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                model.Folders.Add(new BlurayFolder { Path = folderDialog.Folder });
                log.InfoFormat("Bluray Folder added {0}", folderDialog.Folder);
            }
        }

        private void RemoveBluRayFolderButton_Click_1(object sender, RoutedEventArgs e)
        {
            Button removeButton = (Button)sender;
            BlurayFolder folder = (BlurayFolder)removeButton.DataContext;

            MainViewModel model = (MainViewModel)this.DataContext;

            model.Folders.Remove(folder);

            log.InfoFormat("Bluray folder {0} removed", folder.Path);
        }

        private void RunSubTitleExtractionButton_Click_1(object sender, RoutedEventArgs e)
        {
            MainViewModel model = (MainViewModel)this.DataContext;

            Thread t = new Thread((ThreadStart)delegate
            {
                try
                {
                    foreach (var folder in model.Folders)
                    {
                        Eac3ToLib.Eac3ToLib.GetAllEnglishSubTitles(folder.Path, model.SubTitlesFolder);


                    }

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        log.Info("Subtitle Extraction Complete");
                        log.Info("Clearing folders");
                        model.Folders.Clear();
                    }));
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error running subtitle extraction.  Exception: {0}", ex);
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        model.Busy = false;
                    }));
                }
            });

            model.Busy = true;
            t.Start();
        }

        private void ChangeSubtitlesFolder_Click_1(object sender, RoutedEventArgs e)
        {
            MainViewModel model = (MainViewModel)this.DataContext;

            var diag = new OpenFolderDialog.OpenFolderDialog();

            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                model.SubTitlesFolder = diag.Folder;
                log.InfoFormat("Sub title folder changed to {0}", model.SubTitlesFolder);
            }
        }



    }
}
