using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BAChat
{
    namespace Protocol
    {
        class Send
        {
            public static void Login(string username_or_token, string password = null)
            {
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("command", "login");
                if (password == null && username_or_token.Length == 32)
                {
                    json.Add("token", username_or_token);
                    WebSocketClient.WSClient.Send(JsonConvert.SerializeObject(json));
                }
                else
                {
                    json.Add("username", username_or_token);
                    json.Add("password", password);
                    WebSocketClient.WSClient.Send(JsonConvert.SerializeObject(json));
                }
            }
            public static void Join(string channel)
            {
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("command", "join");
                if (channel != "")
                {
                    json.Add("channel", channel);
                    WebSocketClient.WSClient.Send(JsonConvert.SerializeObject(json));
                }
            }
            public static void Chat(string message)
            {
                Dictionary<string, string> json = new Dictionary<string, string>();
                json.Add("command", "join");
                if (message != "")
                {
                    json.Add("message", message);
                    WebSocketClient.WSClient.Send(JsonConvert.SerializeObject(json));
                }
            }
        }
        class Receive
        {
            public static bool Login(string json, out string token)
            {
                token = null;
                if (json != "")
                {
                    try
                    {
                        Dictionary<string, string> command = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (command.ContainsKey("command") && command["command"] == "login")
                        {
                            if (command.ContainsKey("token") && command["token"].Length == 32)
                            {
                                token = command["token"];
                            }
                            return true;
                        }
                    }
                    catch(JsonException e)
                    {
                        throw new NotImplementedException("SOmething needs to happen here");
                    }
                }
                return false;
            }
            public static bool Init(string json, out string username, out string channel)
            {
                username = null;
                channel = null;
                if (json != "")
                {
                    try
                    {
                        Dictionary<string, string> command = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (command.ContainsKey("command") && command["command"] == "init")
                        {
                            if (command.ContainsKey("username"))
                            {
                                username = command["username"];
                                if (command.ContainsKey("channel"))
                                {
                                    channel = command["channel"];
                                }
                                return true;
                            }
                        }
                    }
                    catch (JsonException e)
                    {
                        throw new NotImplementedException("SOmething needs to happen here");
                    }
                }
                return false;
            }
            public static bool Join(string json)
            {
                if (json != "")
                {
                    try
                    {
                        Dictionary<string, string> command = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (command.ContainsKey("command") && command["command"] == "join")
                        {
                            return true;
                        }
                    }
                    catch (JsonException e)
                    {
                        throw new NotImplementedException("SOmething needs to happen here");
                    }
                }
                return false;
            }
            public static bool Chat(string json, out string username, out string message)
            {
                username = null;
                message = null;
                if (json != "")
                {
                    try
                    {
                        Dictionary<string, string> command = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                        if (command.ContainsKey("command") && command["command"] == "chat" && command.ContainsKey("username") && command.ContainsKey("message"))
                        {
                            username = command["username"];
                            message = command["message"];
                            return true;
                        }
                    }
                    catch (JsonException e)
                    {
                        throw new NotImplementedException("SOmething needs to happen here");
                    }
                }
                return false;
            }
        }
    }
}
