using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace DiscordBot
{
    static class Queue
    {
        static bool created = false;
        static List<LavalinkTrack> track_list = new List<LavalinkTrack>();

        static public bool Create(CommandContext _ctx)
        {
            if (created == false)
            {
                created = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        static public void Add(LavalinkTrack _track)
        {
            if (Playlist.IsPlaying)
            {
                track_list.Insert(1, _track);
            }
            else
            {
                track_list.Add(_track);
            }
        }

        static public async Task PlayNext(LavalinkGuildConnection _s, TrackFinishEventArgs _e)
        {
            track_list.RemoveAt(0);

            if (IsEmpty)
            {
                if (Playlist.IsActive)
                {
                    Playlist.Deactivate();
                }
            }
            else
            {
                var _lava_node = Lava.Node().Result;
                var _lava_conn = Lava.Connect(_lava_node).Result;
                await _lava_conn.PlayAsync(GetFirst);
            }
        }

        static public async Task PlayNextCmd()
        {
            track_list.RemoveAt(0);

            if (IsEmpty)
            {
                if (Playlist.IsActive)
                {
                    Playlist.Deactivate();
                }
            }
            else
            {
                CurrentLink = GetFirst.Uri.ToString();
                var _lava_node = Lava.Node().Result;
                var _lava_conn = Lava.Connect(_lava_node).Result;
                await _lava_conn.PlayAsync(GetFirst);
            }
        }

        static public void Clear()
        {
            track_list.Clear();
        }

        static public bool IsEmpty
        {
            get 
            {
                if (track_list.Count == 0)
                { return true; }
                else
                { return false; }
            }
        }

        static public bool IsCreated
        {
            get { return created; }
        }

        static public LavalinkTrack GetFirst
        {
            get { return track_list[0]; }
        }
        
        static public string CurrentLink { get; set; }
    }
}
