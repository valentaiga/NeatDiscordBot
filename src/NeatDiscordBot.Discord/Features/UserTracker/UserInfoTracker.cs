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
        _neatClient.BotClient.UserJoined += CreateOrUpdateUserAsync;
        _neatClient.BotClient.UserUpdated += UpdateUserAsync;
        _neatClient.BotClient.UserVoiceStateUpdated += (user, _, _) => CreateOrUpdateUserAsync(user);
        _neatClient.BotClient.UserCommandExecuted += command => CreateOrUpdateUserAsync(command.User);
        _neatClient.BotClient.MessageReceived += message => CreateOrUpdateUserAsync(message.Author);
        _neatClient.BotClient.MessageUpdated += (_, message, _) => CreateOrUpdateUserAsync(message.Author);
    }

    private Task CreateOrUpdateUserAsync(SocketUser dsUser) => dsUser is SocketGuildUser s ? CreateOrUpdateUserAsync(s) : Task.CompletedTask;

    private async Task CreateOrUpdateUserAsync(SocketGuildUser dsUser)
    {
        if (dsUser.IsBot) return;
        var user = await _userRepository.GetOrCreateUserAsync(dsUser.Guild.Id, dsUser.Id);
        Update(user, dsUser);
        await _userRepository.SaveUserAsync(user);
    }

    private async Task UpdateUserAsync(SocketUser beforeUser, SocketUser afterUser)
    {
        if (afterUser.IsBot) return;
        if (afterUser is not SocketGuildUser dsUser) return;

        var user = new User(dsUser.Id, dsUser.Guild.Id);
        Update(user, dsUser);
        await _userRepository.SaveUserAsync(user);
    }

    private static void Update(User dbUser, SocketGuildUser user)
    {
        dbUser.Username = user.Username;
        dbUser.Nickname = string.IsNullOrEmpty(user.Nickname) ? user.Username : user.Nickname;
    }
}