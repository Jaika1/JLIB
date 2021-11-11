using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace JLIB.Net
{
    /// <summary>
    /// Helper methods and properties to assist in performing advanced networking-related tasks based around interfaces, including:
    /// <list type="bullet">
    /// <item>Providing the default gateway for an interface</item>
    /// <item>Providing an endpoint for NAT-PMP communications</item>
    /// <item>Providing an endpoint for SSDP communications</item>
    /// </list>
    /// </summary>
    public sealed class NetInterfaceHelper
    {
        #region Static fields, properties and methods.
        /// <summary>
        /// A reference of the IPv4 SSDP multicast address used during instantiation to determine if an interface supports SSDP.
        /// </summary>
        private static readonly IPAddress ssdpMulticastAddress = IPAddress.Parse("239.255.255.250");

        /// <summary>
        /// Creates and returns a new <c>NetHelper</c> instance using the first network interface that is currently operational (up), excluding any loopback interfaces.
        /// </summary>
        /// <returns>A new <c>NetHelper</c> instance built using an interface that meets the aformentioned criteria.</returns>
        /// <exception cref="Exception">Thrown if no network connections are currently available, or if none of the available network interfaces meet the provided criteria.</exception>
        public static NetInterfaceHelper Create()
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

        /// <summary>
        /// Creates and returns a new <c>NetHelper</c> instance using interface <paramref name="netInterface"/>.
        /// </summary>
        /// <param name="netInterface">The network interface to create a helper for.</param>
        /// <returns>A new <c>NetHelper</c> instance built around <paramref name="netInterface"/>.</returns>
        public static NetInterfaceHelper CreateFromInterface(NetworkInterface netInterface)
            => new NetInterfaceHelper(netInterface);
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
        private NetInterfaceHelper(NetworkInterface netInterface)
        {
            _networkInterface = netInterface;
            IPInterfaceProperties ipInterfaceProperties = _networkInterface.GetIPProperties();

            _defaultGateway = ipInterfaceProperties.GatewayAddresses[0].Address;
            _natPmpEndPoint =  new IPEndPoint(_defaultGateway, 5351);

            bool hasSSDPMulticast = ipInterfaceProperties.MulticastAddresses.Count(a => a.Address != null && a.Address.Equals(ssdpMulticastAddress)) > 0;
            _ssdpEndPoint = hasSSDPMulticast ? new IPEndPoint(ssdpMulticastAddress, 1900) : null;
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
        /// Returns the end point of where Simple Service Discovery Porotcol (SSDP) data should be sent and received from (The SSDP multicast address on port 1900). If the SSDP multicast address is not defined within the provided interface, the returned value will be <c>null</c>.
        /// </summary>
        /// <remarks><i>"The clients send their request in the form of UDP packets to the port 5351 of the default gateway" - http://miniupnp.free.fr/nat-pmp.html</i></remarks>
        public IPEndPoint SSDPEndPoint =>
            _ssdpEndPoint;
    }
}
