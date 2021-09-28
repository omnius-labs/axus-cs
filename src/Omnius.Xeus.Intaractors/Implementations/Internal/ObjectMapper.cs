using System;
using AutoMapper;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Intaractors.Internal.Entities;

namespace Omnius.Xeus.Intaractors.Internal
{
    internal static class ObjectMapper
    {
        private static readonly IMapper _mapper;

        static ObjectMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OmniHash, OmniHashEntity>().ConvertUsing(new OmniHashToEntityConverter());
            });
            _mapper = configuration.CreateMapper();
        }

        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        private sealed class OmniHashToEntityConverter : ITypeConverter<OmniHash, OmniHashEntity>
        {
            public OmniHashEntity Convert(OmniHash source, OmniHashEntity destination, ResolutionContext context)
            {
                return new OmniHashEntity(
                    (int)source.AlgorithmType,
                    context.Mapper.Map<ReadOnlyMemory<byte>, byte[]>(source.Value));
            }
        }
    }
}
