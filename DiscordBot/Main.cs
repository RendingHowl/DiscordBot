using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using Newtonsoft.Json;

namespace DiscordBot
{
    class Config
    {
        public string token { get; set; }
        public string prefix { get; set; }
        public string lava_host { get; set; }
        public int lava_port { get; set; }
        public string lava_password { get; set; }
        public bool lava_secured { get; set; }
    }

    class Program
    {
        const string CONFIG_PATH = "config.json";
        
        public static DiscordClient discord_client;

        static async Task Main(string[] args)
        {
            Config _config;

            if (File.Exists(CONFIG_PATH))
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_PATH));
            }
            else
            {
                _config = new Config()
                {
                    token = "",
                    prefix = "!",
                    lava_host = "127.0.0.1",
                    lava_port = 2333,
                    lava_password = "youshallnotpass",
                    lava_secured = false
                };

                File.WriteAllText(CONFIG_PATH, JsonConvert.SerializeObject(_config));
                Console.WriteLine($"Файл {CONFIG_PATH} создан. Надо закинуть токен бота и настройки лавалинка");
                Console.ReadKey();
            }

            //Дискорд
            var discord_client = new DiscordClient(new DiscordConfiguration()
            {
                Token = _config.token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
                MinimumLogLevel = LogLevel.Debug
            });

            var _discord_commands = discord_client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { _config.prefix }
            });

            _discord_commands.RegisterCommands<Commandos>();
            await discord_client.ConnectAsync();

            //Лавалинк
            var _lava_endpoint = new ConnectionEndpoint
            {
                Hostname = _config.lava_host,
                Port = _config.lava_port,
                Secured = _config.lava_secured
            };

            var _lava_config = new LavalinkConfiguration
            {
                Password = _config.lava_password,
                RestEndpoint = _lava_endpoint,
                SocketEndpoint = _lava_endpoint
            };

            var _lava_client = discord_client.UseLavalink();
            await _lava_client.ConnectAsync(_lava_config);

            //Блок
            await Task.Delay(-1);
        }
    }
}
