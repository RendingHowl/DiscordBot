using System;
using System.IO;
using System.Collections.Generic;

namespace DiscordBot
{
    static class Playlist
    {
        static Playlist() 
        {
            if (created == false)
            {
                if (File.Exists(PATH))
                {
                    ReadFile();
                }

                created = true;
            }
        }

        public const int QUEUE_SIZE = 50;
        const string PATH = "playlist.txt";

        static bool active = false;
        static bool playing = false;
        static public List<string> queue_track_list = new List<string>();
        static List<string> saved_track_list = new List<string>();
        static List<string> buffer_track_list = new List<string>();
        static bool created = false;

        static public void Save(string _track)
        {
            if (saved_track_list.Count == 0)
            {
                saved_track_list.Add(_track);
                WriteFile(_track);
            }
            else
            {
                bool _add = true;

                for (int _i = 0; _i < saved_track_list.Count; ++_i)
                {
                    if (saved_track_list[_i] == _track)
                    {
                        _add = false;
                        break;
                    }
                }

                if (_add == true)
                {
                    saved_track_list.Add(_track);
                    WriteFile(_track);
                }
            }
        }

        static public bool Activate()
        {
            if (saved_track_list.Count != 0)
            {
                buffer_track_list.Clear();
                queue_track_list.Clear();

                for (int _i = 0; _i < QUEUE_SIZE; ++_i)
                {
                    buffer_track_list.Add(saved_track_list[_i]);
                }

                int _imax = buffer_track_list.Count;
                Random _rnd = new Random();

                for (int _i = 0; _i < _imax; ++_i)
                {
                    int _ind = _rnd.Next(buffer_track_list.Count);
                    queue_track_list.Add(buffer_track_list[_ind]);
                    buffer_track_list.RemoveAt(_ind);
                }

                active = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        static public void Deactivate()
        {
            queue_track_list.Clear();
            playing = false;
            active = false;
        }

        static public void Delete(string _line)
        {
            if (saved_track_list.Remove(_line))
            {
                RecreateFile();
            }
        }

        static async void ReadFile()
        {
            using (FileStream _fs = new FileStream(PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))

            using (StreamReader _reader = new StreamReader(_fs))
            {
                string _line;

                while ((_line = await _reader.ReadLineAsync()) != null)
                {
                    saved_track_list.Add(_line);
                }
            }
        }

        static async void WriteFile(string _line)
        {
            using (StreamWriter _writer = new StreamWriter(PATH, true))
            {
                await _writer.WriteLineAsync(_line);
            }
        }

        static async void RecreateFile()
        {
            using (StreamWriter _writer = new StreamWriter(PATH, false))
            {
                for (int _i = 0; _i < saved_track_list.Count; ++_i)
                {
                    string _new_line = saved_track_list[_i];
                    await _writer.WriteLineAsync(_new_line);
                }
            }
        }

        public static bool IsActive
        {
            get { return active; }
        }

        public static bool IsPlaying
        {
            get { return playing; }
            set { playing = value; }
        }
    }
}
