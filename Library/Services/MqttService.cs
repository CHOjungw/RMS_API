using Library.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Diagnostics;
using Library.Interfaces;
//using Library.Context;


using Library.Hub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;

namespace Library.Services
{
    public class MqttService
    {
        private IMqttClient _mqttClient;
        private ConcurrentDictionary<string, DateTime> _connectedDevices;
        private Timer _heartbeatTimer;
        private readonly ErrorViewModel _errorViewModel;
        private readonly ErrorLogHub _errorLogHub;              
        private readonly IServiceProvider _serviceProvider;
        public string Signaldevice = null;
        public MqttService(ErrorViewModel errorViewModel, ErrorLogHub errorLogHub, IServiceProvider serviceProvider)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _connectedDevices = new ConcurrentDictionary<string, DateTime>();            
            _errorViewModel = errorViewModel;
            _errorLogHub = errorLogHub;
            _serviceProvider = serviceProvider;
        }
        public async Task ConnectAsync()
        {

            if (_mqttClient == null)
            {
                Console.WriteLine("클라이언트가 접속하지 않았음");
                Debug.WriteLine("클라이언트 접속 안함");
            }
            else
            {
                Console.WriteLine("클라이언트 접속");
                Debug.WriteLine("클라이언트 접속 함");
            }

            var options = new MqttClientOptionsBuilder()
                .WithClientId("Server")
                .WithTcpServer("localhost", 1883) 
                .WithCleanSession()
                .Build();

            _mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Connected to MQTT broker.");

                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("devices/+/status").Build());
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("devices/+/heartbeat").Build());
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("devices/+/error").Build());
                Console.WriteLine("Subscribed to topic.");

                _heartbeatTimer = new Timer(CheckHeartbeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            });

            _mqttClient.UseDisconnectedHandler(e =>
            {
                Console.WriteLine("Disconnected from MQTT broker.");
            });

            _mqttClient.UseApplicationMessageReceivedHandler(async e =>
            {
            Console.WriteLine("Received message.");
            Console.WriteLine($"Topic: {e.ApplicationMessage.Topic}");
            Console.WriteLine($"Payload: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"QoS: {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"Retain: {e.ApplicationMessage.Retain}");

            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            var topicparts = topic.Split('/');
            if (topicparts.Length == 3 && topicparts[0] == "devices")
            {
                var deviceId = topicparts[1];
                    if (topicparts[2] == "status")
                    {
                        if (payload == "connected")
                        {
                            _errorLogHub.SendDeviceConnect(deviceId);
                        }
                        else if (payload == "disconnected")
                        {
                            _connectedDevices.TryRemove(deviceId, out _);
                        }
                        else
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                                var parts = payload.Split(",");
                                var topicParts = topic.Split("/");

                                var device = new Device
                                {
                                    AirV1 = parts[0],
                                    AirV2 = parts[1],
                                    HV = parts[2],
                                    WV = parts[3],
                                    DeviceName = topicParts[1],
                                    LastUpdated = DateTime.Now
                                }; 
                                context.Devices.Add(device);

                                if (Signaldevice == topicParts[1])
                                {
                                    _errorLogHub.SendDevice(device);
                                }
                                
                                await context.SaveChangesAsync();
                            }
                        }
                    }

                    else if (topicparts[2] == "heartbeat")
                    {
                        _connectedDevices[deviceId] = DateTime.UtcNow;
                    }
                    else if (topicparts[2] == "error")
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();


                            var payloadpart = payload.Split("\t");
                            DateTime date = DateTime.Now;
                            DateTime truncatedDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                            var _errorLog = new ErrorLog { Device = topicparts[1], ErrorMessage = payloadpart[0], Value = payloadpart[1], LogDateTime = truncatedDate };

                            await _errorLogHub.SendErrorLogAsync(_errorLog);
                            context.ErrorLogs.Add(_errorLog);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                Console.WriteLine($"Topic:{topic}, Payload:{payload}");
            });

            await _mqttClient.ConnectAsync(options);
        }

        public async Task PublishAsync(string device, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{device}/setvalue")
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            if (_mqttClient.IsConnected)
            {
                await _mqttClient.PublishAsync(message);
            }
        }

        public int GetConnectedDeviceCount()
        {
            return _connectedDevices.Count;
        }

        private void CheckHeartbeats(object state)
        {
            var now = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(60);

            foreach (var device in _connectedDevices.Keys)
            {
                if (_connectedDevices.TryGetValue(device, out var lastHeartbeat))
                {
                    if (now - lastHeartbeat > timeout)
                    {
                        Console.WriteLine($"Device {device} timed out.");
                        _connectedDevices.TryRemove(device, out _);
                    }
                }
            }
        }        
    }

}
