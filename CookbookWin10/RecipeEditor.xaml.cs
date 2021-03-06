﻿using System;
using System.IO;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using Newtonsoft.Json;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.Media;
using Windows.UI.Popups;
using System.Text;

namespace CookbookWin10
{
    public sealed partial class RecipeEditor : Page
    {
        private RecipeController recipeController;
        private int persons = 0;

        public RecipeEditor()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                    a.Handled = true;
                }
            };            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.Parameter).GetType() == typeof(RecipeController))
            {
                this.recipeController = (RecipeController)e.Parameter;
            }
        }

        private void btn_editor_category_Click(object sender, RoutedEventArgs e)
        {
            buildFlyout(sender);
        }

        public async void buildFlyout(object sender)
        {
            // make this await, this wont run async now
            MenuFlyout menuFlyout = new MenuFlyout();

            for (int x = 0; x < recipeController.getCategories().Count; x++)
            {
                MenuFlyoutItem flyItem = new MenuFlyoutItem();
                flyItem.Text = recipeController.getCategories()[x];
                flyItem.Click += FlyItem_Click;
                menuFlyout.Items.Add(flyItem);
            }

            menuFlyout.ShowAt((FrameworkElement)sender);
        }

        private void FlyItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(MenuFlyoutItem))
            {
                MenuFlyoutItem item = (MenuFlyoutItem)sender;

                setCategory(item.Text);
            }
        }

        private void setCategory(string category)
        {
            changeButtonText(category);
            fillRectanglesWithColors(RecipeController.getCategory(category));
        }

        private void fillRectanglesWithColors(int category)
        {
            int[] idx = { 0, 1, 2, 3 };
            MainPage.FisherYatesShuffle(idx);
            // ask Paul, is this okay? having same named variables in multiple XAML pages?
            rect_main.Fill = Category.colorSets[category, idx[0]];
            rect_ingredients.Fill = Category.colorSets[category, idx[1]];
            rect_left_low.Fill = Category.colorSets[category, idx[2]];
            rect_left_top.Fill = Category.colorSets[category, idx[3]];
            rect_sub_left.Fill = Category.colorSets[category, 0];
            rect_sub_middle.Fill = Category.colorSets[category, 1];
            rect_sub_right.Fill = Category.colorSets[category, 2];
        }

        private void changeButtonText(string category)
        {
            btn_editor_category.Content = category;
        }

        private void btn_personUp_Click(object sender, RoutedEventArgs e)
        {
            if (persons < 20)
                persons++;
            lbl_persons.Text = (persons != 1) ? persons + " personen" : persons + " persoon";
            lbl_error_persons.Visibility = Visibility.Collapsed;
        }

        private void btn_personDown_Click(object sender, RoutedEventArgs e)
        {
            if (persons > 1)
                persons--;
            lbl_persons.Text = (persons != 1) ? persons + " personen" : persons + " persoon";
            lbl_error_persons.Visibility = Visibility.Collapsed;
        }

        private async void btn_browsePhoto_Click(object sender, RoutedEventArgs e)
        {
            await selectFile(); // exception upon closing dialog
        }

        private async Task selectFile()
        {
            Windows.Storage.Pickers.FileOpenPicker picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            BitmapImage bitmap = new BitmapImage();
                        
            await bitmap.SetSourceAsync(await file.OpenAsync(Windows.Storage.FileAccessMode.Read));

            img_main.Source = bitmap;
        }

        private void cbx_type_Loaded(object sender, RoutedEventArgs e)
        {            
            var comboBox = sender as ComboBox;
            string[] source = new string[RecipeController.getRecipeTypes().Length + 1];
            int x = 0;
            for(x = 0; x < RecipeController.getRecipeTypes().Length; x++)
            {
                source[x] = RecipeController.getRecipeTypes()[x];
            }
            source[x] = ""; // an empty selection so user can de-select
            comboBox.ItemsSource = source;            
        }

        private void cbx_type1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            int index = comboBox.SelectedIndex;
            lbl_error_type.Visibility = Visibility.Collapsed;
            cbx_type1.BorderBrush = R.Colors.BORDER_DEFAULT;

            if (comboBox.SelectedIndex != comboBox.Items.Count - 1 && comboBox.SelectedIndex != -1)
                cbx_type2.Visibility = Visibility.Visible;
            else
            {
                cbx_type2.Visibility = Visibility.Collapsed;
                cbx_type2.SelectedIndex = -1;
                cbx_type3.Visibility = Visibility.Collapsed;
                cbx_type3.SelectedIndex = -1;
            }
        }
        private void cbx_type2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            int index = comboBox.SelectedIndex;

            if (comboBox.SelectedIndex == comboBox.Items.Count - 1)
            {
                cbx_type3.Visibility = Visibility.Collapsed;
                cbx_type3.SelectedIndex = -1;
            }
            else
                cbx_type3.Visibility = Visibility.Visible;

        }
        private void cbx_type3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;
            int index = comboBox.SelectedIndex;
            if (comboBox.SelectedIndex == comboBox.Items.Count-1)            
                cbx_type3.Visibility = Visibility.Collapsed;            
        }
        private void btn_makePhoto_Click(object sender, RoutedEventArgs e)
        {
            getWebcam2();
        }

        private async void getWebcam2()
        {
            MediaCapture capture = new MediaCapture();
            await capture.InitializeAsync();            

            await capture.ClearEffectsAsync(MediaStreamType.VideoRecord);

            cap_CaptureElement.Source = capture;
            await capture.StartPreviewAsync();
        }

        private async void getWebcam()
        {
            MediaCapture mediaCapture = new MediaCapture();
            string deviceID = "";

            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            for (int x = 0; x < devices.Count; x++)
            {
                tbx_recipe.Text += "✠  Kind:" + (devices[x]).Kind.ToString() + " Name: " + (devices[x]).Name.ToString();
                deviceID = devices[x].Id;
            }

            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
            settings.AudioDeviceId = "";
            settings.VideoDeviceId = deviceID;
            settings.PhotoCaptureSource = PhotoCaptureSource.Photo;
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            await mediaCapture.InitializeAsync(settings);

            VideoEncodingProperties resolutionMax = null;
            int max = 0;
            var resolutions = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo);
            for (int x = 0; x < resolutions.Count; x++)
            {
                VideoEncodingProperties res = (VideoEncodingProperties)resolutions[x];
                tbx_recipe.Text += "Resolution: " + res.Width + "x" + res.Height;
                if(res.Width * res.Height > max)
                {
                    max = (int)(res.Width * res.Height);
                    resolutionMax = res;
                }
            }
            await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolutionMax);
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            var fPhotoStream = new InMemoryRandomAccessStream();
        }

        private async void btn_editor_preview_Click(object sender, RoutedEventArgs e)
        {
            if(checkFields())
            {
                Recipe recipe = new Recipe();
                recipe.title = tbx_title.Text;
                recipe.subtitle = tbx_subtitle.Text;
                //recipe.category = 
                recipe.author = tbx_author.Text;
                recipe.ingredients = tbx_ingredients.Text;
                recipe.recipe = tbx_recipe.Text;
                recipe.preperationTime = tbx_preperationTime.Text;
                recipe.persons = persons;
                recipe.types = parseRecipeTypes();

                await (new MessageDialog(recipe.getJSON())).ShowAsync();
            }
        } 
        
        private bool checkFields()
        {
            bool returner = true;
            if (tbx_title.Text.Length < 1)
            {
                lbl_error_title.Visibility = Visibility.Visible;
                lbl_error_title.Text = "U moet een titel opgeven!";
                tbx_title.BorderBrush = R.Colors.BORDER_ERROR;
                returner = false;
            }
            else
            {
                lbl_error_title.Visibility = Visibility.Collapsed;
                lbl_error_title.Text = "U moet een titel opgeven!";
                tbx_title.BorderBrush = R.Colors.BORDER_DEFAULT;
            }
            if (cbx_type1.SelectedIndex < 0 || cbx_type1.SelectedIndex == (cbx_type1.Items.Count-1))
            {
                lbl_error_type.Visibility = Visibility.Visible;
                lbl_error_type.Text = "U moet een type gerecht kiezen!";
                cbx_type1.BorderBrush = R.Colors.BORDER_ERROR;
                returner = false;
            }
            if (persons < 1)
            {
                lbl_error_persons.Visibility = Visibility.Visible;
                lbl_error_persons.Text = "U moet aantal personen kiezen!";
                returner = false;
            }
            return returner;
        }  
        
        private string parseRecipeTypes()
        {
            StringBuilder sb = new StringBuilder();
            if (cbx_type1.SelectedIndex >= 0 && cbx_type1.SelectedIndex != cbx_type1.Items.Count-1)
                sb.Append(cbx_type1.SelectedIndex + ",");
            if(cbx_type2.SelectedIndex >= 0 && cbx_type2.SelectedIndex != cbx_type2.Items.Count - 1)
                sb.Append(cbx_type2.SelectedIndex + ",");
            if (cbx_type3.SelectedIndex >= 0 && cbx_type3.SelectedIndex != cbx_type3.Items.Count-1)
                sb.Append(cbx_type3.SelectedIndex);
            return sb.ToString();
        }     
        private int parseCategory()
        {
            // make categories into integers
            return 0;
        }
    }
}