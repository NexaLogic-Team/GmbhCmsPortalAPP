namespace GmbhCmsPortalApp.Components.Service;

// Services/UserProfileState.cs
public class UserProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfileImage { get; set; }
}

public class UserProfileState
{
    private readonly HttpClient _http;
    
    // Static / Singleton logic သို့မဟုတ် Scoped Instance
    public UserProfileDto? Profile { get; private set; }
    public bool IsLoaded => Profile != null;

    public event Action? OnChange;

    public UserProfileState(HttpClient http)
    {
        _http = http;
    }

    public async Task InitializeAsync(bool forceRefresh = false)
    {
        if (Profile != null && !forceRefresh) return;

        try
        {
            var response = await _http.GetAsync("api/cms/auth/profile");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Normal behavior on login screen - swallow or log gracefully
                Profile = null;
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                Profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
                NotifyStateChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching profile: {ex.Message}");
        }
    }

    public void ClearCache()
    {
        Profile = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}