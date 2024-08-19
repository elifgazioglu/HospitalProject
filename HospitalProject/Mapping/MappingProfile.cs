using AutoMapper;
using HospitalProject.Controllers;
using HospitalProject.Models;

namespace HospitalProject.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserRequestModel>().ReverseMap();
            CreateMap<Doctor, DoctorRequestModel>().ReverseMap();
        }
    }
}