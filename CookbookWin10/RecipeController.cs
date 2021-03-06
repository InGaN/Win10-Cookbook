﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace CookbookWin10
{
    class RecipeController
    {
        private List<MainListboxModel> listboxItems;
        private MainListboxModel dailyRecipe;
        private List<string> categories;

        private EventArgs eventArgs = null;
        public event JsonReadyHandler jsonReady;
        public event JsonReadyHandler jsonFailed;
        public delegate void JsonReadyHandler(RecipeController rc, EventArgs e);
        
        public RecipeController()
        {
            retrieveJSON();        
        }

        private async void retrieveJSON()
        {
            string output = null;
            string page = Config.URL_JSON_MAIN;
            HttpClient client = new HttpClient();

            try {                
                HttpResponseMessage response = await client.GetAsync(page);
                HttpContent content = response.Content;
                output = await content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                output = null;
                jsonFailed(this, eventArgs);
            }
            
            if (output != null)
            {
                this.listboxItems = JsonConvert.DeserializeObject<List<MainListboxModel>>(output);
                
                for (int x = 0; x < getListboxItems().Count; x++)
                {
                    BitmapImage img = new BitmapImage();
                    if (getListboxItems()[x].image.Length != 0)
                    {
                        try
                        {
                            page = Config.URL_GALLERY + getListboxItems()[x].image;
                            Stream st = await client.GetStreamAsync(page);

                            var memoryStream = new MemoryStream();
                            await st.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            img.SetSource(memoryStream.AsRandomAccessStream());
                        }
                        catch (Exception e)
                        {
                            
                        }
                        getListboxItems()[x].bitmapImage = img;
                    }

                    // obsolete
                    Random random = new Random();
                    int colorIndex = random.Next(4);
                    if (getListboxItems()[x].category.Equals("Spaans"))
                    {
                        getListboxItems()[x].rectColor = (Category.colorSets[Category.SPANISH, colorIndex]);
                    }
                    else if (getListboxItems()[x].category.Equals("Frans"))
                    {
                        getListboxItems()[x].rectColor = (Category.colorSets[Category.FRENCH, colorIndex]);
                    }
                }

                this.categories = new List<string>();
                for (int x = 0; x < listboxItems.Count; x++)
                {
                    if (!categories.Contains(listboxItems[x].category))
                    {
                        categories.Add(listboxItems[x].category);
                    }
                }
                jsonReady(this, eventArgs);
            }
        }

        public List<string> getCategories()
        {
            return categories;
        }  
        
        public static int getCategory(string category)
        {
            int cat = 0;
            if (category.Equals("Spaans"))
                cat = Category.SPANISH;
            else if (category.Equals("Frans"))
                cat = Category.FRENCH;
            else if (category.Equals("Amerikaans"))
                cat = Category.AMERICAN;
            else if (category.Equals("Italiaans"))
                cat = Category.ITALIAN;
            return cat;
        }
            
              
        public void randomDailyRecipe()
        {
            Random random = new Random();
            int idx = random.Next(listboxItems.Count);
            dailyRecipe = listboxItems[idx];
        }

        public MainListboxModel getDailyRecipe()
        {
            return dailyRecipe;
        }

        public List<MainListboxModel> getListboxItems()
        {
            return listboxItems;
        }

        public List<MainListboxModel> getListboxItems(string category)
        {
            if(category.Equals("all"))
            {
                return listboxItems;
            }

            List<MainListboxModel> list = new List<MainListboxModel>();
            for(int x = 0; x < listboxItems.Count; x++)
            {
                if(listboxItems[x].category.Equals(category))
                {
                    list.Add(listboxItems[x]);
                }
            }
            return list;
        }

        public async Task<List<MainListboxModel>> getFavorites()
        {
            List<MainListboxModel> list = new List<MainListboxModel>();
            StorageManager storageManager = new StorageManager();
            
            for (int x = 0; x < listboxItems.Count; x++)
            {
                bool fav = await storageManager.isFavorite(listboxItems[x].id);
                if (fav)                
                    list.Add(listboxItems[x]);                
            }
            return list;
        }
    

        public void setListBoxColors(int catColorID)
        {
            foreach (MainListboxModel model in listboxItems)
            {
                model.rectColor = Category.colorSets[catColorID, 0];
            }
        }

        public void setListboxItems(List<MainListboxModel> list)
        {
            this.listboxItems = list;
        }
        public static string[] getSearchItems()
        {
            // 0 = no cat
            // 1 = 3-Gangen
            return new string[] { "Willekeurig", "3-Gangen Menu" };
        }
        public static string[] getRecipeTypes()
        {            
            // 0 = Voorgerecht
            // 1 = Hoofdgerecht
            // 2 = Nagerecht
            // 3 = Snacks
            // 4 = Ontbijtgerecht
            // 5 = Lunchgerecht
            // 6 = Drank
            // 7 = Bijgerecht            
            return new string[] { "Voorgerecht", "Hoofdgerecht", "Nagerecht", "Snack",
            "Ontbijtgerecht", "Lunchgerecht", "Drank", "Bijgerecht"};
        }
        public static string[] getSortingItems()
        {
            string[] array = new string[getSearchItems().Length + getRecipeTypes().Length];

            for(int x = 0; x < getSearchItems().Length; x++)
            {
                array[x] = getSearchItems()[x];
            } 
            for(int x = 0; x < getRecipeTypes().Length; x++)
            {
                array[x + getSearchItems().Length] = getRecipeTypes()[x];
            }

            return array;
        }
    }
}
