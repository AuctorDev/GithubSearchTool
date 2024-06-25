using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GithubSearchTool
{
    class Program
    {
        // Github Api URL
        private static readonly string GitHubApiUrl = "https://api.github.com/search/code";
        // Github Token
        private static readonly string GitHubToken = "GITHUB PAT TOKEN HERE"; // Replace with your GitHub token

        // Main Entry
        static async Task Main(string[] args)
        {
            // Heracy
            restart:

            // Query string
            string query = string.Empty;


            // Query String option one
            query = "something in:file";


            // Query string option 2
            // query = Console.ReadLine();
            

            // Run the search with query
            await SearchGitHubCode(query);
            Console.WriteLine("Search completed: Press any key to run a new query");
            Console.ReadKey();

            // GOTO yes i know, herracy, live with it
            goto restart;
        }

        private static async Task SearchGitHubCode(string query)
        {
            using (HttpClient client = new HttpClient())
            {
                // Define some client options
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));

                // Add the token to the header for authorization
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", GitHubToken);

                // Set the current page to 1
                int currentPage = 1;

                // hasMoreResults 
                bool hasMoreResults = true;

                // Some loop magic, no i dont know what i was thinking either
                while (hasMoreResults)
                {
                    // Construct the URL Address for API
                    string url = $"{GitHubApiUrl}?q={Uri.EscapeDataString(query)}&page={currentPage}";
                    
                    // Send the request and get the responce
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Only need to process success so lets do that
                    if (response.IsSuccessStatusCode)
                    {
                        // Extract the response as a string
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        // Convert to a searchResult object, make life easier
                        JObject searchResult = JObject.Parse(jsonResponse);

                        // For every item returned in the search results
                        foreach (var item in searchResult["items"])
                        {
                            string repo = item["repository"]["full_name"].ToString();
                            string path = item["path"].ToString();
                            string fileUrl = item["html_url"].ToString();
                            Console.WriteLine($"Repository: {repo}");
                            Console.WriteLine($"File: {item["name"]}");
                            Console.WriteLine($"Path: {path}");
                            Console.WriteLine($"URL: {fileUrl}");
                            Console.WriteLine(new string('-', 80));

                            
                        }
                        // If the searchResult Has values, pagination is needed IE page 1 was returned
                        // indicating that page 2 is valid.
                        hasMoreResults = searchResult["items"].HasValues;

                        // Increment the current page
                        currentPage++;
                    }

                    // We dont really care why it failed but it did, spit out the error
                    // and end the processing loop
                    else
                    {
                        Console.WriteLine($"Error: {response.ReasonPhrase}");
                        hasMoreResults = false;
                    }
                }
            }
        }
        
        // Decode Base64 data - Not needed but was used for some search types.
        private static string DecodeBase64(string encodedData)
        {
            byte[] data = Convert.FromBase64String(encodedData);
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }
}
