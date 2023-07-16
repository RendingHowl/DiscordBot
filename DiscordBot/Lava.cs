using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;

namespace DiscordBot
{
    static class Lava
    {
        static CommandContext ctx = null;

        static public void CreateCtx(CommandContext _ctx)
        {
            if (ctx == null)
            {
                ctx = _ctx;
            }
        }

        static public void ResetCtx()
        {
            ctx = null;
        }

        static public async Task<LavalinkNodeConnection> Node()
        {
            //Канал юзера
            if (ctx.Member.VoiceState == null)
            {
                await ctx.RespondAsync("Ты где ебать? Зайди в канал");
                return null;
            };

            var _user_vc = ctx.Member.VoiceState.Channel;

            if (_user_vc.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Я в этот канал не полезу");
                return null;
            };

            //Клиент лавы
            var _client = ctx.Client.GetLavalink();

            if (!_client.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Клиент лавы в говне! :poop:");
                return null;
            };

            //Узел лавы
            var _node = _client.ConnectedNodes.Values.First();
            await _node.ConnectAsync(_user_vc);
            return _node;
        }

        static public async Task<LavalinkGuildConnection> Connect(LavalinkNodeConnection _node)
        {
            //Коннект лавы
            var _conn = _node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (_conn == null)
            {
                await ctx.Channel.SendMessageAsync("Коннект лавы в говне! :poop:");
                return null;
            };

            return _conn;
        }
    }
}
