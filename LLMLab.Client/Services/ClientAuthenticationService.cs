using System.Net.Http.Json;
using LLMLab.Dtos.User;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LLMLab.Client.Services;

public class ClientAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly AppsettingsService _appSettingsService;
    private readonly StorageService _storageService;
    private readonly ThreadSyncService _threadSyncService;

    public ClientAuthenticationService(HttpClient httpClient, AppsettingsService appSettingsService, NavigationManager navigationManager, StorageService storageService, ThreadSyncService threadSyncService)
    {
        _httpClient = httpClient;
        _appSettingsService = appSettingsService;
        _navigationManager = navigationManager;
        _storageService = storageService;
        _threadSyncService = threadSyncService;
    }

    public async Task<UserDto?> GetCurrentUser(bool forceLogin = true)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/authentication/user");
            
            if (!response.IsSuccessStatusCode)
            {
                await UserIsLoggedOut(forceLogin);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching current user (likely not logged in): {ex.Message}");
            await UserIsLoggedOut(forceLogin);
            return null;
        }
    }

    private async Task UserIsLoggedOut(bool forceLogin)
    {
        var keys = await _storageService.GetKeysAsync();
        foreach (var key in keys)
        {
            await _storageService.RemoveObjectAsync(key);
        }
        _threadSyncService.ClearMemoryCache();
        if (forceLogin)
        {
            Console.Write("Forcing Login");
            _navigationManager.NavigateTo(_appSettingsService.ServerUrl + "/Account/Login", true);
        }
    }
}