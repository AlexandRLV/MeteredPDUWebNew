using System.Net;
using Lextm.SharpSnmpLib;
using Newtonsoft.Json;

namespace MeteredPDUWebNew.SNMP
{
    [JsonObject]
    public class SNMPDevice
    {
        public string OnlineBadgeClass => OnlineStatus == OnlineStatus.Online
                    ? "badge-online"
                    : OnlineStatus == OnlineStatus.Offline
                        ? "badge-offline"
                        : "badge-unknown";

        public string OnlineText => OnlineStatus == OnlineStatus.Online
                    ? "Онлайн"
                    : OnlineStatus == OnlineStatus.Offline
                        ? "Оффлайн"
                        : "Подключение...";

        [JsonProperty("ip_address")] public string IPAddress { get; set; }
        [JsonProperty("port")] public int Port { get; set; }
        [JsonProperty("name")] public string Name { get; set; }

        public double Amperage { get; set; }
        public double Voltage { get; set; }
        public double Power { get; set; }
        public double ReactivePower { get; set; }

        public OnlineStatus OnlineStatus { get; set; }

        public int Id { get; set; }
        public IPEndPoint EndPoint { get; private set; }
        public List<Variable> RequestVariables { get; private set; }

        public bool MarkedAsDelete { get; set; }

        public void Initialize(OidParameters oids)
        {
            OnlineStatus = OnlineStatus.Offline;

            EndPoint = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), 161);
            RequestVariables = new List<Variable>
            {
                new Variable(new ObjectIdentifier(oids.AmperageOid)),
                // new Variable(new ObjectIdentifier(oids.VoltageOid)),
                // new Variable(new ObjectIdentifier(oids.PowerOid)),
                // new Variable(new ObjectIdentifier(oids.ReactivePowerOid)),
            };
        }

        public void ParseResponse(IList<Variable> response)
        {
            Amperage = double.Parse(response[0].Data.ToString());
            //Voltage = double.Parse(response[1].Data.ToString());
            //Power = double.Parse(response[2].Data.ToString());
            //ReactivePower = double.Parse(response[3].Data.ToString());
        }

        public void UpdateValues()
        {
            var random = new Random();
            Amperage = Lerp(0.1, 0.4, random.NextDouble());
            Voltage = Lerp(218, 222, random.NextDouble());
            Power = Lerp(40, 50, random.NextDouble());
            ReactivePower = Lerp(30, 40, random.NextDouble());
        }

        public void SetToZero()
        {
            Amperage = 0;
            Voltage = 0;
            Power = 0;
            ReactivePower = 0;
        }

        private static double Lerp(double a, double b, double t)
        {
            return Math.Round(a + (b - a) * t, 2);
        }
    }
}