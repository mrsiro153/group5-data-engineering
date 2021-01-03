using Dasync.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Newtonsoft.Json;
using Windows.Web.Http;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace DE_CW_1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        int CHUNK_SIZE_THRESHOLD = 150;
        string EMAIL = "nvttrang.sdh20@hcmut.edu.vn";
        Uri CSS_UPLOAD_ENDPOINT = new Uri("http://172.16.5.36:8000/submitFile");
        Uri CSS_VERIFY_ENDPOINT = new Uri("http://172.16.5.36:8000/challenge");
        Uri BC_ENDPOINT = new Uri("http://172.16.5.152:3000/merkleroot");

        StorageFile selected_file = null;
        String file_result_text = "";
        String selected_shard = "";
        bool is_passed = false;
        ObservableCollection<FileSelection> list_shards = new ObservableCollection<FileSelection>();
        IList<string> metadata = null;

        public MainPage()
        {
            this.InitializeComponent();
            reload_shards();
            feature_selection.IsChecked = false;
            grid_uploading.Visibility = Visibility.Visible;
            grid_verification.Visibility = Visibility.Collapsed;
            txt_selected_file.Text = "";
            btn_upload_file.Visibility = Visibility.Collapsed;
            loading_control.IsLoading = false;

        }

        /// 
        /// 
        /// 

        private void feature_selection_checked(object sender, RoutedEventArgs e)
        {
            reload_shards();
            //System.Diagnostics.Debug.WriteLine("checked!");
            ToggleButton tb = sender as ToggleButton;

            tb.Content = "Data Verification";
            txt_verification_result.Text = "";
            grid_uploading.Visibility = Visibility.Collapsed;
            grid_verification.Visibility = Visibility.Visible;
            btn_verify_file.Visibility = Visibility.Collapsed;
        }

        private void feature_selection_unchecked(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("unchecked!");
            ToggleButton tb = sender as ToggleButton;


            //tb.Background = aquaBrush;
            tb.Content = "Data Uploading";
            grid_verification.Visibility = Visibility.Collapsed;
            grid_uploading.Visibility = Visibility.Visible;
            txt_selected_file.Text = "";
            btn_upload_file.Visibility = Visibility.Collapsed;
            selected_file = null;
        }

        private async void btn_select_file_click(object sender, RoutedEventArgs e)
        {
            selected_file = null;
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".txt");
            selected_file = await openPicker.PickSingleFileAsync();
            if (selected_file != null)
            {
                txt_selected_file.Text = "Selected File: " + selected_file.Name;
                btn_upload_file.Visibility = Visibility.Visible;
            }
            else
            {
                txt_selected_file.Text = "Try Again...";
                btn_upload_file.Visibility = Visibility.Collapsed;
            }
        }


        private void btn_upload_file_click(object sender, RoutedEventArgs e)
        {

            loading_control.IsLoading = true;
            process_uploading();
            btn_upload_file.Visibility = Visibility.Collapsed;

            txt_selected_file.Text = "Uploaded file " + selected_file.Name + " sucessfully. Please select another file to continue...";
            loading_control.IsLoading = false;
        }

        private void cbox_select_shard_changed(object sender, SelectionChangedEventArgs e)
        {
            btn_verify_file.Visibility = Visibility.Visible;
            ComboBox comboBox = sender as ComboBox;
            selected_shard = comboBox.SelectedValue == null ? null : comboBox.SelectedValue.ToString();
            System.Diagnostics.Debug.WriteLine(selected_shard);
        }


        private void btn_verify_file_click(object sender, RoutedEventArgs e)
        {
            loading_control.IsLoading = true;
            process_verification();
            cbox_select_shard.SelectedValue = null;
            btn_verify_file.Visibility = Visibility.Collapsed;
            txt_verification_result.Text = "Please check your mail " + EMAIL + " for the result";
            loading_control.IsLoading = false;
        }

        /// 
        /// 
        ///

        private async void process_uploading()
        {
            string content = await FileIO.ReadTextAsync(selected_file);

            /// 
            /// BUILD MERKLE TREE
            ///
            int numLeaves = (int) Math.Ceiling(Math.Log((double) content.Length / CHUNK_SIZE_THRESHOLD, 2));
            int chunk_size = (int)Math.Ceiling((double) content.Length / Math.Pow(2, numLeaves));
            string identifier = selected_file.Name.Split(".")[0] + "_" + DateTime.Now.ToString("yyyyMMdd-Hmmss");

            List<String> data_chunks = Enumerable.Range(0, (int) Math.Pow(2, numLeaves)).Select(
                i => content.Substring(i * chunk_size, (i+1) * chunk_size > content.Length ? content.Length - i*chunk_size : chunk_size)).ToList();
            MerkleTree tree = new MerkleTree(data_chunks, chunk_size, identifier);
            String merkleRootHash = tree.merkleRoot;
            String json = JsonConvert.SerializeObject(tree.formattedMerkleTree);
            System.Diagnostics.Debug.WriteLine(json);



            /// 
            /// WRITE MERKLE TREE
            /// 
            string merkleFileName = selected_file.Name.Split(".")[0] + ".json";
            StorageFolder accessFolder = ApplicationData.Current.LocalFolder;
            StorageFile merkleFile = await accessFolder.CreateFileAsync(merkleFileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(merkleFile, json);



            /// 
            /// SEND REQUEST
            ///

            try
            {
                // Construct the HttpClient and Uri. This endpoint is for test purposes only.
                HttpClient httpClient = new HttpClient();

                var formContent = new HttpMultipartFormDataContent();
                var dataFileContent = new HttpStreamContent(await selected_file.OpenReadAsync());
                var merkleFileContent = new HttpStreamContent(await merkleFile.OpenReadAsync());
                formContent.Add(dataFileContent, "dataFile", identifier + "." + selected_file.Name.Split(".")[1]);
                formContent.Add(merkleFileContent, "merkleFile", identifier + "." + merkleFile.Name.Split(".")[1]);

                var response = await httpClient.PostAsync(CSS_UPLOAD_ENDPOINT, formContent);

                string responseString = Convert.ToString(response.Content);
                System.Diagnostics.Debug.WriteLine(response.StatusCode);
                System.Diagnostics.Debug.WriteLine(responseString);

            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                System.Diagnostics.Debug.WriteLine(ex);
                file_result_text = "Internal error occured. Please try again!";
                is_passed = false;
            }


            ///
            /// DELETE MERKLE FILE
            /// 
            await merkleFile.DeleteAsync();
            

            /// 
            /// SEND REQUEST
            ///
            // Construct the JSON to post.
            HttpClient BChttpClient = new HttpClient();
            HttpStringContent rqContent = new HttpStringContent(
                "{ \"roothash\": \"" + merkleRootHash + "\" }",
                UnicodeEncoding.Utf8,
                "application/json");

            // Post the JSON and wait for a response.
            HttpResponseMessage httpResponseMessage = await BChttpClient.PostAsync(BC_ENDPOINT, rqContent);

            // Make sure the post succeeded, and write out the response.
            httpResponseMessage.EnsureSuccessStatusCode();
            var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine(httpResponseBody);
            JObject BCresponse = JObject.Parse(httpResponseBody);
            string address = BCresponse.GetValue("yourAddress").ToString();

            System.Diagnostics.Debug.WriteLine(address);
            JObject metadata = JObject.Parse(json);
            metadata["bcAddress"] = address;
            metadata["FileIdentifier"] = identifier + "." + selected_file.Name.Split(".")[1];

            /// 
            /// WRITE METADATA
            ///

            StorageFile metadataFile = await accessFolder.GetFileAsync("metadata.txt");
            await FileIO.AppendTextAsync(metadataFile, JsonConvert.SerializeObject(metadata) + "\n");


            is_passed = true;

        }



        private async void process_verification()
        {
            int file_index =  Int32.Parse(selected_shard);
            string selected_file_verification = metadata[file_index];

            System.Diagnostics.Debug.WriteLine(selected_file_verification);
            JObject verification_info = JObject.Parse(selected_file_verification);
            List<JToken> all_shards = verification_info.GetValue("Leaves").ToList();

            Random r = new Random();
            int shard_index = r.Next(0, all_shards.Count());

            FileVerification vefirication = new FileVerification(
                EMAIL,
                verification_info.GetValue("bcAddress").ToString(),
                verification_info.GetValue("FileIdentifier").ToString(),
                all_shards[shard_index].ToString(),
                shard_index
                );

            String parameter = JsonConvert.SerializeObject(vefirication);

            System.Diagnostics.Debug.WriteLine(parameter);

            /// 
            /// SEND REQUEST
            ///
            // Construct the JSON to post.
            HttpClient httpClient = new HttpClient();
            HttpStringContent rqContent = new HttpStringContent(
                parameter,
                UnicodeEncoding.Utf8,
                "application/json");

            // Post the JSON and wait for a response.
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(CSS_VERIFY_ENDPOINT, rqContent);
            try
            {
                // Make sure the post succeeded, and write out the response.
                httpResponseMessage.EnsureSuccessStatusCode();
                var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(httpResponseBody);

            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                System.Diagnostics.Debug.WriteLine(ex);
            }




            //HttpResponseMessage httpResponse = new HttpResponseMessage();
            //string httpResponseBody = "";

            //try
            //{
            //    //Send the GET request
            //    httpResponse = await httpClient.GetAsync(CSS_VERIFY_ENDPOINT);
            //    httpResponse.EnsureSuccessStatusCode();
            //    httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            //}
            //catch (Exception ex)
            //{
            //    httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            //}

        }


        private void reload_shards()
        {
            StorageFolder accessFolder = ApplicationData.Current.LocalFolder;
            System.Diagnostics.Debug.WriteLine(accessFolder.Path);
            StorageFile metadataFile = accessFolder.GetFileAsync("metadata.txt").AsTask().Result;
            metadata = FileIO.ReadLinesAsync(metadataFile).AsTask().Result;
            list_shards.Clear();

            for(int i=0; i<metadata.Count(); i++)
            {
                JObject json = JObject.Parse(metadata[i]);
                string file_name = json.GetValue("FileIdentifier").ToString();
                list_shards.Add(new FileSelection(file_name, i));
            }
        }
    }
}
