using memoryGame.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Windows.Forms;

namespace memoryGame
{
    public partial class ChoosePartner : Form
    {

        List<User> allUsers = new List<User>();
        List<User> partners = new List<User>();
        string whoChose = "";//the player who invites the partner
        public ChoosePartner()
        {
            allUsers = GetUsersList();
            InitializeComponent();
        }

        private void ChoosePartner_Load(object sender, EventArgs e)
        {
            partners = allUsers.Where(p => p.UserName != GlobalProp.CurrentUser.UserName).ToList();
            dataGridView_partnerList.DataSource = partners.Select(c => new { c.UserName, c.Age }).ToList();
        }

        private void dataGridView_partnerList_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            User partner = partners[e.RowIndex];
            whoChose = GlobalProp.CurrentUser.UserName;
            //Request via put method to choose partner
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create($@"http://localhost:57034/ChoosePatner/{partner.UserName}");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "PUT";
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    dynamic currentUserName = GlobalProp.CurrentUser.UserName;
                    string currentUserNameString = Newtonsoft.Json.JsonConvert.SerializeObject(currentUserName, Formatting.None);
                    streamWriter.Write(currentUserNameString);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                //Get response
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //Read response
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream(), ASCIIEncoding.ASCII))
                {
                    string result = streamReader.ReadToEnd();
                    //If request succeeded
                    if (result.Contains("true"))
                    {
                        //Switch the screen to "startGame"
                        StartGame startGame = new StartGame(partner);
                        startGame.Show();
                        Close();
                    }
                    //Print the matching error
                    else MessageBox.Show(result);

                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Every 1000 ms.:

            //Update the shown userslist
            allUsers = GetUsersList();
            partners = allUsers.Where(p => p.UserName != GlobalProp.CurrentUser.UserName).ToList();
            dataGridView_partnerList.DataSource = partners.Select(c => new { c.UserName, c.Age }).ToList();

            User user = GetUser(GlobalProp.CurrentUser.UserName);

            //If anyone chose me:
            if (user.PartnerUserName != null)
            {
                GlobalProp.CurrentUser = user;
                User partner = GetUser(GlobalProp.CurrentUser.PartnerUserName);
                partner.PartnerUserName = user.UserName;
                if (whoChose != GlobalProp.CurrentUser.UserName)
                {
                    StartGame startGame = new StartGame(partner);
                    startGame.Show();
                    Close();
                }
            }
        }

        public List<User> GetUsersList()
        {
            //Get request-GetUsers
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(@"http://localhost:57034/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("GetUsers").Result;
            if (response.IsSuccessStatusCode)
            {
                var usersJson = response.Content.ReadAsStringAsync().Result;
                allUsers = JsonConvert.DeserializeObject<List<User>>(usersJson);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return allUsers;
        }

        public User GetUser(string username)
        {
            //Get request-GetCurrentUser
            User user = new User();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(@"http://localhost:57034/");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync($"GetCurrentUser/{username}").Result;
            if (response.IsSuccessStatusCode)
            {
                var userJson = response.Content.ReadAsStringAsync().Result;
                user = JsonConvert.DeserializeObject<User>(userJson);
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
            return user;
        }
    }

}

