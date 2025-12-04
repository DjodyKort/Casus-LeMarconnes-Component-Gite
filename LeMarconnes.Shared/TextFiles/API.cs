// ======== Imports ========
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConsoleApp1.Models;

public class API {
    // ==== Properties ====.
    private string? apiURL = "";

    // ==== Getters ====
    public string VerkrijgAPIURL() {
        return this.apiURL;
    }

    // ==== Methods ====
    public async Task<string> VerkrijgDataVanAPI(string apiUrl, int id=0) {
        string url = id == 0 ? apiUrl : $"{apiUrl}/{id.ToString()}";

        using (HttpClient client = new HttpClient()) {
            try {
                // Console.WriteLine($"Sending GET request to {apiUrl}");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"Received response: {responseBody}");
                return responseBody;
            }
            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound) {
                Console.WriteLine("Error 404: Resource not found.");
                return null;
            }
            catch (HttpRequestException e) {
                Console.WriteLine($"Een error heeft opgetreden: {e.Message}");
                return null;
            }
            catch (JsonException e) {
                Console.WriteLine($"JSON deserialization error: {e.Message}");
                return null;
            }
        }
    }

    public async Task<bool> PostDataNaarAPI(string data) {
        using (HttpClient client = new HttpClient()) {
            try {
                // Console.WriteLine($"Sending POST request to {endpoint} with data: {data}");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{this.apiURL}");
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"Received response: {responseBody}");

                if (response.IsSuccessStatusCode) { return true; }
                return false;
            }
            catch (HttpRequestException e) {
                Console.WriteLine($"Een error heeft opgetreden: {e.Message}");
                return false;
            }
        }
    }

    public async Task<bool> PUTDataNaarAPI(string endpoint, string data) {
        using (HttpClient client = new HttpClient()) {
            try {
                // Console.WriteLine($"Sending PUT request to {endpoint} with data: {data}");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, $"{this.apiURL}/{endpoint}");
                request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"Received response: {responseBody}");

                if (response.IsSuccessStatusCode) { return true; }
                return false;
            }
            catch (HttpRequestException e) {
                Console.WriteLine($"Een error heeft opgetreden: {e.Message}");
                return false;
            }
        }
    }

    public async Task<bool> DeleteNaarAPI(string endpoint) {
        using (HttpClient client = new HttpClient()) {
            try {
                // Console.WriteLine($"Sending PUT request to {endpoint} with data: {data}");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, $"{this.apiURL}/{endpoint}");
                HttpResponseMessage response = await client.SendAsync(request);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) { return true; }
                return false;
            }
            catch (HttpRequestException e) {
                Console.WriteLine($"Een error heeft opgetreden: {e.Message}");
                return false;
            }
        }
    }

    // ==== Constructor ====
    public API(string apiURL) {
        this.apiURL = apiURL;
    }
}