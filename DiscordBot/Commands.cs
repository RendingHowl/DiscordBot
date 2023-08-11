using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Lavalink;

namespace DiscordBot
{
    enum query_types
    {
        str,
        url
    }

    enum embed_types
    {
        first,
        queue
    }

    public class Commandos : BaseCommandModule
    {
        #region Функции

        async Task FunPlay(CommandContext _ctx, query_types _query_type, string _user_query) //Воспроизведение или добавление трека в очередь, по запросу или ссылке
        {
            //Лава
            Lava.CreateCtx(_ctx);
            var _lava_node = Lava.Node().Result;
            var _lava_conn = Lava.Connect(_lava_node).Result;

            //Запрос юзера
            LavalinkLoadResult _lava_search = null;

            switch (_query_type)
            {
                case query_types.str:
                _lava_search = await _lava_node.Rest.GetTracksAsync(_user_query);
                break;

                case query_types.url:
                Uri _user_query_uri = new Uri(_user_query);
                _lava_search = await _lava_node.Rest.GetTracksAsync(_user_query_uri);
                break;
            }
            
            if (_lava_search.LoadResultType == LavalinkLoadResultType.NoMatches
            || _lava_search.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await _ctx.Channel.SendMessageAsync($"Этот запрос в говне! :poop: :point_right: {_user_query}");
                return;
            };
            
            //Воспроизведение трека или добавление в очередь
            var _lava_track = _lava_search.Tracks.First();
            embed_types _embed_type = embed_types.first;

            string _link = "";

            switch (_query_type)
            {
                case query_types.str:
                _link = _lava_track.Uri.ToString();
                break;

                case query_types.url:
                _link = _user_query;
                break;
            }

            if (Queue.IsEmpty)
            {
                Queue.CurrentLink = _link;
                await _lava_conn.PlayAsync(_lava_track);
            }
            else
            {
                _embed_type = embed_types.queue;
            }

            Queue.Add(_lava_track);

            //Сохранение трека в плейлист
            Playlist.Save(_link);

            //Окончание трека
            if (Queue.Create(_ctx))
            {
                _lava_conn.PlaybackFinished += Queue.PlayNext;

                _lava_conn.PlaybackFinished += async (s, e) =>
                {
                    if (!Queue.IsEmpty)
                    {
                        DiscordEmbedBuilder _finish_embed = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Gold,
                            Title = $"Подрубаю {Queue.GetFirst.Title}",
                            Description = $"{Queue.CurrentLink}"
                        };

                        await _ctx.Channel.SendMessageAsync(_finish_embed);
                    }
                };
            }
            
            //Сообщение
            DiscordEmbedBuilder _embed = null;

            switch (_embed_type)
            {
                case embed_types.first:
                switch (_query_type)
                {
                    case query_types.str:
                            _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.MidnightBlue,
                        Title = "Нашел вот такую хуету, проверяй",
                        Description = $"Хуета: {_lava_track.Title} \n" +
                                      $"От ниггера: {_lava_track.Author} \n" +
                                      $"Ссылка в ГУЛАГ: {_lava_track.Uri}"
                    };
                    break;
                    
                    case query_types.url:
                    _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Gold,
                        Title = $"Завожу шарманку по ссылке на {_lava_track.Title}"
                    };
                    break;
                }

                await _ctx.RespondAsync(_embed);
                break;
                
                case embed_types.queue:
                switch (_query_type)
                {
                    case query_types.str:
                    _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.PhthaloBlue,
                        Title = "Закинул в очередь вот такую хуету, проверяй",
                        Description = $"Хуета: {_lava_track.Title} \n" +
                                      $"От ниггера: {_lava_track.Author} \n" +
                                      $"Ссылка в ГУЛАГ: {_lava_track.Uri}"
                    };
                    break;

                    case query_types.url:
                    _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Goldenrod,
                        Title = $"Закинул в очередь {_lava_track.Title}"
                    };
                    break;
                }
                
                await _ctx.RespondAsync(_embed);
                break;
            }
        }

        #endregion

        #region Команды

        [Command("найди")] //Воспроизведение, или добавление в очередь, трека по запросу
        public async Task CmdPlaySearch(CommandContext _ctx, [RemainingText] string user_query)
        {
            await FunPlay(_ctx, query_types.str, user_query);
        }

        [Command("заводи")] //Воспроизведение, или добавление в очередь, трека по ссылке, с добавлением его в плейлист
        public async Task CmdPlayUir(CommandContext _ctx, [RemainingText] string user_query)
        {
            await FunPlay(_ctx, query_types.url, user_query);
        }

        [Command("плейлист")] //Воспроизведение рандомных треков из плейлиста
        public async Task CmdPlayList(CommandContext _ctx)
        {
            //Лава
            Lava.CreateCtx(_ctx);
            var _lava_node = Lava.Node().Result;
            var _lava_conn = Lava.Connect(_lava_node).Result;

            //Активация плейлиста
            if (!Playlist.IsActive)
            {
                embed_types _embed_type = embed_types.first;

                if (!Queue.IsEmpty)
                {
                    _embed_type = embed_types.queue;
                }

                if (Playlist.Activate())
                {
                    //Воспроизведение трека или добавление в очередь
                    var _playlist_query = Playlist.queue_track_list[0];
                    Uri _user_query_uri = new Uri(_playlist_query);
                    var _lava_search = await _lava_node.Rest.GetTracksAsync(_user_query_uri);
                    var _lava_track = _lava_search.Tracks.First();

                    if (Queue.IsEmpty)
                    {
                        Queue.Add(_lava_track);
                        Queue.CurrentLink = _playlist_query;
                        await _lava_conn.PlayAsync(_lava_track);
                    };

                    for (int _i = 1; _i < Playlist.QUEUE_SIZE; ++_i)
                    {
                        _playlist_query = Playlist.queue_track_list[_i];
                        _user_query_uri = new Uri(_playlist_query);
                        _lava_search = await _lava_node.Rest.GetTracksAsync(_user_query_uri);
                        _lava_track = _lava_search.Tracks.First();

                        Queue.Add(_lava_track);
                    }

                    Playlist.IsPlaying = true;

                    //Окончание трека
                    if (Queue.Create(_ctx))
                    {
                        _lava_conn.PlaybackFinished += Queue.PlayNext;

                        _lava_conn.PlaybackFinished += async (s, e) =>
                        {
                            if (!Queue.IsEmpty)
                            {
                                DiscordEmbedBuilder queue_embed = new DiscordEmbedBuilder()
                                {
                                    Color = DiscordColor.Gold,
                                    Title = $"Подрубаю {Queue.GetFirst.Title}",
                                    Description = $"{Queue.CurrentLink}"
                                };

                                await _ctx.Channel.SendMessageAsync(queue_embed);
                            }
                        };
                    }
                }
                else
                {
                    await _ctx.Channel.SendMessageAsync("Плейлист пуст :moyai:");
                    return;
                }

                //Сообщение
                var _embed = new DiscordEmbedBuilder();

                switch (_embed_type)
                {
                    case embed_types.first:
                    _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.HotPink,
                        Title = $"ПОДРУБИЛ {Playlist.QUEUE_SIZE} ТРЕКОВ ИЗ ПЛЕЙЛИСТА :accordion:"
                    };

                    DiscordEmbedBuilder _queue_embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Gold,
                        Title = $"Подрубаю {Queue.GetFirst.Title}",
                        Description = $"{Queue.CurrentLink}"
                    };

                    await _ctx.RespondAsync(_embed);
                    await _ctx.Channel.SendMessageAsync(_queue_embed);
                    break;

                    case embed_types.queue:
                    _embed = new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Brown,
                        Title = $"ЗАКИНУЛ {Playlist.QUEUE_SIZE} ТРЕКОВ ПЛЕЙЛИСТА В ОЧЕРЕДЬ :accordion:"
                    };

                    await _ctx.RespondAsync(_embed);
                    break;
                }
            }
            else
            {
                await _ctx.Channel.SendMessageAsync("Плейлист уже подрублен!");
                return;
            }
        }

        [Command("некст")] //Остановка текущего и воспроизведение следующего трека
        public async Task CmdNext(CommandContext _ctx, [RemainingText] string user_query)
        {
            if (Queue.IsCreated)
            {
                var _lava_node = Lava.Node().Result;
                var _lava_conn = Lava.Connect(_lava_node).Result;
                var _curr_lava_track = Queue.GetFirst;
                await _lava_conn.SeekAsync(_curr_lava_track.Length);
                await _ctx.Channel.SendMessageAsync($"Скипаю {_curr_lava_track.Title}");
            }
            else
            {
                await _ctx.RespondAsync("Хуекст");
            }
        }

        [Command("выводи")] //Скип текущего трека с удалением его из плейлиста
        public async Task CmdSkip(CommandContext _ctx)
        {
            //Скип текущего трека и удаление из плейлиста
            if (Queue.IsCreated
            && !Queue.IsEmpty)
            {
                Playlist.Delete(Queue.CurrentLink);
                var _lava_node = Lava.Node().Result;
                var _lava_conn = Lava.Connect(_lava_node).Result;
                var _curr_lava_track = Queue.GetFirst;
                await _lava_conn.SeekAsync(_curr_lava_track.Length);
                await _ctx.Channel.SendMessageAsync($"Выношу в помойку {_curr_lava_track.Title}");
            }
            else
            {
                await _ctx.RespondAsync("Себя выводи епта");
            }
        }

        [Command("заткнись")] //Остановка воспроизведения и выход из канала
        public async Task CmdLeave(CommandContext _ctx)
        {
            //Лава
            Lava.CreateCtx(_ctx);
            var _lava_node = Lava.Node().Result;
            var _lava_conn = Lava.Connect(_lava_node).Result;

            //Выход из канала
            Lava.ResetCtx();
            Queue.Clear();
            Playlist.Deactivate();
            await _lava_conn.StopAsync();
            await _lava_conn.DisconnectAsync();

            //Сообщение
            await _ctx.RespondAsync(":speak_no_evil:");
        }

        #endregion
    }
}
