using CloudflareAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CloudflareAPI.Controllers
{
    public class HomeController : Controller
    {
        private const string apiKey = "YOUR API KEY";
        private const string email = "YOUR CLOUDFLARE REGISTERED ADDRESS";
        private const string zoneId = "YOUR ZONE ID";

        public ActionResult IPAddresses()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> IPAddresses(IPAddressModel model)
        {
            // Check if model state is valid
            if (ModelState.IsValid)
            {
                // Split IP addresses string into an array
                string[] ipAddressesToBlock = model.IPAddress.Split(',');
                // Call method to block IP addresses
                await BlockIpAddress(apiKey, email, zoneId, ipAddressesToBlock);
            }
            // Return the view
            return View();
        }

        // Method to block IP addresses using Cloudflare API
        private async Task BlockIpAddress(string apiKey, string email, string zoneId, string[] ipAddressesToBlock)
        {
            // Initialize HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                // Lists to store IP addresses' statuses
                List<string> AlreadyBlockedIpAddress = new List<string>();
                List<string> ErrorIpAddress = new List<string>();
                List<string> BlockedIpAddress = new List<string>();

                // Iterate through each IP address to block
                foreach (var ipAddress in ipAddressesToBlock)
                {
                    // Check if the IP address is already blocked
                    var IsBlocked = await IsIpAddressBlocked(ipAddress);

                    // If IP address is not blocked
                    if (!IsBlocked.isBlocked)
                    {
                        // Checking if the IP address needs to be updated
                        if (IsBlocked.requireupdate)
                        {
                            await UpdateIpAddressRule(ipAddress, IsBlocked.jsonResponse.result.Select(a => a.id).FirstOrDefault());
                            BlockedIpAddress.Add(ipAddress);
                        }
                        else
                        {
                            // If IP address needs to be blocked
                            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://api.cloudflare.com/client/v4/zones/{zoneId}/firewall/access_rules/rules");

                            // Add Cloudflare authentication headers
                            request.Headers.Add("X-Auth-Email", email);
                            request.Headers.Add("X-Auth-Key", apiKey);

                            // Create JSON payload to block IP address
                            string jsonPayload = $"{{\"mode\":\"block\",\"configuration\":{{\"target\":\"ip\",\"value\":\"{ipAddress}\"}}}}";
                            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                            // Send request to Cloudflare API
                            HttpResponseMessage response = await httpClient.SendAsync(request);
                            response.EnsureSuccessStatusCode();

                            // Check response status code
                            int code = Convert.ToInt16(response.StatusCode);
                            if (code != 200)
                            {
                                ErrorIpAddress.Add(ipAddress);
                            }
                            else if (code == 200)
                            {
                                BlockedIpAddress.Add(ipAddress);
                            }
                        }
                    }
                    else
                    {
                        // If IP address is already blocked
                        AlreadyBlockedIpAddress.Add(ipAddress.ToString());
                    }
                }

                // Check if some IPs were already blocked
                if (AlreadyBlockedIpAddress.Count > 0)
                {
                    // Prepare message for IPs already blocked
                    string alreadyBlockedMessage = string.Join(", ", AlreadyBlockedIpAddress);
                    ViewBag.ErrorMessage = "These IPs were already blocked: " + alreadyBlockedMessage;
                }

                // Check if some IPs encountered errors during blocking
                if (ErrorIpAddress.Count > 0)
                {
                    // Prepare message for invalid IPs
                    string errorIpMessage = string.Join(", ", ErrorIpAddress);
                    ViewBag.ErrorMessage += "Invalid IP Addresses: " + errorIpMessage;
                }

                // Check if IPs were successfully blocked
                if (BlockedIpAddress.Count > 0)
                {
                    // Prepare message for successfully blocked IPs
                    string blockedIpMessage = string.Join(", ", BlockedIpAddress);
                    ViewBag.SuccessMessage = "These IP Addresses were blocked Successfully: " + blockedIpMessage;
                }

                // Check if all IPs were successfully blocked
                if (response.IsSuccessStatusCode && AlreadyBlockedIpAddress.Count == 0 && ErrorIpAddress.Count == 0)
                {
                    // Set success message in ViewBag
                    ViewBag.SuccessMessage = "All the given IPs are blocked successfully.";
                }
            }
        }

        // Method to check if IP address is blocked using Cloudflare API
        private async Task<IsIpBlocked> IsIpAddressBlocked(string ipAddress, string email, string apiKey)
        {
            // Initialize HttpClient
            using (HttpClient httpClient = new HttpClient())
            {
                // Initialize result object
                IsIpBlocked result = new IsIpBlocked();

                // Create request to fetch IP address block status
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"https://api.cloudflare.com/client/v4/zones/5a9951cc305ce86d143aa747dbd943f9/firewall/access_rules/rules?configuration.value=" + ipAddress);

                // Add Cloudflare authentication headers
                request.Headers.Add("X-Auth-Email", email);
                request.Headers.Add("X-Auth-Key", apiKey);

                // Send request to Cloudflare API
                HttpResponseMessage response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Read response JSON
                string resultJson = await response.Content.ReadAsStringAsync();

                // Parse JSON response
                JObject responseObject = JObject.Parse(resultJson);

                // Deserialize JSON into custom class
                Root jsonResponse = JsonConvert.DeserializeObject<Root>(resultJson);

                // Check if IP address is already blocked
                bool alreadyBlocked = jsonResponse.result.Any(x => x.mode.ToLower() == "block");
                bool ipAllowed = jsonResponse.result.Any(x => x.mode.ToLower() == "whitelist");

                // Determine if IP address needs update
                result.requireupdate = ipAllowed;
                result.isBlocked = alreadyBlocked;
                result.jsonResponse = jsonResponse;

                return result;
            }
        }

        // Method to update IP address rule in Cloudflare firewall
        public async Task<bool> UpdateIpAddressRule(string ipAddress, string identifier, string email, string apiKey)
        {
            try
            {
                // Initialize HttpClient
                using (HttpClient httpClient = new HttpClient())
                {
                    // Create request to update IP address rule
                    HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://api.cloudflare.com/client/v4/zones/5a9951cc305ce86d143aa747dbd943f9/firewall/access_rules/rules/" + identifier);

                    // Add Cloudflare authentication headers
                    request.Headers.Add("X-Auth-Email", email);
                    request.Headers.Add("X-Auth-Key", apiKey);

                    // Build the JSON payload
                    string jsonPayload = $"{{\"mode\":\"block\"}}";
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Send request to Cloudflare API
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    // Check if the update was successful
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public class IsIpBlocked
        {
            public bool isBlocked { get; set; }
            public bool requireupdate { get; set; }
            public Root jsonResponse { get; set; }
        }

        public class Configuration
        {
            public string target { get; set; }
            public string value { get; set; }
        }

        public class Result
        {
            // Properties for IP address rule result
            public string id { get; set; }
            public bool paused { get; set; }
            public DateTime modified_on { get; set; }
            public List<string> allowed_modes { get; set; }
            public string mode { get; set; }
            public string notes { get; set; }
            public Configuration configuration { get; set; }
            public Scope scope { get; set; }
            public string created_on { get; set; }
        }

        public class ResultInfo
        {
            public int page { get; set; }
            public int per_page { get; set; }
            public int count { get; set; }
            public int total_count { get; set; }
            public int total_pages { get; set; }
        }

        public class Root
        {
            public List<Result> result { get; set; }
            public bool success { get; set; }
            public List<object> errors { get; set; }
            public List<object> messages { get; set; }
            public ResultInfo result_info { get; set; }
        }

        public class Scope
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

    }
}