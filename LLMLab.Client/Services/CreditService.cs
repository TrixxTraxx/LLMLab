using System.Net.Http.Json;
using LLMLab.Dtos.User;
using LLMLab.Client.Models;

namespace LLMLab.Client.Services;

public class CreditService
{
    private readonly HttpClient _httpClient;
    private readonly AppsettingsService _appsettingsService;

    public CreditService(HttpClient httpClient, AppsettingsService appsettingsService)
    {
        _httpClient = httpClient;
        _appsettingsService = appsettingsService;
    }

    /// <summary>
    /// Gets the current user's credit balance
    /// Note: This endpoint needs to be implemented on the server
    /// </summary>
    public async Task<int> GetUserCreditBalance()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/credits/balance");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get credit balance: {response.StatusCode}");
                return 0;
            }

            var balance = await response.Content.ReadFromJsonAsync<int>();
            return balance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting credit balance: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets the user's transaction history
    /// Note: This endpoint needs to be implemented on the server
    /// </summary>
    public async Task<List<CreditTransaction>?> GetTransactionHistory(int page = 1, int pageSize = 20)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/credits/transactions?page={page}&pageSize={pageSize}");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get transaction history: {response.StatusCode}");
                return null;
            }

            var transactions = await response.Content.ReadFromJsonAsync<List<CreditTransaction>>();
            return transactions;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting transaction history: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deducts credits from the user's balance (used when consuming API tokens)
    /// Note: This endpoint needs to be implemented on the server
    /// </summary>
    public async Task<bool> DeductCredits(int amount, string reason)
    {
        try
        {
            var deductRequest = new CreditDeductRequest
            {
                Amount = amount,
                Reason = reason
            };

            var response = await _httpClient.PostAsJsonAsync("api/credits/deduct", deductRequest);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deducting credits: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Creates a checkout session by making a POST request to the server
    /// </summary>
    public async Task<string?> CreateCheckoutSession(int credits)
    {
        try
        {
            var response = await _httpClient.PostAsync($"create-checkout-session/{credits}", null);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Redirect || 
                response.StatusCode == System.Net.HttpStatusCode.SeeOther)
            {
                // Get the redirect URL from Location header
                if (response.Headers.Location != null)
                {
                    return response.Headers.Location.ToString();
                }
            }
            
            Console.WriteLine($"Unexpected response status: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating checkout session: {ex.Message}");
            return null;
        }
    }
} 