using AutoMapper;
using Ethanol.Artifacts;
using System;

namespace Ethanol.Providers
{
    public class ArtifactMapper<RawIpfixRecord, TArtifact> : IArtifactMapper<RawIpfixRecord,TArtifact> where TArtifact : Artifact
    {
        Func<RawIpfixRecord, TArtifact> _mapper;
        public ArtifactMapper() 
        {
            var config = new MapperConfiguration(cfg => { cfg.AddMaps(typeof(TArtifact).Assembly); });
            var mapper = new Mapper(config); 
            _mapper = (src) => mapper.Map<TArtifact>(src);
        }

        public TArtifact Map(RawIpfixRecord src)
        {
            return _mapper(src);
        }
    }
}