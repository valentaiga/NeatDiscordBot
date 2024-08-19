using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using NeatDiscordBot.Discord.Services;

namespace NeatDiscordBot.Discord.Features.UserTracker;

public class VoiceActivityTracker : IFeature
{
    public string FeatureName => "voice_activity_tracker";

    private readonly INeatClient _neatClient;
    private readonly IUserRepository _userRepository;
    private readonly NeatClientConfig _config;

    private readonly ConcurrentDictionary<ulong, long> _trackedUsers;
    private readonly ConcurrentQueue<UserTimeInVoice> _usersVoiceTime;

    public VoiceActivityTracker(
        INeatClient neatClient, 
        IOptions<NeatClientConfig> config,
        IUserRepository userRepository)
    {
        _neatClient = neatClient;
        _userRepository = userRepository;
        _config = config.Value;
        _trackedUsers = new(10, 100);
        _usersVoiceTime = new();
    }
    
    public void Enable()
    {
        _neatClient.BotClient.UserVoiceStateUpdated += UpdateUserVoiceActivityAsync;
    }

    private async Task UpdateUserVoiceActivityAsync(SocketUser socketUser, SocketVoiceState oldVoiceState, SocketVoiceState newVoiceState)
    {
        if (socketUser.IsBot || socketUser is not SocketGuildUser user)
            return;

        // user muted/unmuted in same vc
        if (oldVoiceState.VoiceChannel?.Id == newVoiceState.VoiceChannel?.Id)
        {
            var vc = newVoiceState.VoiceChannel!;

            if (oldVoiceState.IsMuted() && newVoiceState.IsMuted()) { } // ignore
            else
            // unmuted
            if (oldVoiceState.IsMuted() && !newVoiceState.IsMuted())
            {
                var usersInVc = GetNotMutedUsers(vc);
                if (usersInVc.Length == 2)
                {
                    foreach (var u in usersInVc)
                        BeginTrack(u.Id);
                }

                if (usersInVc.Length == 2)
                    BeginTrack(user.Id);
            }
            else
            // muted
            if (!oldVoiceState.IsMuted() && newVoiceState.IsMuted())
            {
                var usersInVc = GetNotMutedUsers(vc);
                EndTrack(user.Guild.Id, user.Id);
                if (usersInVc.Length == 1)
                    EndTrack(user.Guild.Id, usersInVc[0].Id);
            }

            await UpdateUserActivityAsync();
            return;
        }

        // user joined/left vc
        if (newVoiceState.VoiceChannel is not null)
        {
            var usersInVc = GetNotMutedUsers(newVoiceState.VoiceChannel);
            if (usersInVc.Length >= 2)
            {
                foreach (var u in usersInVc)
                    BeginTrack(u.Id);
            }
        }

        if (oldVoiceState.VoiceChannel is not null)
        {
            var usersInVc = GetNotMutedUsers(oldVoiceState.VoiceChannel);
            if (usersInVc.Length == 1)
            {
                EndTrack(user.Guild.Id, usersInVc[0].Id);
            }
        }
        
        await UpdateUserActivityAsync();
    }
    
    private void BeginTrack(ulong userId) => 
        _trackedUsers.AddOrUpdate(userId,
            _ => Stopwatch.GetTimestamp(),
            (_, dt) => dt);

    private void EndTrack(ulong guildId, ulong userId)
    {
        if (!_trackedUsers.TryRemove(userId, out var startVc))
            return;

        var trackedTimeInVc = Stopwatch.GetElapsedTime(startVc);

        if (trackedTimeInVc < _config.MinimalVoiceActivity) return;
        _usersVoiceTime.Enqueue(new UserTimeInVoice(guildId, userId, trackedTimeInVc));
    }

    private async ValueTask UpdateUserActivityAsync()
    {
        while (_usersVoiceTime.TryDequeue(out var userActivity))
        {
            var user = await _userRepository.GetOrCreateUserAsync(userActivity.GuildId, userActivity.UserId);
            user.TotalVoiceActivity += userActivity.TimeInVc;
            await _userRepository.SaveUserAsync(user);
        }
    }

    private static ImmutableArray<SocketGuildUser> GetNotMutedUsers(SocketVoiceChannel vc) =>
        vc.ConnectedUsers.Where(user => !user.IsBot && !user.IsMuted()).ToImmutableArray();

    private struct UserTimeInVoice
    {
        public ulong GuildId { get; }
        public ulong UserId { get; }
        public TimeSpan TimeInVc { get; }

        public UserTimeInVoice(ulong guildId, ulong userId, TimeSpan timeInVc)
        {
            GuildId = guildId;
            UserId = userId;
            TimeInVc = timeInVc;
        }
    }
}

