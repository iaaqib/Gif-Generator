using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Gif_Generator.Resources;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Windows.Media.Imaging;
using System.IO;
using Windows.Storage.Pickers;
using System.Windows.Media;
using System.Windows.Threading;
using Lumia.Imaging;
using Lumia.Imaging.Transforms;

using System.Threading.Tasks;
using ImageTools;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Threading;
using Microsoft.Phone.Tasks;

namespace Gif_Generator
{
    public partial class MainPage : PhoneApplicationPage
    {

        public static MainPage Current;
        // Constructor

        BackgroundWorker BackgroundWork;

        Popup pop;

        public MainPage()
        {
            InitializeComponent();
            pop = new Popup();
            
            pop.IsOpen = true;
            
            pop.Child = new SplashScreen();


            ApplicationBar.IsMenuEnabled = false;   
            RunProcess();
            
            
          //  radAutoCompleteBox1.SuggestionsSource = Data.GetSuggestions();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void RunProcess()

{
 BackgroundWork = new BackgroundWorker();
BackgroundWork.RunWorkerCompleted += ((r, args) => { this.Dispatcher.BeginInvoke(() => { this.pop.IsOpen = false; ApplicationBar.IsMenuEnabled = true; }); });

BackgroundWork.DoWork += ((r, args) => { Thread.Sleep(5000);
              
});

BackgroundWork.RunWorkerAsync();
 

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = App.Current as App;
            
            if (app.FilePickerContinuationArgs != null)
            {
                this.ContinueFileOpenPicker(app.FilePickerContinuationArgs);
                app.FilePickerContinuationArgs = null;
            }

            base.OnNavigatedTo(e);

          

        }

     
        
        
        private void picker_Click(object sender, RoutedEventArgs e)
        {
           
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.ContinuationData["Operation"] = "UpdateProfilePicture";
            openPicker.PickMultipleFilesAndContinue();


        }
        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            
            if ((args.ContinuationData["Operation"] as string) == "UpdateProfilePicture" &&
               args.Files != null &&
               args.Files.Count > 0)
            {
                //    for (int i = 0; i< args.Files.Count; i++) {
                if (args.Files.Count == 1)
                {

                    MessageBox.Show("Choose At least two images to generate a Gif");
                }
                else
                {
                    List<IImageProvider> imageProviders = new List<IImageProvider>();
                    List<IImageProvider> tempImageProvider = new List<IImageProvider>();
                    //   _onScreenImageProviders = args.Files[0];// CreateImageSequenceFromResources(file , args.Files.Count);

                    try
                    {
                        
                        for (int i = 0; i < args.Files.Count; i++)        
                        {

                            Uri uri = new Uri(args.Files[i].Name, UriKind.Relative);
                            
                            Stream stream = await args.Files[i].OpenStreamForReadAsync(); 
                            StreamImageSource sis = new StreamImageSource(stream);
                            imageProviders.Add(sis);
                   
                        }
                      
                    }
                    catch (NullReferenceException ex)
                    {
                        // No more images available
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        //}
                           CreateGif(imageProviders);
                        
                       // await GifExporter.Export(imageProviders, null);
                    } 
                     
                }
            }
        


        }

      
        private void  CreateGif(List<IImageProvider> imageProviders)
        {
            var tb = new TextBox();
            tb.Text = "100";
            var messageBox = new CustomMessageBox()
            {
                Caption = "Frame Delay",
                Message = "Please enter frame delay, make sure it is in digits",
                LeftButtonContent = "ok",
                RightButtonContent = "cancel",
                Content = tb,
                IsFullScreen = false
            };
            //Add the dismissed event handler
            messageBox.Dismissed += async (s1, e1) =>
                        {
                            switch (e1.Result)
                            {
                                case CustomMessageBoxResult.LeftButton:
                                    //add the task you wish to perform when user clicks on yes button here
                                    int Duration;
                                    if (int.TryParse(tb.Text, out Duration))
                                    {
                                        System.Diagnostics.Debug.WriteLine("" + Duration);
                                        ProgressBar.Visibility = Visibility.Visible;
                                        ProgressBar.IsIndeterminate = true;
                                        await GifExporter.Export(imageProviders, null, Duration);
                                        ProgressBar.IsIndeterminate = false;
                                        ProgressBar.Visibility = Visibility.Collapsed;
                                    }
                                    else {
                                        MessageBox.Show("Enter duration in digits to proceed");
                                       CreateGif(imageProviders);
                                    }
                                        break;
                                case CustomMessageBoxResult.RightButton:
                                    //add the task you wish to perform when user clicks on no button here
                                    MessageBox.Show("Gif creation was cancel");
                                    break;
                                case CustomMessageBoxResult.None:
                                    // Do something.
                                    break;
                                default:
                                    break;
                            }
                        };
                        
            //add the show method
                messageBox.Show();
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();

            marketplaceReviewTask.Show();
        }

      
        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}