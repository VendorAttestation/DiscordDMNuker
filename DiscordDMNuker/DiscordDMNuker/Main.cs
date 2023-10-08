using Discord;
using DiscordDMNuker.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DiscordDMNuker
{
    public partial class Main : Form
    {
        private ToolStripStatusLabel Status;
        private ListBox Logs;
        private readonly static Random random = new Random();
        private readonly string currentPath = Directory.GetCurrentDirectory();
        private readonly string[] allowedExtensions =
        {
            ".mp4", ".avi", ".mov", ".mkv", ".flv", ".wmv", ".webm", // Video
            ".jpg", ".jpeg", ".png", ".gif", // Image
            ".mp3", ".wav", ".ogg", ".flac", ".m4a", // Audio
            ".txt", ".json", // Text
        };
        public Main()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(Path.Combine(currentPath, "SavedMedia")))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, "SavedMedia"));
            }

            if (!Directory.Exists(Path.Combine(currentPath, "SavedConvos")))
            {
                Directory.CreateDirectory(Path.Combine(currentPath, "SavedConvos"));
            }
        }
        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
        private async Task DownloadAndSaveAsync(string url, string currentPath, string fileName)
        {
            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string fullPath = Path.Combine(currentPath, "SavedMedia", fileName);
                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    Logs.SafeAddItem($"Saved Image/Video: {fileName}");
                }
                else
                {
                    // Handle the error appropriately, e.g.:
                    Logs.SafeAddItem($"Failed to download Image/Video: {fileName}. Status code: {response.StatusCode}");
                }
            }
        }
        private async void Start(string Token, ulong MainId, bool savepicsnvids, bool savemessages, bool delete, bool IsGroupChat, bool Edit, string MessageContent, bool IsChannel, ulong ChannelId)
        {
            await Task.Run(async () =>
            {
                try
                {
                    Status.SafeChangeText("Starting");
                    DiscordClient client = new DiscordClient(Token);
                    Logs.SafeAddItem($"Logged In To: {client.User.Username}");
                    if (!IsGroupChat && !IsChannel)
                    {
                        DiscordUser user = await client.GetUserAsync(MainId);
                        PrivateChannel channel = await client.CreateDMAsync(MainId);
                        Logs.SafeAddItem($"Created DMs With: {user.Username}");
                        var messages = await client.GetChannelMessagesAsync(channel.Id);
                        Status.SafeChangeText("In Progress....");
                        string convoPath = $"SavedConvos/{RemoveSpecialCharacters(user.Username)}{random.Next(1, 999999999)}.txt";

                        foreach (DiscordMessage message in messages)
                        {
                            if (savemessages)
                            {

                                using (StreamWriter writer = new StreamWriter(convoPath, true))
                                {
                                    writer.WriteLine($"{message.Author.User.Username} || {message.Content}");
                                }
                            }

                            if (savepicsnvids)
                            {
                                if (message.Attachments != null)
                                {
                                    foreach (var Attachment in message.Attachments)
                                    {
                                        if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                        {
                                            using var httpClient = new HttpClient();
                                            var fileName = $"{RandomString(4)}{Attachment.FileName}";
                                            await DownloadAndSaveAsync(Attachment.Url, currentPath, fileName);
                                        }
                                    }
                                }
                            }

                            if (delete || Edit)
                            {
                                if (message.Author.User.Id == client.User.Id && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                {
                                    if (delete)
                                    {
                                        await message.DeleteAsync();
                                        Logs.SafeAddItem($"Deleted Message: {message.Content}");
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }

                                    if (Edit)
                                    {
                                        await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                        Logs.SafeAddItem($"Edited Message: {message.Content}");
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                        }
                        Status.SafeChangeText("Completed");
                    }
                    else if (IsGroupChat)
                    {
                        DiscordChannel GroupChat = await client.GetChannelAsync(MainId);
                        if (GroupChat.Name != null)
                        {
                            Logs.SafeAddItem($"Hooked Group With Name: {GroupChat.Name}");
                            System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(GroupChat.Id);
                            Status.SafeChangeText("In Progress....");
                            string Convo = $"SavedConvos/{RemoveSpecialCharacters(GroupChat.Name)}{random.Next(1, 999_999_999)}.txt";
                            foreach (DiscordMessage message in msg)
                            {
                                if (savemessages)
                                {

                                    using (StreamWriter writer = new StreamWriter(Convo, true))
                                    {
                                        writer.WriteLine($"{message.Author.User.Username} || {message.Content}");
                                    }
                                }

                                if (savepicsnvids)
                                {
                                    if (message.Attachments != null)
                                    {
                                        foreach (var Attachment in message.Attachments)
                                        {
                                            if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                            {
                                                using var httpClient = new HttpClient();
                                                var fileName = $"{RandomString(4)}{Attachment.FileName}";
                                                await DownloadAndSaveAsync(Attachment.Url, currentPath, fileName);
                                            }
                                        }
                                    }
                                }

                                if (delete || Edit)
                                {
                                    if (message.Author.User.Id == client.User.Id && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                    {
                                        if (delete)
                                        {
                                            await message.DeleteAsync();
                                            Logs.SafeAddItem($"Deleted Message: {message.Content}");
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }

                                        if (Edit)
                                        {
                                            await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                            Logs.SafeAddItem($"Edited Message: {message.Content}");
                                            await Task.Delay(new Random().Next(2200, 2500));
                                        }
                                    }
                                }
                            }
                        }
                        Status.SafeChangeText("Completed");
                    }
                    else if (IsChannel)
                    {
                        DiscordChannel Channel = await client.GetChannelAsync(ChannelId);
                        System.Collections.Generic.IReadOnlyList<DiscordMessage> msg = await client.GetChannelMessagesAsync(Channel.Id);
                        Status.SafeChangeText("In Progress....");
                        string Convo = $"SavedConvos/{Channel.Name}{random.Next(1, 999_999_999)}.txt";
                        foreach (DiscordMessage message in msg)
                        {
                            if (savemessages)
                            {

                                using (StreamWriter writer = new StreamWriter(Convo, true))
                                {
                                    writer.WriteLine($"{message.Author.User.Username} || {message.Content}");
                                }
                            }

                            if (savepicsnvids)
                            {
                                if (message.Attachments != null)
                                {
                                    foreach (var Attachment in message.Attachments)
                                    {
                                        if (allowedExtensions.Any(ext => Attachment.FileName.Contains(ext)))
                                        {
                                            using var httpClient = new HttpClient();
                                            var fileName = $"{RandomString(4)}{Attachment.FileName}";
                                            await DownloadAndSaveAsync(Attachment.Url, currentPath, fileName);
                                        }
                                    }
                                }
                            }

                            if (delete || Edit)
                            {
                                if (message.Author.User.Id == client.User.Id && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.ChannelPinnedMessage && message.Type != MessageType.GuildBoostedTier1 && message.Type != MessageType.GuildBoostedTier2 && message.Type != MessageType.GuildBoostedTier3 && message.Type != MessageType.GuildMemberJoin && message.Type != MessageType.GuildBoosted && message.Type != MessageType.ThreadCreated && message.Type != MessageType.ThreadStarterMessage && message.Type != MessageType.RecipientAdd && message.Type != MessageType.RecipientRemove && message.Type != MessageType.Call)
                                {
                                    if (delete)
                                    {
                                        await message.DeleteAsync();
                                        Logs.SafeAddItem($"Deleted Message: {message.Content}");
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }

                                    if (Edit)
                                    {
                                        await message.EditAsync(new MessageEditProperties { Content = MessageContent });
                                        Logs.SafeAddItem($"Edited Message: {message.Content}");
                                        await Task.Delay(new Random().Next(2200, 2500));
                                    }
                                }
                            }
                        }
                        Status.SafeChangeText("Completed");
                    }
                    else
                    {
                        throw new Exception("Something went horribly wrong before i can continue, please check your internet connection, discord token and message parameters.");
                    }
                }
                catch { }
            });
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            using (FormStart FormStart = new FormStart())
            {
                FormStart.ShowDialog();
                if (FormStart.Start)
                {
                    Status = toolStripStatusLabel1;
                    Logs = listBox1;

                    Start(FormStart.Token, FormStart.MainId, FormStart.SavePicsNVids, FormStart.SaveMessages, FormStart.Delete, FormStart.IsGroupChat, FormStart.Edit, FormStart.Message, FormStart.IsChannel, FormStart.ChannelId);
                }
            }
        }
    }
}
