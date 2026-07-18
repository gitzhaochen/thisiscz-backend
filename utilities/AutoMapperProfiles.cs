using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using ThisisczApi.DTOs;
using ThisisczApi.Entities;

namespace ThisisczApi.Utilities;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        ConfigureMappings();
    }

    private void ConfigureMappings()
    {
        CreateMap<PostCreationDTO, Post>();
        CreateMap<Post, PostDTO>();
        CreateMap<IdentityUser, UserDTO>().ForMember(dest => dest.Role, opt => opt.Ignore()); // Role 在控制器中手动设置

        CreateMap<CommentCreationDTO, Comment>();
        CreateMap<Comment, CommentDTO>();

        CreateMap<LinkCreationDTO, Link>().ForMember(dest => dest.UserId, opt => opt.Ignore()); // 更新时不应改变创建者
        CreateMap<Link, LinkDTO>();

        CreateMap<School, SchoolDTO>();
        CreateMap<School, SchoolDetailDTO>();

        CreateMap<CarCreationDTO, Car>();
        CreateMap<Car, CarDTO>();
    }
}
