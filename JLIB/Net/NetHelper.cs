using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace JLIB.Net
{
    public sealed class NetHelper
    {
        #region Static fields, properties and methods.
        public static NetHelper Create()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                throw new Exception("No network connections are currently available.");

            IEnumerable<NetworkInterface> networkInterfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            if (networkInterfaces == null || networkInterfaces.Count() == 0)
                throw new Exception("No supported network interfaces were found.");

            NetworkInterface firstInterface = networkInterfaces.First();

            return CreateFromInterface(firstInterface);
        }

        public static NetHelper CreateFromInterface(NetworkInterface netInterface)
            => new NetHelper(netInterface);
        #endregion

        /// <summary>
        /// The network interface associated with this helper.
        /// </summary>
        private NetworkInterface _networkInterface;

        /// <summary>
        /// Cached instance to be fetched by the <c>DefaultGateway</c> property.
        /// </summary>
        /// <see cref="DefaultGateway"/>
        private IPAddress _defaultGateway;
        /// <summary>
        /// Cached instance to be fetched by the <c>NATPMPEndPoint</c> property.
        /// </summary>
        /// <see cref="NATPMPEndPoint"/>
        private IPEndPoint _natPmpEndPoint;
        /// <summary>
        /// Cached instance to be fetched by the <c>SSDPEndPoint</c> property.
        /// </summary>
        /// <see cref="SSDPEndPoint"/>
        private IPEndPoint _ssdpEndPoint;

        /// <summary>
        /// Constructor used by the <c>CreateFromInterface</c> method to create a new <c>NetHelper</c> instance bound to the specified network interface. Hidden, since going through the method is likely more descriptive, and the predicted infrequency of calls made to it means the performance impact of such a desicion should be neglegable.
        /// </summary>
        /// <param name="netInterface">The network interface to construct this helper around.</param>
        private NetHelper(NetworkInterface netInterface)
        {
            _networkInterface = netInterface;
            IPInterfaceProperties ipInterfaceProperties = _networkInterface.GetIPProperties();

            _defaultGateway = ipInterfaceProperties.GatewayAddresses[0].Address;
            _natPmpEndPoint =  new IPEndPoint(_defaultGateway, 5351);
            _ssdpEndPoint = new IPEndPoint(ipInterfaceProperties.MulticastAddresses.First().Address, 1900);
        }

        /// <summary>
        /// The default gateway for the provided network interface.
        /// </summary>
        public IPAddress DefaultGateway
            => _defaultGateway;

        /// <summary>
        /// Returns the end point of where Network Address Translation Port Mapping Protocol (NAT-PMP) data should be sent and received from. (The default gateway address on port 5351)
        /// </summary>
        /// <remarks><i>"The clients send their request in the form of UDP packets to the port 5351 of the default gateway" - http://miniupnp.free.fr/nat-pmp.html</i></remarks>
        public IPEndPoint NATPMPEndPoint =>
            _natPmpEndPoint;

        /// <summary>
        /// Returns the end point of where Simple Service Discovery Porotcol (SSDP) data should be sent and received from. (The multicast address on port 1900)
        /// </summary>
        /// <remarks><i>"The clients send their request in the form of UDP packets to the port 5351 of the default gateway" - http://miniupnp.free.fr/nat-pmp.html</i></remarks>
        public IPEndPoint SSDPEndPoint =>
            _ssdpEndPoint;
    }
}
