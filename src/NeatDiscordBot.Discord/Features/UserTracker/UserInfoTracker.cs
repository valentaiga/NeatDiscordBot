using Discord.WebSocket;
using NeatDiscordBot.Discord.Entities;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.UserTracker;

public class UserInfoTracker : IFeature
{
    public string FeatureName => "user_info_tracker";

    private readonly INeatClient _neatClient;
    private readonly IUserRepository _userRepository;

    public UserInfoTracker(INeatClient neatClient, IUserRepository userRepository)
    {
        _neatClient = neatClient;
        _userRepository = userRepository;
    }

    public void Enable()
    {
        _neatClient.BotClient.UserJoined += CreateUserInDb;
        _neatClient.BotClient.UserUpdated += UpdateUserInDb;
    }

    private async Task CreateUserInDb(SocketGuildUser dsUser)
    {
        if (dsUser.IsBot) return;
        var user = await _userRepository.GetOrCreateUserAsync(dsUser.Guild.Id, dsUser.Id);
        Update(user, dsUser);
        await _userRepository.SaveUserAsync(user);
    }

    private async Task UpdateUserInDb(SocketUser beforeUser, SocketUser afterUser)
    {
        if (afterUser.IsBot) return;
        if (!afterUser.IsSocketGuildUser(out var dsUser)) return;

        var user = new User(dsUser.Id, dsUser.Guild.Id);
        Update(user, dsUser);
        await _userRepository.SaveUserAsync(user);
    }

    private static void Update(User dbUser, SocketGuildUser user)
    {
        dbUser.Nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
    }
}