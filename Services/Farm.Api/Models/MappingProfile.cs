using AutoMapper;
using Farm.Domain.Entities;
using Farm.Domain.ViewModels.Animal;
using Farm.Domain.ViewModels.Cage;
using Farm.Domain.ViewModels.Farm;
using Farm.Domain.ViewModels.Paging;

namespace Farm.Api.Models
{
    /// <summary>
    /// Mapping configuration of Models to Entity and vice versa
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Mapping profile Constructor
        /// </summary>
        public MappingProfile()
        {
            //Dto to entities
            CreateMap<CreateAnimalDto, Animal>();
            CreateMap<UpdateAnimalDto, Animal>();
            CreateMap<CreateCageDto, Cage>();
            CreateMap<UpdateCageDto, Cage>();
            CreateMap<CreateFarmDto, Domain.Entities.Farm>();
            CreateMap<UpdateFarmDto, Domain.Entities.Farm>();
            

            //entities to Dto           
            CreateMap<Animal, AnimalDetailsDto>();
            CreateMap<Cage, CageDetailsDto>()
                .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm.Name));
            CreateMap<Domain.Entities.Farm, FarmDetailsDto>();
        }
    }
}
