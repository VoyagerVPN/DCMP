using dc;
using dc.steam;
using DCMP.Utils;

namespace DCMP.Utils;

public class SteamHelper
{
    public static string GetPlayerName()
    {
        try
        {
            if (Api.Class.active && Api.Class.isUserLoggedIn.Invoke())
            {
                var user = Api.Class.getUser.Invoke();
                if (user != null)
                {
                    string name = user.get_name().ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Logger.Warning($"[Steam] Failed to get player name: {ex.Message}");
        }
        return "GuestPlayer";
    }
}
