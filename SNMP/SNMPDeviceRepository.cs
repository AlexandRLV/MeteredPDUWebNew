using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace MeteredPDUWebNew.SNMP;

public class SNMPDeviceRepository : IDisposable
{
    private const int DevicesRequestInterval = 1000;

    public List<SNMPDevice> Devices { get; private set; }

    private OidParameters _oidParameters;
    private OctetString _community;
    private CancellationTokenSource _tokenSource;

    private ILogger<SNMPDeviceRepository> _logger;

    public SNMPDeviceRepository(ILogger<SNMPDeviceRepository> logger)
    {
        _logger = logger;
        // _logger.LogDebug("Starting snmp device repository");
        // using (var reader = new StreamReader(DevicesFileName))
        // {
        //     string json = reader.ReadToEnd();
        //     _logger.LogDebug($"SNMP Repository: readed {json}");
        //     _devicesInfo = JsonConvert.DeserializeObject<DevicesInfo>(json);
        //     _logger.LogDebug($"SNMP Repository: readed {Devices.Count} snmp devices from json");
        // }
        //
        // for (int i = 0; i < Devices.Count; i++)
        // {
        //     var device = Devices[i];
        //     device.Id = i;
        //     device.Initialize(_devicesInfo.Oids);
        // }
        // _logger.LogDebug($"SNMP Repository: initialized {Devices.Count} devices");
        Devices = new List<SNMPDevice>();

        _oidParameters = new OidParameters
        {
            AmperageOid = "1.3.6.1.2.1.1.7.0",
            VoltageOid = "1.3.6.1.2.1.17.1.3.0",
            PowerOid = "1.3.6.1.2.1.17.1.4.1.1.1",
            ReactivePowerOid = "1.3.6.1.2.1.17.1.4.1.2.1"
        };

        _community = new OctetString("public");
        _tokenSource = new CancellationTokenSource();
        Task.Run(() => Update(_tokenSource.Token));
    }

    public void Dispose()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
    }

    public void AddDevice(CreateDeviceViewModel model)
    {
        var device = new SNMPDevice
        {
            Name = model.Name,
            IPAddress = model.IpAddress,
            Port = model.Port,
            Id = Devices.Count,
        };

        device.Initialize(_oidParameters);
        Devices.Add(device);
    }

    public void DeleteDevice(int id)
    {
        if (id < 0 || id >= Devices.Count)
            return;

        Devices[id].MarkedAsDelete = true;
        Devices.RemoveAt(id);
        UpdateDevicesIds();
    }

    private async Task Update(CancellationToken token)
    {
        _logger.LogDebug("Started snmp device update");
        var tasks = new List<Task>();
        while (!token.IsCancellationRequested)
        {
            tasks.Clear();
            foreach (var device in Devices)
            {
                if (device.OnlineStatus == OnlineStatus.Offline)
                    continue;

                device.UpdateValues();
                // tasks.Add(UpdateDevice(device));
            }

            await Task.WhenAll(tasks);
            await Task.Delay(DevicesRequestInterval, token);
        }
        _logger.LogDebug("Stopped snmp device update");
    }

    private Task UpdateDevice(SNMPDevice device)
    {
        try
        {
            var result = Messenger.Get(
            VersionCode.V1,
            device.EndPoint,
            _community,
            device.RequestVariables,
            10000);

            device.OnlineStatus = OnlineStatus.Online;
            // device.ParseResponse(result);
            device.UpdateValues();
        }
        catch (Exception e)
        {
            device.OnlineStatus = OnlineStatus.Offline;
            device.SetToZero();
            _logger.LogError($"SNMP Repository: Caught an exception while updating device: {e}");
        }

        return Task.CompletedTask;
    }

    private void UpdateDevicesIds()
    {
        for (int i = 0; i < Devices.Count; i++)
        {
            Devices[i].Id = i;
        }
    }
}