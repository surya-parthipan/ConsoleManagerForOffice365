using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using Microsoft.Graph;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConsoleUsers
{
    public class Value
    {
        public string id { get; set; }
        public List<string> businessPhones { get; set; }
        public string displayName { get; set; }
        public string givenName { get; set; }
        public object jobTitle { get; set; }
        public string mail { get; set; }
        public string mobilePhone { get; set; }
        public object officeLocation { get; set; }
        public string preferredLanguage { get; set; }
        public string surname { get; set; }
        public string userPrincipalName { get; set; }
    }

    public class RootObject
    {
        public List<Value> value { get; set; }
    }

    

    class Program
    {
        private const string clientId = "2a0dc5ff-3a17-4fb6-8dfb-bd6a179434d8";
        private const string addInstance = "https://login.microsoftonline.com/{0}";
        private const string tenant = "codeninja2.onmicrosoft.com";
        private const string resource = "https://graph.microsoft.com";
        private const string appKey = "DAUzCJdB4Tsn/TGj15bwpE+MVZNfVQABWnlxyclpmbA=";
        static string authority = String.Format(CultureInfo.InvariantCulture,addInstance,tenant);

        private static HttpClient httpClient = new HttpClient();
        private static AuthenticationContext context = null;
        private static ClientCredential credentials = null;

        static void Main(string[] args)
        {
            string username, password = "",s;
            RootObject op;
            Console.Write("Enter the username: ");
            username = Console.ReadLine();
            string pswd = "zoho123";
            Console.Write("enter the passcodes: ");
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password.Substring(0, (password.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            } while (key.Key != ConsoleKey.Enter);
            if (pswd == password)
            {
                Console.Write($"\nwelcome {username}\n");

                context = new AuthenticationContext(authority);
                credentials = new ClientCredential(clientId, appKey);

                Task<string> token = GetToken();
                token.Wait();
                //Console.WriteLine(token.Result+"\n");

                int ch;
                Console.WriteLine("1. List Users");
                Console.WriteLine("2. Create Users");
                Console.WriteLine("3. Search a Users");
                Console.WriteLine("4. Groups");
                Console.WriteLine("5. Group Members");
                Console.WriteLine("10. Exit");
                Console.WriteLine("\nEnter your Choice:  ");
                ch = Convert.ToInt16(Console.ReadLine());
                switch (ch)
                {
                    case 1 :
                        Task<string> users = GetUsers(token.Result);
                        users.Wait();
                        Console.WriteLine("All the user in the Application");
                        s = users.Result;
                        op = new JavaScriptSerializer().Deserialize<RootObject>(s);

                        foreach (var item in op.value)
                        {
                            Console.WriteLine("id: {0}, name: {1}", item.id, item.displayName);
                        }
                        break;
                    case 2:
                        Console.WriteLine("Enter the name to create User: ");
                        string mailId = Console.ReadLine();
                        Task<string> user = NewUsers(token.Result,mailId);
                        break;
                    case 3:
                        Task<string> mailuser = GetUsersByMail(token.Result);
                        mailuser.Wait();
                        s = mailuser.Result;
                        //Console.WriteLine(s);
                        break;
                    case 4:
                        Task<string> group = GetGroup(token.Result);
                        group.Wait();
                        Console.WriteLine("\n");
                        Console.WriteLine("All the Groups in the Application");
                        s = group.Result;
                        op = new JavaScriptSerializer().Deserialize<RootObject>(s);

                        foreach (var item in op.value)
                        {
                            Console.WriteLine("id: {0}, name: {1}", item.id, item.displayName);
                        }
                        break;
                    case 5:
                        Task<string> groupMembers = GetGroup(token.Result);
                        groupMembers.Wait();
                        Console.WriteLine("\n");
                        Console.WriteLine("All the Groups in the Application");
                        s = groupMembers.Result;
                        op = new JavaScriptSerializer().Deserialize<RootObject>(s);

                        foreach (var item in op.value)
                        {
                            Console.WriteLine("id: {0}, name: {1}", item.id, item.displayName);
                        }
                        Console.WriteLine("Enter the Group Name: ");
                        String groupName = Console.ReadLine();
                        foreach (var item in op.value)
                        {
                            if (groupName.Equals(item.displayName))
                            {
                                Task<string> groupMember = GetGroupMembers(token.Result,item.id);
                                groupMember.Wait();
                                Console.WriteLine("\n");
                                Console.WriteLine("All the Members in the Group");
                                s = groupMember.Result;
                                op = new JavaScriptSerializer().Deserialize<RootObject>(s);

                                foreach (var i in op.value)
                                {
                                    Console.WriteLine("id: {0}, name: {1}", i.id, i.displayName);
                                }
                            }
                        }
                        
                        break;


                } while (ch < 10) ;


                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Wrong username or Password");
            }
        }

        private static async Task<string> GetUsers(string tokenValue)
        {
            string users = null;
            var uri = "https://graph.microsoft.com/v1.0/users";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",tokenValue);
            var getResult = await httpClient.GetAsync(uri);
            if(getResult.Content != null)
            {
                users = await getResult.Content.ReadAsStringAsync();
            }

            return users;
        }

        private static async Task<string> NewUsers(string tokenValue,string mailId)
        {
            string users = "";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            string userValue =@"{
                                  'accountEnabled': true,
                                  'city': 'seattle',
                                  'country': 'United States',
                                  'department': 'Sales & Marketing',
                                  'displayName': 'hello new',
                                  'givenName': 'Test',
                                  'jobTitle': 'Marketing Director',
                                  'mailNickname': 'hello',
                                  'passwordPolicies': 'DisablePasswordExpiration',
                                  'passwordProfile': {
                                                'password': '1111@test',
                                    'forceChangePasswordNextSignIn': false
                                  },
                                  'officeLocation': '131/110',
                                  'postalCode': '98052',
                                  'preferredLanguage': 'en-US',
                                  'state': 'WA',
                                  'streetAddress': '9256 Towne Center Dr., Suite 400',
                                  'surname': '3',
                                  'mobilePhone': '+1 206 555 0110',
                                  'usageLocation': 'US',
                                  'userPrincipalName': 'hellonew@codeninja2.onmicrosoft.com'
                                }";
            
            var httpContent = new StringContent(userValue, Encoding.GetEncoding("utf-8"), "application/json");
            var response = await httpClient.PostAsync("https://graph.microsoft.com/v1.0/users", httpContent);

            users = await response.Content.ReadAsStringAsync();
            Console.WriteLine("user is created and added to the system");
            dynamic parsedJson = JsonConvert.DeserializeObject(users);
            Console.WriteLine(JsonConvert.SerializeObject(parsedJson, Formatting.Indented));
            return users;
 
        }

        private static async Task<string> GetUsersByMail(string tokenValue)
        {
            string users = null;
            var uri = "https://graph.microsoft.com/v1.0/users/";
            string mailid = "";
            Console.WriteLine("Enter the mail id:");
            mailid = Console.ReadLine();
            uri += mailid;
            uri += "@codeninja2.onmicrosoft.com";
            Console.WriteLine("Mail id:    " + mailid+"@codeninja2.onmicrosoft.com");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            var getResult = await httpClient.GetAsync(uri);
            if (getResult.Content != null)
            {
                users = await getResult.Content.ReadAsStringAsync();
            }

            return users;
        }

        private static async Task<string> GetGroup(string tokenValue)
        {
            string users = null;
            var uri = "https://graph.microsoft.com/v1.0/groups";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            var getResult = await httpClient.GetAsync(uri);
            if (getResult.Content != null)
            {
                users = await getResult.Content.ReadAsStringAsync();
            }

            return users;
        }

        private static async Task<string> GetGroupMembers(string tokenValue,string id)
        {
            string users = null;
            var uri = "https://graph.microsoft.com/v1.0/groups/"+id+"/members";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
            var getResult = await httpClient.GetAsync(uri);
            if (getResult.Content != null)
            {
                users = await getResult.Content.ReadAsStringAsync();
            }

            return users;
        }

        private static async Task<string> GetToken()
        {
            AuthenticationResult tokenValue = null;
            string token = null;
            tokenValue = await context.AcquireTokenAsync(resource,credentials);
            token = tokenValue.AccessToken;

            return token;
        }
    }
}
