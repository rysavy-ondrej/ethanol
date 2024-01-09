using AutoMapper;
using Ethanol.DataObjects;
namespace Ethanol.ContextBuilder.Cleaners
{
    /// <summary>
    /// Provides utilities for mapping flow pairs into their bidirectional representations.
    /// </summary>
    /// <remarks>
    /// This class utilizes a configured mapper to transform a pair of flows (request and response) into a single bidirectional flow representation. 
    /// The specific mapping configurations are determined by the types of flows being mapped.
    /// </remarks>
    static class FlowPairMapper
    {
        /// <summary>
        /// Internal AutoMapper configuration to define the mappings between flow pairs and their bidirectional representations.
        /// </summary>
        static IMapper _mapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FlowPair<IpFlow>, IpFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets));

            cfg.CreateMap<FlowPair<DnsFlow>, DnsFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(d => d.Identifier, o => o.MapFrom(s => s.ResFlow.Identifier))
                .ForMember(d => d.QuestionCount, o => o.MapFrom(s => s.ResFlow.QuestionCount))
                .ForMember(d => d.AnswerCount, o => o.MapFrom(s => s.ResFlow.AnswerCount))
                .ForMember(d => d.AuthorityCount, o => o.MapFrom(s => s.ResFlow.AuthorityCount))
                .ForMember(d => d.AdditionalCount, o => o.MapFrom(s => s.ResFlow.AdditionalCount))
                .ForMember(d => d.ResponseType, o => o.MapFrom(s => s.ResFlow.ResponseType))
                .ForMember(d => d.ResponseClass, o => o.MapFrom(s => s.ResFlow.ResponseClass))
                .ForMember(d => d.ResponseTTL, o => o.MapFrom(s => s.ResFlow.ResponseTTL))
                .ForMember(d => d.ResponseName, o => o.MapFrom(s => s.ResFlow.ResponseName))
                .ForMember(d => d.ResponseCode, o => o.MapFrom(s => s.ResFlow.ResponseCode))
                .ForMember(d => d.ResponseData, o => o.MapFrom(s => s.ResFlow.ResponseData))
                .ForMember(d => d.QuestionType, o => o.MapFrom(s => s.ReqFlow.QuestionType))
                .ForMember(d => d.QuestionClass, o => o.MapFrom(s => s.ReqFlow.QuestionClass))
                .ForMember(d => d.QuestionName, o => o.MapFrom(s => s.ReqFlow.QuestionName))
                .ForMember(d => d.Flags, o => o.MapFrom(s => s.ReqFlow.Flags))
                .ForMember(d => d.Opcode, o => o.MapFrom(s => s.ReqFlow.Opcode))
                .ForMember(d => d.QueryResponseFlag, o => o.MapFrom(s => s.ReqFlow.QueryResponseFlag));

            cfg.CreateMap<FlowPair<HttpFlow>, HttpFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(d => d.Hostname, o => o.MapFrom(s => s.ReqFlow.Hostname))
                .ForMember(d => d.Method, o => o.MapFrom(s => s.ReqFlow.Method))
                .ForMember(d => d.ResultCode, o => o.MapFrom(s => s.ResFlow.ResultCode))
                .ForMember(d => d.Url, o => o.MapFrom(s => s.ReqFlow.Url))
                .ForMember(d => d.OperatingSystem, o => o.MapFrom(s => s.ReqFlow.OperatingSystem))
                .ForMember(d => d.ApplicationInformation, o => o.MapFrom(s => s.ReqFlow.ApplicationInformation));
            
            cfg.CreateMap<FlowPair<TlsFlow>, TlsFlow>()
                .ForMember(d => d.FlowType, o => o.MapFrom(s => FlowType.BidirectionFlow))
                .ForMember(d => d.SentOctets, o => o.MapFrom(s => s.ReqFlow.SentOctets))
                .ForMember(d => d.DestinationAddress, o => o.MapFrom(s => s.ReqFlow.DestinationAddress))
                .ForMember(d => d.DestinationPort, o => o.MapFrom(s => s.ReqFlow.DestinationPort))
                .ForMember(d => d.SentPackets, o => o.MapFrom(s => s.ReqFlow.SentPackets))
                .ForMember(d => d.Protocol, o => o.MapFrom(s => s.ReqFlow.Protocol))
                .ForMember(d => d.ApplicationTag, o => o.MapFrom(s => s.ReqFlow.ApplicationTag))
                .ForMember(d => d.SourceAddress, o => o.MapFrom(s => s.ReqFlow.SourceAddress))
                .ForMember(d => d.SourcePort, o => o.MapFrom(s => s.ReqFlow.SourcePort))
                .ForMember(d => d.TimeStart, o => o.MapFrom(s => s.ReqFlow.TimeStart))
                .ForMember(d => d.TimeDuration, o => o.MapFrom(s => s.ReqFlow.TimeDuration))
                .ForMember(d => d.RecvPackets, o => o.MapFrom(s => s.ResFlow.SentPackets))
                .ForMember(d => d.RecvOctets, o => o.MapFrom(s => s.ResFlow.SentOctets))

                .ForMember(h => h.IssuerCommonName, o => o.MapFrom(s => s.ResFlow.IssuerCommonName))
                .ForMember(h => h.SubjectCommonName, o => o.MapFrom(s => s.ResFlow.SubjectCommonName))
                .ForMember(h => h.SubjectOrganisationName, o => o.MapFrom(s => s.ResFlow.SubjectOrganisationName))
                .ForMember(h => h.CertificateValidityFrom, o => o.MapFrom(s => s.ResFlow.CertificateValidityFrom))
                .ForMember(h => h.CertificateValidityTo, o => o.MapFrom(s => s.ResFlow.CertificateValidityTo))
                .ForMember(h => h.SignatureAlgorithm, o => o.MapFrom(s => s.ResFlow.SignatureAlgorithm))
                .ForMember(h => h.PublicKeyAlgorithm, o => o.MapFrom(s => s.ResFlow.PublicKeyAlgorithm))
                .ForMember(h => h.PublicKeyLength, o => o.MapFrom(s => s.ResFlow.PublicKeyLength))
                .ForMember(h => h.ClientVersion, o => o.MapFrom(s => s.ReqFlow.ClientVersion))
                .ForMember(h => h.CipherSuites, o => o.MapFrom(s => s.ReqFlow.CipherSuites))
                .ForMember(h => h.ClientRandomID, o => o.MapFrom(s => s.ReqFlow.ClientRandomID))
                .ForMember(h => h.ClientSessionID, o => o.MapFrom(s => s.ReqFlow.ClientSessionID))
                .ForMember(h => h.ExtensionTypes, o => o.MapFrom(s => s.ReqFlow.ExtensionTypes))
                .ForMember(h => h.ExtensionLengths, o => o.MapFrom(s => s.ReqFlow.ExtensionLengths))
                .ForMember(h => h.EllipticCurves, o => o.MapFrom(s => s.ReqFlow.EllipticCurves))
                .ForMember(h => h.EcPointFormats, o => o.MapFrom(s => s.ReqFlow.EcPointFormats))
                .ForMember(h => h.ClientKeyLength, o => o.MapFrom(s => s.ReqFlow.ClientKeyLength))
                .ForMember(h => h.JA3Fingerprint, o => o.MapFrom(s => s.ReqFlow.JA3Fingerprint))
                .ForMember(h => h.ContentType, o => o.MapFrom(s => s.ResFlow.ContentType))
                .ForMember(h => h.HandshakeType, o => o.MapFrom(s => s.ResFlow.HandshakeType))
                .ForMember(h => h.SetupTime, o => o.MapFrom(s => s.ResFlow.SetupTime))
                .ForMember(h => h.ServerVersion, o => o.MapFrom(s => s.ResFlow.ServerVersion))
                .ForMember(h => h.ServerRandomID, o => o.MapFrom(s => s.ResFlow.ServerRandomID))
                .ForMember(h => h.ServerSessionID, o => o.MapFrom(s => s.ResFlow.ServerSessionID))
                .ForMember(h => h.ServerCipherSuite, o => o.MapFrom(s => s.ResFlow.ServerCipherSuite))
                .ForMember(h => h.ApplicationLayerProtocolNegotiation, o => o.MapFrom(s => s.ReqFlow.ApplicationLayerProtocolNegotiation));
        }).CreateMapper();

        /// <summary>
        /// Maps the provided flow pair into a single bidirectional flow representation.
        /// </summary>
        /// <typeparam name="T">The type of flow that must inherit from <see cref="IpFlow"/>.</typeparam>
        /// <param name="flowPair">The pair of request and response flows to be combined.</param>
        /// <returns>A bidirectional flow representation that combines the provided flow pair.</returns>
        /// <remarks>
        /// The method uses an internal mapper to transform the flow pair into a single flow representation 
        /// by combining the properties of both flows as needed. The type of the returned flow will be of the same type as the provided flows.
        /// </remarks>
        public static T Map<T>(FlowPair<T> flowPair) where T : IpFlow
        {
            return (T)_mapper.Map(flowPair, flowPair.GetType(), flowPair.ReqFlow.GetType());
        }

    }
}
